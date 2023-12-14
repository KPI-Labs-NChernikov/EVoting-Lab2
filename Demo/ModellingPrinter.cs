using Algorithms.Abstractions;
using Modelling;
using System.Security.Cryptography;

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
        foreach (var (voter, candidateId) in votersWithCandidateIds)
        {
            var batchesCollection = voter.GenerateBallotBatches(commission.Candidates.Select(c => c.Id), commission.PublicKey, _rsaService, _objectToByteArrayTransformer, _rsaKeysGenerator);
            var signedBatch = commission.AcceptBatches(batchesCollection, _rsaService, _objectToByteArrayTransformer, _randomProvider);
            signedBatch.PrintErrorIfFailed();
            
            var finalBallot = voter.CreateFinalBallot(signedBatch.Value, candidateId, commission.PublicKey, _rsaService, _objectToByteArrayTransformer);
            finalBallot.PrintErrorIfFailed();

            var votingResult = commission.AcceptVote(finalBallot.Value, _rsaService, _objectToByteArrayTransformer);
        }
    }

    //public void PrintVotingWithIncorrectBallot(CentralElectionCommission commission)
    //{
    //    var ballot = new EncryptedSignedBallot(new byte[] { 4, 6, 8, 0 });
    //    var result = commission.AcceptBallot(ballot, _signatureProvider, _encryptionProvider, _objectToByteArrayTransformer);

    //    if (!result.IsSuccess)
    //    {
    //        PrintError(result);
    //    }
    //}

    //public void PrintVotingWithBallotSignedByThirdParty(CentralElectionCommission commission, int candidateId, int voterId)
    //{
    //    var keys = _asymmetricKeysGenerator.GenerateKeys();

    //    var ballot = new Ballot(voterId, candidateId);

    //    var ballotAsByteArray = _objectToByteArrayTransformer.Transform(ballot);
    //    var signedBallot = new SignedBallot(ballot, _signatureProvider.Sign(ballotAsByteArray, keys.PrivateKey), keys.PublicKey);

    //    var signedBallotAsByteArray = _objectToByteArrayTransformer.Transform(signedBallot);
    //    var encryptedSignedBallot = new EncryptedSignedBallot(_encryptionProvider.Encrypt(signedBallotAsByteArray, commission.BallotEncryptionKey));

    //    var result = commission.AcceptBallot(encryptedSignedBallot, _signatureProvider, _encryptionProvider, _objectToByteArrayTransformer);

    //    if (!result.IsSuccess)
    //    {
    //        PrintError(result);
    //    }
    //}

    //public void PrintVotingWithDoubleBallot(CentralElectionCommission commission, int candidateId, Voter voter)
    //{
    //    var ballot = voter.GenerateBallot(candidateId, commission.BallotEncryptionKey, _signatureProvider, _encryptionProvider, _objectToByteArrayTransformer);

    //    var result = commission.AcceptBallot(ballot, _signatureProvider, _encryptionProvider, _objectToByteArrayTransformer);

    //    if (!result.IsSuccess)
    //    {
    //        PrintError(result);
    //    }
    //}

    //public void PrintVotingResults(CentralElectionCommission commission)
    //{
    //    commission.CompleteVoting();

    //    var results = commission.VotingResults.CandidatesResults.OrderByVotes().ToList();
    //    foreach (var candidate in results)
    //    {
    //        Console.WriteLine($"{candidate.Candidate.FullName} (id: {candidate.Candidate.Id}): {candidate.Votes} votes");
    //    }
    //}

    //public void PrintVotingAfterCompletion(CentralElectionCommission commission, int candidateId, Voter voter)
    //{
    //    if (!commission.IsVotingCompleted)
    //    {
    //        commission.CompleteVoting();
    //    }

    //    var ballot = voter.GenerateBallot(candidateId, commission.BallotEncryptionKey, _signatureProvider, _encryptionProvider, _objectToByteArrayTransformer);

    //    var result = commission.AcceptBallot(ballot, _signatureProvider, _encryptionProvider, _objectToByteArrayTransformer);

    //    if (!result.IsSuccess)
    //    {
    //        PrintError(result);
    //    }
    //}
}
