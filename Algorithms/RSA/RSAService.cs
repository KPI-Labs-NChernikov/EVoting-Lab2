using Algorithms.Abstractions;
using Algorithms.Common;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;

namespace Algorithms.RSA;
public sealed class RSAService : IRSAService
{
    private static IDigest CreateDigest() => new Sha256Digest();
    private const int s_saltLength = 20;

    public byte[] Decrypt(byte[] data, RSAParameters privateKey)
    {
        var engine = new RsaEngine();
        engine.Init(false, BouncyCastleRsaParametersMapper.RSAParametersToBouncyPrivate(privateKey));

        return engine.ProcessBlock(data, 0, data.Length);
    }

    public byte[] DemaskSignature(byte[] data, RSAParameters publicKey, byte[] maskMultiplier)
    {
        var blindingParams = new RsaBlindingParameters(BouncyCastleRsaParametersMapper.RSAParametersToBouncyPublic(publicKey), new BigInteger(maskMultiplier));

        var blindingEngine = new RsaBlindingEngine();
        blindingEngine.Init(false, blindingParams);

        return blindingEngine.ProcessBlock(data, 0, data.Length);
    }

    public byte[] Encrypt(byte[] data, RSAParameters publicKey)
    {
        var engine = new RsaEngine();
        engine.Init(true, BouncyCastleRsaParametersMapper.RSAParametersToBouncyPublic(publicKey));

        return engine.ProcessBlock(data, 0, data.Length);
    }

    public byte[] Mask(byte[] data, RSAParameters publicKey, byte[] maskMultiplier)
    {
        var blindingParams = new RsaBlindingParameters(BouncyCastleRsaParametersMapper.RSAParametersToBouncyPublic(publicKey), new BigInteger(maskMultiplier));

        var signer = new PssSigner(new RsaBlindingEngine(), CreateDigest(), s_saltLength);

        signer.Init(true, blindingParams);

        signer.BlockUpdate(data, 0, data.Length);

        return signer.GenerateSignature(); // get signature ready to sign
    }

    public byte[] SignHash(byte[] hash, RSAParameters privateKey)
    {
        var engine = new RsaEngine();
        engine.Init(true, BouncyCastleRsaParametersMapper.RSAParametersToBouncyPrivate(privateKey));

        return engine.ProcessBlock(hash, 0, hash.Length);
    }

    public bool Verify(byte[] data, byte[] signature, RSAParameters publicKey)
    {
        var signer = new PssSigner(new RsaEngine(), CreateDigest(), s_saltLength);
        signer.Init(false, BouncyCastleRsaParametersMapper.RSAParametersToBouncyPublic(publicKey));

        signer.BlockUpdate(data, 0, data.Length);

        return signer.VerifySignature(signature);
    }
}
