using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;

namespace Algorithms.Common;
public static class BouncyCastleRsaParametersMapper
{
    public static RSAParameters BouncyPrivateToRSAParameters(RsaPrivateCrtKeyParameters parameters)
    {
        return new RSAParameters()
        {
            D = parameters.Exponent.ToByteArray(),
            DP = parameters.DP.ToByteArray(),
            DQ = parameters.DQ.ToByteArray(),
            Exponent = parameters.PublicExponent.ToByteArray(),
            InverseQ = parameters.QInv.ToByteArray(),
            Modulus = parameters.Modulus.ToByteArray(),
            P = parameters.P.ToByteArray(),
            Q = parameters.Q.ToByteArray()
        };
    }

    public static RSAParameters BouncyPublicToRSAParameters(RsaKeyParameters parameters)
    {
        return new RSAParameters()
        {
            Exponent = parameters.Exponent.ToByteArray(),
            Modulus = parameters.Modulus.ToByteArray()
        };
    }

    public static RsaPrivateCrtKeyParameters RSAParametersToBouncyPrivate(RSAParameters parameters)
    {
        if (parameters.D is null || parameters.DP is null || parameters.DQ is null || parameters.InverseQ is null || parameters.P is null || parameters.Q is null)
        {
            throw new ArgumentException("The provided parameters are not a private key.", nameof(parameters));
        }

        if (parameters.Exponent is null || parameters.Modulus is null)
        {
            throw new ArgumentException("The provided parameters are not an RSA key.", nameof(parameters));
        }

        return new RsaPrivateCrtKeyParameters(
            modulus: new BigInteger(parameters.Modulus),
            publicExponent: new BigInteger(parameters.Exponent),
            privateExponent: new BigInteger(parameters.D),
            p: new BigInteger(parameters.P),
            q: new BigInteger(parameters.Q),
            dP: new BigInteger(parameters.DP),
            dQ: new BigInteger(parameters.DQ),
            qInv: new BigInteger(parameters.InverseQ));
    }

    public static RsaKeyParameters RSAParametersToBouncyPublic(RSAParameters parameters)
    {
        if (parameters.Exponent is null || parameters.Modulus is null)
        {
            throw new ArgumentException("The provided parameters are not an RSA key.", nameof(parameters));
        }

        return new RsaKeyParameters(
            isPrivate: false,
            modulus: new BigInteger(parameters.Modulus),
            exponent: new BigInteger(parameters.Exponent));
    }
}
