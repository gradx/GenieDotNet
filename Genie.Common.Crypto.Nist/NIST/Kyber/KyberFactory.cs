using NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper;
using NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Kyber;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Kyber;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class KyberFactory
{
    private readonly IShaFactory _shaFactory;

#pragma warning disable IDE0290 // Use primary constructor
    public KyberFactory(IShaFactory shaFactory)
#pragma warning restore IDE0290 // Use primary constructor
    {
        _shaFactory = shaFactory;
    }

    public IMLKEM GetKyber(KyberParameterSet parameterSet)
    {
        var param = new KyberParameters(parameterSet);
        return new Kyber(param, _shaFactory);
    }
}
