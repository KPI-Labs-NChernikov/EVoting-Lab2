using Algorithms.Abstractions;
using Algorithms.Common;
using FluentResults;
using Modelling.Entities;
using Modelling.Models;
using System.Security.Cryptography;

namespace Modelling;
public sealed class CentralElectionCommission
{
    private readonly Dictionary<int, Candidate> _candidates = new();
    private readonly Dictionary<Guid, Voter> _voters = new();

    public VotingResults VotingResults { get; }

    private readonly Dictionary<Guid, VoterStatus> _votersStatuses = new();

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
            _votersStatuses.Add(voter.Id, VoterStatus.NotAttended);
        }

        (PublicKey, _privateKey) = keysGenerator.GenerateKeys();
    }

    public Result<IEnumerable<byte[]>> AcceptBatches(byte[] collection, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteTransformer, IRandomProvider randomProvider)
    {
        return CheckIfVotingIsCompleted()
            .Bind(() => DecryptBallotBatchesCollection(collection, rsaService, objectToByteTransformer))
            .Bind(c => CheckBatches())
    }

    private Result<BallotBatchesCollection> DecryptBallotBatchesCollection(byte[] collection, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteArrayTransformer)
    {
        return Result.Try(()
            => objectToByteArrayTransformer.ReverseTransform<BallotBatchesCollection>(rsaService.Decrypt(collection, _privateKey))
                ?? throw new InvalidOperationException("Value cannot be transformed to ballot batches collection ballot."),
            e => new Error("Message has wrong format or was incorrectly encrypted.").CausedBy(e));
    }

    private Result<IEnumerable<byte[]>> CheckBatches(BallotBatchesCollection collection, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteArrayTransformer, IRandomProvider randomProvider)
    {
        var skipBatch = randomProvider.NextItem(collection.Batches);
        foreach (var batch in collection.Batches) 
        {
            if (batch == skipBatch)
            {
                continue;
            }

            var batchCheckResult = Result.Ok()
                .Bind();
        }
    }

    private Result<BallotBatch> CheckBatch(BallotBatch batch, byte[] maskMultiplier, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteArrayTransformer)
    {
        foreach (var maskedBallot in batch.MaskedBallots)
        {

        }
    }

    public Result<int> AcceptVote(byte[] encryptedSignedBallot, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteTransformer)
    {
        return CheckIfVotingIsCompleted()
            .Bind(() => DecryptSignedBallot(encryptedSignedBallot, rsaService, objectToByteTransformer))
            .Bind(sb => VerifySignature(sb, rsaService, objectToByteTransformer))
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

    private Result<Ballot> VerifySignature(SignedBallot signedBallot, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteArrayTransformer)
    {
        var signatureIsAuthentic = rsaService.VerifyHash(objectToByteArrayTransformer.Transform(signedBallot.Ballot), signedBallot.Signature, PublicKey);

        if (!signatureIsAuthentic)
        {
            return Result.Fail(new Error("The signature is not authentic."));
        }

        return Result.Ok(signedBallot.Ballot);
    }

    private Result<Ballot> VerifyVoterWhileVoting(Ballot ballot)
    {
        var voterWasFound = _voters.TryGetValue(ballot.VoterId, out var voter);
        if (!voterWasFound)
        {
            return Result.Fail(new Error("The voter was not found."));
        }

        var voterHasVoted = _votersStatuses[ballot.VoterId] == VoterStatus.Voted;
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
