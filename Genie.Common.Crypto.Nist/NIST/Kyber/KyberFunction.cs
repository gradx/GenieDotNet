using System.Runtime.Serialization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Kyber;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public enum KyberFunction
{
    None,
    
    [EnumMember(Value = "encapsulation")]
    Encapsulation,
    
    [EnumMember(Value = "decapsulation")]
    Decapsulation
}
