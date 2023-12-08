﻿using System.Security.Cryptography;

namespace Algorithms.Abstractions;
public interface IRSAService
{
    byte[] Mask(byte[] data, RSAParameters publicKey, byte[] maskMultiplier);
    byte[] DemaskSignature(byte[] data, RSAParameters publicKey, byte[] maskMultiplier);
    byte[] SignHash(byte[] hash, RSAParameters privateKey);
    bool Verify(byte[] data, byte[] signature, RSAParameters publicKey);
    byte[] Encrypt(byte[] data, RSAParameters publicKey);
    byte[] Decrypt(byte[] data, RSAParameters privateKey);
}