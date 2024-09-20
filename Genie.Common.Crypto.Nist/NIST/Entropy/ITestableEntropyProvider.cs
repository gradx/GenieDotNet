using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Entropy
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Allows for adding specific entropy to a provider
    /// </summary>
    public interface ITestableEntropyProvider
    {
        /// <summary>
        /// Add entropy
        /// </summary>
        /// <param name="entropy">The <see cref="BitString"/> to add</param>
        void AddEntropy(BitString entropy);

        void AddEntropy(BigInteger entropy);
    }
}
