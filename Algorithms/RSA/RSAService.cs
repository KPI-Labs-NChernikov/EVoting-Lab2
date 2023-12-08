//using Algorithms.Abstractions;
//using System.Numerics;
//using System.Security.Cryptography;

//namespace Algorithms.RSA;
//public sealed class RSAService : IRSAService
//{
//    public byte[] Sign(byte[] data, byte[] privateKey)
//    {
//        using var rsa = System.Security.Cryptography.RSA.Create();
//        rsa.ImportRSAPrivateKey(privateKey, out _);
//        var key = rsa.ExportParameters
//        if (_hashAlgorithmName.Name != s_noHashAlgorithmName)
//        {
//            return rsa.SignData(data, _hashAlgorithmName, _signaturePadding);
//        }

//        return rsa.SignHash(data, _hashAlgorithmName, _signaturePadding);
//    }

//    public bool Verify(byte[] data, byte[] signature, byte[] publicKey)
//    {
//        using var rsa = System.Security.Cryptography.RSA.Create();
//        rsa.ImportRSAPublicKey(publicKey, out _);

//        if (_hashAlgorithmName.Name != s_noHashAlgorithmName)
//        {
//            return rsa.VerifyData(data, signature, _hashAlgorithmName, _signaturePadding);
//        }

//        return rsa.VerifyHash(data, signature, _hashAlgorithmName, _signaturePadding);
//    }

//    public byte[] Mask(byte[] data, byte[] publicKey, byte[] maskMultiplier)
//    {
//        using var rsa = System.Security.Cryptography.RSA.Create();
//        rsa.ImportRSAPublicKey(publicKey, out _);

//        var parameters = rsa.ExportParameters(includePrivateParameters: false);

//        var dataAsNumber = new BigInteger(data, true, true);
//        var maskMultiplierAsNumber = new BigInteger(maskMultiplier, true, true);
//        var eAsNumber = new BigInteger(parameters.Exponent, true, true);
//        var nAsNumber = new BigInteger(parameters.Modulus, true, true);

//        var maskedDataAsNumber = (dataAsNumber * BigInteger.Pow(maskMultiplierAsNumber, (int)eAsNumber)) % nAsNumber;
//        return maskMultiplierAsNumber.ToByteArray(true, true);
//    }

//    public byte[] DemaskSignature(byte[] data, byte[] publicKey, byte[] maskMultiplier)
//    {
//        using var rsa = System.Security.Cryptography.RSA.Create();
//        rsa.ImportRSAPublicKey(publicKey, out _);

//        var parameters = rsa.ExportParameters(includePrivateParameters: false);

//        var dataAsNumber = new BigInteger(data, true, true);
//        var maskMultiplierAsNumber = new BigInteger(maskMultiplier, true, true);
//        var nAsNumber = new BigInteger(parameters.Modulus, true, true);

//        var demaskedDataAsNumber = (dataAsNumber * BigIntegerHelperMethods.ModInverse(maskMultiplierAsNumber, nAsNumber)) % nAsNumber;
//        return demaskedDataAsNumber.ToByteArray(true, true);
//    }

//    public byte[] Encrypt(byte[] data, byte[] publicKey)
//    {
//        using var rsa = System.Security.Cryptography.RSA.Create();
//        rsa.ImportRSAPublicKey(publicKey, out _);

//        return rsa.Encrypt(data, _encryptionPadding);
//    }

//    public byte[] Decrypt(byte[] data, byte[] privateKey)
//    {
//        using var rsa = System.Security.Cryptography.RSA.Create();
//        rsa.ImportRSAPrivateKey(privateKey, out _);

//        return rsa.Decrypt(data, _encryptionPadding);
//    }
//}
