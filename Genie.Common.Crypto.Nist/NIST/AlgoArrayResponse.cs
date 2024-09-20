using Newtonsoft.Json;
using NIST.CVP.ACVTS.Libraries.Math;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class AlgoArrayResponse
    {
        [JsonIgnore]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public BitString Message { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [JsonProperty(PropertyName = "md")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public BitString Digest { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [JsonProperty(PropertyName = "outLen")]
        public int DigestLength => Digest.BitLength;

        [JsonIgnore] public bool ShouldPrintOutputLength { get; set; } = false;
    }
}
