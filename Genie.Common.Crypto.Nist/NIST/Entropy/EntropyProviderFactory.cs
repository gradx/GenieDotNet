using System;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Used for retrieving an instance of an <see cref="IEntropyProvider"/>
    /// </summary>
    public class EntropyProviderFactory : IEntropyProviderFactory
    {
        /// <summary>
        /// Returns a new instance of an <see cref="IEntropyProvider"/>
        /// </summary>
        /// <param name="providerType">The <see cref="IEntropyProvider"/> type </param>
        /// <exception cref="ArgumentException">Thrown when <see cref="providerType"/> is invalid</exception>
        /// <returns></returns>
        public IEntropyProvider GetEntropyProvider(EntropyProviderTypes providerType)
        {
#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (providerType)
            {
                case EntropyProviderTypes.Testable:
                    return new TestableEntropyProvider();
                case EntropyProviderTypes.Random:
                    return new EntropyProvider(new Random800_90());
                default:
                    throw new ArgumentException($"Invalid {providerType} supplied.");
            }
#pragma warning restore IDE0066 // Convert switch statement to expression
        }
    }
}
