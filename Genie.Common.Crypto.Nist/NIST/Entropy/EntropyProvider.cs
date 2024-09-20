using System.Numerics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class EntropyProvider : IEntropyProvider
    {
        private readonly IRandom800_90 _random;

#pragma warning disable IDE0290 // Use primary constructor
        public EntropyProvider(IRandom800_90 random)
#pragma warning restore IDE0290 // Use primary constructor
        {
            _random = random;
        }

        public virtual BitString GetEntropy(int numberOfBits)
        {
            return _random.GetRandomBitString(numberOfBits);
        }

        public BigInteger GetEntropy(BigInteger minInclusive, BigInteger maxInclusive)
        {
            return _random.GetRandomBigInteger(minInclusive, maxInclusive);
        }

        public void AddEntropy(BitString entropy) { }
        public void AddEntropy(BigInteger entropy) { }
    }
}
