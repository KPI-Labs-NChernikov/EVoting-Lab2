using Algorithms.Common;

namespace Algorithms.Abstractions;
public interface IMaskedAsymmetricAlgorithmKeysGenerator<TKey>
{
    Keys<TKey> GenerateKeys();
    byte[] GenerateMaskMultiplier(TKey publicKey);
}
