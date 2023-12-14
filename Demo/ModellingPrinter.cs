using Algorithms.Abstractions;
using Modelling;
using Modelling.Extensions;
using System.Security.Cryptography;
using static Demo.UtilityMethods;

namespace Demo;
public sealed class ModellingPrinter
{
    private readonly IRSAService _rsaService;

    private readonly IMaskedAsymmetricAlgorithmKeysGenerator<RSAParameters> _rsaKeysGenerator;

    private readonly IRandomProvider _randomProvider;

    private readonly IObjectToByteArrayTransformer _objectToByteArrayTransformer;

    public ModellingPrinter(IRSAService rsaService, IMaskedAsymmetricAlgorithmKeysGenerator<RSAParameters> rsaKeysGenerator, IRandomProvider randomProvider, IObjectToByteArrayTransformer objectToByteArrayTransformer)
    {
        _rsaService = rsaService;
        _rsaKeysGenerator = rsaKeysGenerator;
        _randomProvider = randomProvider;
        _objectToByteArrayTransformer = objectToByteArrayTransformer;
    }

    public void PrintUsualVoting(CentralElectionCommission commission, Dictionary<Voter, int> votersWithCandidateIds)
    {
        Console.WriteLine("Usual voting:");

        foreach (var (voter, candidateId) in votersWithCandidateIds)
        {
            var batchesCollection = voter.GenerateBallotBatches(commission.Candidates.Select(c => c.Id), commission.PublicKey, _rsaService, _objectToByteArrayTransformer, _rsaKeysGenerator);
            var signedBatch = commission.AcceptBatches(batchesCollection, _rsaService, _objectToByteArrayTransformer, _randomProvider);
            if (signedBatch.IsFailed)
            {
                PrintError(signedBatch.ToResult());
                continue;
            }
            
            var finalBallot = voter.CreateFinalBallot(signedBatch.Value, candidateId, commission.PublicKey, _rsaService, _objectToByteArrayTransformer);
            if (finalBallot.IsFailed)
            {
                PrintError(finalBallot.ToResult());
                continue;
            }

            var votingResult = commission.AcceptVote(finalBallot.Value, _rsaService, _objectToByteArrayTransformer);
            if (votingResult.IsSuccess)
            {
                Console.WriteLine($"Voter {voter.Id} has casted their vote successfully.");
            }
        }

        Console.WriteLine();
    }

    public void PrintVotingWithIncorrectBallot(CentralElectionCommission commission)
    {
        Console.WriteLine("Voting with incorrect ballot:");
        var finalBallot = new byte[] { 4, 6, 8, 0 };
        var result = commission.AcceptVote(finalBallot, _rsaService, _objectToByteArrayTransformer);

        result.PrintErrorIfFailed();
        Console.WriteLine();
    }

    public void PrintVotingWithDoubleBallotCase1(CentralElectionCommission commission, Voter voter)
    {
        Console.WriteLine("Trying to vote two times (case 1: generate 2 ballots):");
        var batchesCollection = voter.GenerateBallotBatches(commission.Candidates.Select(c => c.Id), commission.PublicKey, _rsaService, _objectToByteArrayTransformer, _rsaKeysGenerator);
        var signedBatch1 = commission.AcceptBatches(batchesCollection, _rsaService, _objectToByteArrayTransformer, _randomProvider);
        if (signedBatch1.IsSuccess)
        {
            Console.WriteLine("Batch has been signed for the first time.");
        }

        var signedBatch2 = commission.AcceptBatches(batchesCollection, _rsaService, _objectToByteArrayTransformer, _randomProvider);
        signedBatch2.PrintErrorIfFailed();
        Console.WriteLine();
    }

    public void PrintVotingWithDoubleBallotCase2(CentralElectionCommission commission, int candidateId, Voter voter)
    {
        Console.WriteLine("Trying to vote two times (case 2: vote with same ballot two times):");
        var batchesCollection = voter.GenerateBallotBatches(commission.Candidates.Select(c => c.Id), commission.PublicKey, _rsaService, _objectToByteArrayTransformer, _rsaKeysGenerator);
        var signedBatch = commission.AcceptBatches(batchesCollection, _rsaService, _objectToByteArrayTransformer, _randomProvider);
        if (signedBatch.IsSuccess)
        {
            Console.WriteLine("Batch has been signed for the first time.");
        }
        var finalBallot = voter.CreateFinalBallot(signedBatch.Value, candidateId, commission.PublicKey, _rsaService, _objectToByteArrayTransformer);
        var votingResult1 = commission.AcceptVote(finalBallot.Value, _rsaService, _objectToByteArrayTransformer);
        if (votingResult1.IsSuccess)
        {
            Console.WriteLine("Vote has been accepted for the first time.");
        }

        var votingResult2 = commission.AcceptVote(finalBallot.Value, _rsaService, _objectToByteArrayTransformer);
        if (votingResult2.IsSuccess)
        {
            Console.WriteLine("Vote has been accepted for the second time.");
        }
        else
        {
            PrintError(votingResult2);
        }

        Console.WriteLine();
    }

    public void PrintVotingResults(CentralElectionCommission commission)
    {
        Console.WriteLine("Results:");
        commission.CompleteVoting();

        var results = commission.VotingResults;
        Console.WriteLine("Ballots:");
        foreach (var ballotResult in results.VotersResults)
        {
            Console.WriteLine($"Ballot {ballotResult.BallotId} Voter {ballotResult.VoterId} Candidate {ballotResult.CandidateId}");
        }
        Console.WriteLine("Candidates:");
        foreach (var candidate in results.CandidatesResults.Values.OrderByVotes())
        {
            Console.WriteLine($"{candidate.Candidate.FullName} (id: {candidate.Candidate.Id}): {candidate.Votes} votes");
        }
        Console.WriteLine();
    }

    public void PrintVotingAfterCompletion(CentralElectionCommission commission, int candidateId, Voter voter)
    {
        Console.WriteLine("Trying to vote after the completion of voting:");
        if (!commission.IsVotingCompleted)
        {
            commission.CompleteVoting();
        }

        var batchesCollection = voter.GenerateBallotBatches(commission.Candidates.Select(c => c.Id), commission.PublicKey, _rsaService, _objectToByteArrayTransformer, _rsaKeysGenerator);
        var signedBatch = commission.AcceptBatches(batchesCollection, _rsaService, _objectToByteArrayTransformer, _randomProvider);
        signedBatch.PrintErrorIfFailed();

        var fakeFinalBallot = new byte[] { 4, 6, 8, 0 };
        var result = commission.AcceptVote(fakeFinalBallot, _rsaService, _objectToByteArrayTransformer);
        result.PrintErrorIfFailed();

        Console.WriteLine();
    }
}
