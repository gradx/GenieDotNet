using NIST.CVP.ACVTS.Libraries.Math;
using NIST.CVP.ACVTS.Libraries.Math.Domain;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public interface IShaMct
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        MctResult<AlgoArrayResponse> MctHash(BitString message, bool isSample = false, MathDomain domain = null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
