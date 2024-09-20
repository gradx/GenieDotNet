using System.Runtime.Serialization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper.Enums
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public enum MctVersions
    {
        [EnumMember(Value = "standard")]
        Standard,
        [EnumMember(Value = "alternate")]
        Alternate
    }
}
