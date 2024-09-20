#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public enum EntropyProviderTypes
    {
        /// <summary>
        /// Allows for the setting/injection of specific entropy for testing purposes
        /// </summary>
        Testable,
        /// <summary>
        /// Uses a Random number generator for entropy retrieval.
        /// </summary>
        Random
    }
}
