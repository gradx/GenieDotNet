using System.Runtime.Serialization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Dilithium;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public enum DilithiumParameterSet
{
    None,
    
    [EnumMember(Value = "ML-DSA-44")]
    ML_DSA_44,
    
    [EnumMember(Value = "ML-DSA-65")]
    ML_DSA_65,
    
    [EnumMember(Value = "ML-DSA-87")]
    ML_DSA_87
}
