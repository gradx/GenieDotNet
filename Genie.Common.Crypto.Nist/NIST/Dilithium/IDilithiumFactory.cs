#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Dilithium;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IDilithiumFactory
{
    IMLDSA GetDilithium(DilithiumParameterSet parameterSet);
}
