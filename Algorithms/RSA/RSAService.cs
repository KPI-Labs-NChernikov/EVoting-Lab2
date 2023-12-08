using Algorithms.Abstractions;
using Algorithms.Common;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;

namespace Algorithms.RSA;
public sealed class RSAService : IRSAService
{
    public byte[] Decrypt(byte[] data, RSAParameters privateKey)
    {
        var engine = new RsaEngine();
        engine.Init(false, BouncyCastleRsaParametersMapper.RSAParametersToBouncy(privateKey));

        return engine.ProcessBlock(data, 0, data.Length);
    }

    public byte[] DemaskSignature(byte[] data, RSAParameters publicKey, byte[] maskMultiplier)
    {
        var blindingParams = new RsaBlindingParameters(BouncyCastleRsaParametersMapper.RSAParametersToBouncy(publicKey), new BigInteger(maskMultiplier));

        var blindingEngine = new RsaBlindingEngine();
        blindingEngine.Init(false, blindingParams);

        return blindingEngine.ProcessBlock(data, 0, data.Length);
    }

    public byte[] Encrypt(byte[] data, RSAParameters publicKey)
    {
        var engine = new RsaEngine();
        engine.Init(true, BouncyCastleRsaParametersMapper.RSAParametersToBouncy(publicKey));

        return engine.ProcessBlock(data, 0, data.Length);
    }

    public byte[] Mask(byte[] data, RSAParameters publicKey, byte[] maskMultiplier)
    {
        var blindingParams = new RsaBlindingParameters(BouncyCastleRsaParametersMapper.RSAParametersToBouncy(publicKey), new BigInteger(maskMultiplier));

        var engine = new RsaBlindingEngine();

        engine.Init(true, blindingParams);

        return engine.ProcessBlock(data, 0, data.Length);
    }

    public byte[] SignHash(byte[] hash, RSAParameters privateKey)
    {
        var engine = new RsaEngine();
        engine.Init(true, BouncyCastleRsaParametersMapper.RSAParametersToBouncy(privateKey));

        return engine.ProcessBlock(hash, 0, hash.Length);
    }

    public bool VerifyHash(byte[] hash, byte[] signature, RSAParameters publicKey)
    {
        var engine = new RsaEngine();
        engine.Init(false, BouncyCastleRsaParametersMapper.RSAParametersToBouncy(publicKey));

        var decrypted = engine.ProcessBlock(signature, 0, signature.Length);

        return decrypted.AsSpan().SequenceEqual(hash);
    }
}
