using Algorithms.Abstractions;
using FluentResults;
using System.Security.Cryptography;

namespace Modelling;
public sealed class Voter
{
    public Guid Id { get; }

    public bool IsCapable { get; }

    private readonly byte[] _maskMultiplier;

    public Voter(Guid id, bool isCapable, RSAParameters centralElectionCommissionPublicKey, IMaskedAsymmetricAlgorithmKeysGenerator<RSAParameters> keysGenerator)
    {
        Id = id;
        IsCapable = isCapable;
        _maskMultiplier = keysGenerator.GenerateMaskMultiplier(centralElectionCommissionPublicKey);
    }

    public Result IsAbleToVote()
    {
        if (!IsCapable)
        {
            return Result.Fail($"Voter {Id} is not capable.");
        }

        return Result.Ok();
    }

    public byte[] GenerateBallotBatches(
        IEnumerable<int> candidatesIds, 
        RSAParameters centralElectionCommissionPublicKey, 
        IRSAService rsaService, 
        IObjectToByteArrayTransformer objectToByteTransformer)
    {
        const int batchesCount = 10;
        var batches = new BallotBatch[batchesCount];

        for (var i = 0; i < batchesCount; i++)
        {
            var maskedBallots = new List<byte[]>();
            foreach (var candidateId in candidatesIds)
            {
                var ballot = new Ballot(Id, candidateId);
                var ballotAsByteArray = objectToByteTransformer.Transform(ballot);
                var maskedBallot = rsaService.Mask(ballotAsByteArray, centralElectionCommissionPublicKey, _maskMultiplier);
                maskedBallots.Add(maskedBallot);
            }
            batches[i] = new BallotBatch(maskedBallots);
        }

        var result = new BallotBatchesCollection(batches, _maskMultiplier);

        return rsaService.Encrypt(objectToByteTransformer.Transform(result), centralElectionCommissionPublicKey);
    }

    public Result<byte[]> Vote(
        IEnumerable<byte[]> signedBallots, 
        int candidateId,
        RSAParameters centralElectionCommissionPublicKey,
        IRSAService rsaService,
        IObjectToByteArrayTransformer objectToByteTransformer)
    {
        return Result.Ok()
            .Bind(() => FindAndDemaskSignedBallot(signedBallots, candidateId, centralElectionCommissionPublicKey, rsaService, objectToByteTransformer))
            .Bind(sb => EncryptSignedBallot(sb, centralElectionCommissionPublicKey, rsaService, objectToByteTransformer));
    }

    private Result<SignedBallot> FindAndDemaskSignedBallot(
        IEnumerable<byte[]> signedBallots,
        int candidateId,
        RSAParameters centralElectionCommissionPublicKey,
        IRSAService rsaService,
        IObjectToByteArrayTransformer objectToByteTransformer)
    {
        return Result.Try(() =>
        {
            foreach (var signedBallot in signedBallots)
            {
                var signature = rsaService.DemaskSignature(signedBallot, centralElectionCommissionPublicKey, _maskMultiplier);
                var ballotAsByteArray = rsaService.Decrypt(signature, centralElectionCommissionPublicKey);
                var ballot = objectToByteTransformer.ReverseTransform<Ballot>(ballotAsByteArray);

                if (ballot is not null && ballot.VoterId == Id && ballot.CandidateId == candidateId)
                {
                    return new SignedBallot(ballot, signature);
                }
            }
            throw new ArgumentException("Not able to find the right ballot.");
        }, e => new Error("Message has wrong format or was incorrectly encrypted.").CausedBy(e));
    }

    private static Result<byte[]> EncryptSignedBallot(
        SignedBallot signedBallot,
        RSAParameters centralElectionCommissionPublicKey,
        IRSAService rsaService,
        IObjectToByteArrayTransformer objectToByteTransformer)
    {
        var signedBallotAsByteArray = objectToByteTransformer.Transform(signedBallot);
        var encryptedSignedBallot = rsaService.Encrypt(signedBallotAsByteArray, centralElectionCommissionPublicKey);
        return Result.Ok(encryptedSignedBallot);
    }
}
