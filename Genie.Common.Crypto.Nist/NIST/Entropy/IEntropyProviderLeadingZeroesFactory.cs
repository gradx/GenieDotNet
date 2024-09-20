#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Means of getting entropy that has (a minimum) number of leading zeroes.
    /// </summary>
    public interface IEntropyProviderLeadingZeroesFactory : IEntropyProviderFactory
    {
        /// <summary>
        /// The (minimum) number of leading zeroes to be returned by the <see cref="IEntropyProvider"/>.
        /// </summary>
        int MinimumLeadingZeroes { get; set; }
    }
}
