// See https://aka.ms/new-console-template for more information
using Algorithms.Common;
using Algorithms.RSA;
using System.Numerics;

var generator = new RSAKeysGenerator();
var keys  = generator.GenerateKeys();
var maskMultiplier = generator.GenerateMaskMultiplier(keys.PublicKey);

var message = "Hello world";
var data = new ObjectToByteArrayTransformer().Transform(message);

var rsaService = new RSAService();
var mask = rsaService.Mask(data, keys.PublicKey, maskMultiplier);

var signed = rsaService.SignHash(mask, keys.PrivateKey);

var demasked = rsaService.DemaskSignature(signed, keys.PublicKey, maskMultiplier);

var verified = rsaService.VerifyHash(data, demasked, keys.PublicKey);
var verified2 = rsaService.VerifyHash(mask, signed, keys.PublicKey);

var demasked2 = rsaService.DemaskSignature(mask, keys.PublicKey, maskMultiplier);

var encrypted = rsaService.Encrypt(data, keys.PublicKey);
var decrypted = rsaService.Decrypt(encrypted, keys.PrivateKey);
var decryptedData = new ObjectToByteArrayTransformer().ReverseTransform<string>(decrypted);

Console.WriteLine();
