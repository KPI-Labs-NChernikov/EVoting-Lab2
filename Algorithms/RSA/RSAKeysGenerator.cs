using Algorithms.Abstractions;
using Algorithms.Common;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;

namespace Algorithms.RSA;
public sealed class RSAKeysGenerator : IMaskedAsymmetricAlgorithmKeysGenerator<RSAParameters>
{
    private const int s_keySize = 512;

    public Keys<RSAParameters> GenerateKeys()
    {
        var internalKeyPairGenerator = new RsaKeyPairGenerator();
        internalKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), s_keySize));
        var keyPair = internalKeyPairGenerator.GenerateKeyPair();
        var privateKey = (RsaPrivateCrtKeyParameters)keyPair.Private;
        var publicKey = (RsaKeyParameters)keyPair.Public;
        return new Keys<RSAParameters>(
            BouncyCastleRsaParametersMapper.BouncyToRSAParameters(publicKey),
            BouncyCastleRsaParametersMapper.BouncyToRSAParameters(privateKey));
    }

    public byte[] GenerateMaskMultiplier(RSAParameters publicKey)
    {
        var blindingFactorGenerator = new RsaBlindingFactorGenerator();
        blindingFactorGenerator.Init(BouncyCastleRsaParametersMapper.RSAParametersToBouncy(publicKey));

        var blindingFactor = blindingFactorGenerator.GenerateBlindingFactor();

        return blindingFactor.ToByteArray();
    }
}
