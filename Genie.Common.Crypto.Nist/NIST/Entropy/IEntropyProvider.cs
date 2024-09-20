using System.Numerics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Provides an interface for getting entropy as <see cref="BitString"/> with a specified number of bits
    /// </summary>
    public interface IEntropyProvider
    {
        /// <summary>
        /// Get Entropy
        /// </summary>
        /// <param name="numberOfBits">The number of bits to receive</param>
        /// <returns>Entropy as a <see cref="BitString"/></returns>
        BitString GetEntropy(int numberOfBits);

        BigInteger GetEntropy(BigInteger minInclusive, BigInteger maxInclusive);

        void AddEntropy(BitString entropy);
        void AddEntropy(BigInteger entropy);
    }
}
