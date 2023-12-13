using Algorithms.Abstractions;
using FluentResults;
using System.Security.Cryptography;

namespace Modelling;
public sealed class SignedBallot
{
    public Ballot Ballot { get; }

    public byte[] Signature { get; }

    public SignedBallot(Ballot ballot, byte[] signature)
    {
        Ballot = ballot;
        Signature = signature;
    }

    public Result VerifySignature(RSAParameters publicKey, IRSAService rsaService, IObjectToByteArrayTransformer objectToByteArrayTransformer)
    {
        var signatureIsAuthentic = rsaService.VerifyHash(objectToByteArrayTransformer.Transform(Ballot), Signature, publicKey);

        if (!signatureIsAuthentic)
        {
            return Result.Fail(new Error("The signature is not authentic."));
        }

        return Result.Ok();
    }
}
