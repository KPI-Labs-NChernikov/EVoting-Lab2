// See https://aka.ms/new-console-template for more information
using Algorithms.Common;
using Algorithms.RSA;
using System.Numerics;

var generator = new RSAKeysGenerator();
var keys  = generator.GenerateKeys();

var bouncyPrivate = BouncyCastleRsaParametersMapper.RSAParametersToBouncyPrivate(keys.PrivateKey);
var bouncyPublic = BouncyCastleRsaParametersMapper.RSAParametersToBouncyPublic(keys.PublicKey);

var privateParameters = BouncyCastleRsaParametersMapper.BouncyPrivateToRSAParameters(bouncyPrivate);
var publicParameters = BouncyCastleRsaParametersMapper.BouncyPublicToRSAParameters(bouncyPublic);

Console.WriteLine();
