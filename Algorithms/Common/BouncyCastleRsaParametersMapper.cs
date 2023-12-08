using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;

namespace Algorithms.Common;
public static class BouncyCastleRsaParametersMapper
{
    public static RSAParameters BouncyToRSAParameters(RsaKeyParameters parameters)
    {
        if (!parameters.IsPrivate)
        {
            return new RSAParameters()
            {
                Exponent = parameters.Exponent.ToByteArray(),
                Modulus = parameters.Modulus.ToByteArray()
            };
        }

        var privateParameters = (RsaPrivateCrtKeyParameters)parameters;
        return new RSAParameters()
        {
            D = parameters.Exponent.ToByteArray(),
            DP = privateParameters.DP.ToByteArray(),
            DQ = privateParameters.DQ.ToByteArray(),
            Exponent = privateParameters.PublicExponent.ToByteArray(),
            InverseQ = privateParameters.QInv.ToByteArray(),
            Modulus = parameters.Modulus.ToByteArray(),
            P = privateParameters.P.ToByteArray(),
            Q = privateParameters.Q.ToByteArray()
        };
    }

    public static RsaKeyParameters RSAParametersToBouncy(RSAParameters parameters)
    {
        if (parameters.Exponent is null || parameters.Modulus is null)
        {
            throw new ArgumentException("The provided parameters are not an RSA key.", nameof(parameters));
        }

        if (parameters.D is null || parameters.DP is null || parameters.DQ is null || parameters.InverseQ is null || parameters.P is null || parameters.Q is null)
        {
            return new RsaKeyParameters(
            isPrivate: false,
            modulus: new BigInteger(parameters.Modulus),
            exponent: new BigInteger(parameters.Exponent));
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
}
