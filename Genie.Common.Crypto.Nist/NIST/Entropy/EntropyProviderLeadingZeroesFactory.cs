﻿#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class EntropyProviderLeadingZeroesFactory : IEntropyProviderLeadingZeroesFactory
    {
        private readonly IRandom800_90 _random;

#pragma warning disable IDE0290 // Use primary constructor
        public EntropyProviderLeadingZeroesFactory(IRandom800_90 random)
#pragma warning restore IDE0290 // Use primary constructor
        {
            _random = random;
        }

        public IEntropyProvider GetEntropyProvider(EntropyProviderTypes providerType)
        {
            return new EntropyProviderLeadingZeroes(_random, MinimumLeadingZeroes);
        }

        public int MinimumLeadingZeroes { get; set; }
    }
}
