using System;
using System.Collections;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class EntropyProviderLeadingOnes : EntropyProvider
    {
        private readonly int _minimumLeadingOnes;

#pragma warning disable IDE0290 // Use primary constructor
        public EntropyProviderLeadingOnes(IRandom800_90 random, int minimumLeadingOnes) : base(random)
#pragma warning restore IDE0290 // Use primary constructor
        {
            _minimumLeadingOnes = minimumLeadingOnes;
        }

        public override BitString GetEntropy(int numberOfBits)
        {
            var totalRandomBits = numberOfBits - _minimumLeadingOnes;

            if (totalRandomBits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfBits),
                    "Random number of bits to generate cannot be less than 0.");
            }

            var bits = new BitArray(_minimumLeadingOnes, true);

            return new BitString(bits)
                .ConcatenateBits(base.GetEntropy(totalRandomBits));
        }
    }
}
