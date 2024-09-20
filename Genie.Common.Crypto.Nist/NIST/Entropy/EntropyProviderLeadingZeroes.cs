using System;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class EntropyProviderLeadingZeroes : EntropyProvider
    {
        private readonly int _minimumLeadingZeroes;

#pragma warning disable IDE0290 // Use primary constructor
        public EntropyProviderLeadingZeroes(IRandom800_90 random, int minimumLeadingZeroes) : base(random)
#pragma warning restore IDE0290 // Use primary constructor
        {
            _minimumLeadingZeroes = minimumLeadingZeroes;
        }

        public override BitString GetEntropy(int numberOfBits)
        {
            var totalRandomBits = numberOfBits - _minimumLeadingZeroes;

            if (totalRandomBits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfBits),
                    "Random number of bits to generate cannot be less than 0.");
            }

            return new BitString(_minimumLeadingZeroes)
                .ConcatenateBits(base.GetEntropy(totalRandomBits));
        }
    }
}
