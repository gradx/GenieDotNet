using NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Dilithium;
using NIST.CVP.ACVTS.Libraries.Math.Entropy;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Dilithium;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class DilithiumFactory : IDilithiumFactory
{
    private readonly IShaFactory _shaFactory;
    private readonly IEntropyProvider _entropyProvider;
    
#pragma warning disable IDE0290 // Use primary constructor
    public DilithiumFactory(IShaFactory shaFactory, IEntropyProvider entropyProvider)
#pragma warning restore IDE0290 // Use primary constructor
    {
        _shaFactory = shaFactory;
        _entropyProvider = entropyProvider;
    }
    
    public IMLDSA GetDilithium(DilithiumParameterSet parameterSet)
    {
        return new Dilithium(new DilithiumParameters(parameterSet), _shaFactory, _entropyProvider);
    }
}
