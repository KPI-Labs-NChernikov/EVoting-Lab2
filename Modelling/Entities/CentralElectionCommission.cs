﻿using Algorithms.Abstractions;
using FluentResults;
using Modelling.Entities;
using Modelling.Models;
using System.Security.Cryptography;

namespace Modelling;
public sealed class CentralElectionCommission
{
    private readonly Dictionary<int, Candidate> _candidates = [];
    private readonly Dictionary<Guid, Voter> _voters = [];

    public VotingResults VotingResults { get; }
    public IReadOnlyList<Candidate> Candidates => _candidates.Values.OrderBy(x => x.Id).ToList();

    private readonly Dictionary<Guid, VotingAttendanceStatus> _votersStatuses = [];

    public RSAParameters PublicKey { get; }
    private readonly RSAParameters _privateKey;

    public bool IsVotingCompleted { get; private set; }

    public CentralElectionCommission(IEnumerable<Candidate> candidates, IEnumerable<Voter> voters, IMaskedAsymmetricAlgorithmKeysGenerator<RSAParameters> keysGenerator)
    {
        VotingResults = new();

        foreach (var candidate in candidates)
        {
            _candidates.Add(candidate.Id, candidate);
            VotingResults.CandidatesResults.Add(candidate.Id, new (candidate));
        }

        foreach (var voter in voters)
        {
            _voters.Add(voter.Id, voter);
            _votersStatuses.Add(voter.Id, VotingAttendanceStatus.NotAttended);
        }

        (PublicKey, _privateKey) = keysGenerator.GenerateKeys();
    }

    public Result<IReadOnlyCollection<byte[]>> AcceptBatches(BallotBatchesCollection collection, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteTransformer, IRandomProvider randomProvider)
    {
        return CheckIfVotingIsCompleted()
            .Bind(() => CheckBatches(collection, rsaService, objectToByteTransformer, randomProvider))
            .Bind(r => MarkVoterAsReceivedBallot(r.voterId).ToResult(r.ballotBatch))
            .Bind(b => SignBatch(b, rsaService));
    }

    private Result<(BallotBatch ballotBatch, Guid voterId)> CheckBatches(BallotBatchesCollection ballotBatches, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteTransformer, IRandomProvider randomProvider)
    {
        var skipBatch = randomProvider.NextItem(ballotBatches);
        Guid? expectedVoterId = null;
        foreach (var batch in ballotBatches)
        {
            if (batch == skipBatch)
            {
                continue;
            }

            var batchVerificationResult = CheckBatch(batch, expectedVoterId, ballotBatches.MaskMultiplier, rsaService, objectToByteTransformer);

            if (batchVerificationResult.IsFailed)
            {
                return batchVerificationResult.ToResult();
            }

            expectedVoterId ??= batchVerificationResult.Value;
        }
        return (skipBatch, expectedVoterId!.Value);
    }

    private Result<Guid> CheckBatch(BallotBatch batch, Guid? expectedVoterId, byte[] maskMultiplier, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteTransformer)
    {
        return DemaskBallots(batch, maskMultiplier, rsaService, objectToByteTransformer)
            .Bind(VerifyBatchCandidates)
            .Bind(b => CheckBatchVoter(b, expectedVoterId));
    }

    private Result<IReadOnlyList<Ballot>> DemaskBallots(BallotBatch batch, byte[] maskMultiplier, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteTransformer)
    {
        return Result.Try<IReadOnlyList<Ballot>>(() => batch.MaskedBallots.Select(b =>
        {
            var temporarilySigned = rsaService.SignHash(b, _privateKey);
            var demaskedSignature = rsaService.DemaskSignature(temporarilySigned, PublicKey, maskMultiplier);
            var decryptedBallot = rsaService.Decrypt(demaskedSignature, PublicKey);
            var ballot = objectToByteTransformer.ReverseTransform<Ballot>(decryptedBallot);
            return ballot ?? throw new InvalidOperationException("Value cannot be demasked.");
        }).ToList(), e => new Error("Message has wrong format or was incorrectly encrypted.").CausedBy(e));
    }

    private Result<IReadOnlyList<Ballot>> VerifyBatchCandidates(IReadOnlyList<Ballot> ballots)
    {
        return Result.Ok(ballots)
            .Bind(b => b.Count == _candidates.Keys.Count 
                ? Result.Ok(b) : Result.Fail<IReadOnlyList<Ballot>>("Quantity of batches is not equal to quantity of candidates."))
            .Bind(b => b.Select(x => x.CandidateId).ToHashSet().SetEquals(_candidates.Keys.ToHashSet())
                ? Result.Ok(b) : Result.Fail<IReadOnlyList<Ballot>>("Ballot batches contain candidates that are not enlisted or duplicate candidates."));
    }

    private Result<Guid> CheckBatchVoter(IReadOnlyList<Ballot> ballots, Guid? expectedVoterId)
    {
        expectedVoterId ??= ballots.FirstOrDefault()?.VoterId;

        if (expectedVoterId is null || ballots.Any(b => b.VoterId != expectedVoterId))
        {
            return Result.Fail(new Error("Not all ballots have the same voter."));
        }

        if (!_voters.TryGetValue(expectedVoterId.Value, out var voter))
        {
            return Result.Fail(new Error("Voter was not found."));
        }

        var voterAbility = voter.IsAbleToVote();
        if (voterAbility.IsFailed)
        {
            return voterAbility;
        }

        var voterHasSentBallots = _votersStatuses[expectedVoterId.Value] is VotingAttendanceStatus.ReceivedBallot or VotingAttendanceStatus.Voted;
        if (voterHasSentBallots)
        {
            return Result.Fail(new Error("The voted has already received signed ballots."));
        }

        return Result.Ok(expectedVoterId.Value);
    }

    private Result MarkVoterAsReceivedBallot(Guid voterId)
    {
        _votersStatuses[voterId] = VotingAttendanceStatus.ReceivedBallot;
        return Result.Ok();
    }

    private Result<IReadOnlyCollection<byte[]>> SignBatch(BallotBatch ballotBatch, IRSAService rsaService)
    {
        return Result.Ok<IReadOnlyCollection<byte[]>>(ballotBatch.MaskedBallots.Select(b => rsaService.SignHash(b, _privateKey)).ToList());
    }

    public Result<int> AcceptVote(byte[] encryptedSignedBallot, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteTransformer)
    {
        return CheckIfVotingIsCompleted()
            .Bind(() => DecryptSignedBallot(encryptedSignedBallot, rsaService, objectToByteTransformer))
            .Bind(sb => 
            { 
                var isSignatureAuthentic = sb.VerifySignature(PublicKey, rsaService, objectToByteTransformer);
                return isSignatureAuthentic.ToResult(sb.Ballot);
            })
            .Bind(VerifyVoterWhileVoting)
            .Bind(VerifyCandidate)
            .Bind(AddVote);
    }

    private Result CheckIfVotingIsCompleted()
    {
        return Result.FailIf(IsVotingCompleted, new Error("The voting is already completed."));
    }


    private Result<SignedBallot> DecryptSignedBallot(byte[] encryptedSignedBallot, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteArrayTransformer)
    {
        return Result.Try(()
            => objectToByteArrayTransformer.ReverseTransform<SignedBallot>(rsaService.Decrypt(encryptedSignedBallot, _privateKey))
                ?? throw new InvalidOperationException("Value cannot be transformed to signed ballot."),
            e => new Error("Message has wrong format or was incorrectly encrypted.").CausedBy(e));
    }

    private Result<Ballot> VerifyVoterWhileVoting(Ballot ballot)
    {
        var voterWasFound = _voters.ContainsKey(ballot.VoterId);
        if (!voterWasFound)
        {
            return Result.Fail(new Error("The voter was not found."));
        }

        var voterHasVoted = _votersStatuses[ballot.VoterId] == VotingAttendanceStatus.Voted;
        if (voterHasVoted)
        {
            return Result.Fail(new Error("The voted has already casted a vote."));
        }

        return Result.Ok(ballot);
    }

    private Result<Ballot> VerifyCandidate(Ballot ballot)
    {
        var candidateWasFound = _candidates.ContainsKey(ballot.CandidateId);
        if (!candidateWasFound)
        {
            return Result.Fail(new Error("Candidate was not found."));
        }

        return Result.Ok(ballot);
    }

    private Result<int> AddVote(Ballot ballot)
    {
        _votersStatuses[ballot.VoterId] = VotingAttendanceStatus.Voted;

        VotingResults.CandidatesResults[ballot.CandidateId].Votes++;
        var ballotNumber = VotingResults.VotersResults.Count + 1;
        VotingResults.VotersResults.Add(new(ballot.VoterId, ballotNumber, ballot.CandidateId));

        return Result.Ok(ballotNumber);
    }

    public void CompleteVoting()
    {
        IsVotingCompleted = true;
    }
}
