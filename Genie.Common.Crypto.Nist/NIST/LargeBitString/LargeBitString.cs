#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.LargeBitString
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class LargeBitString
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public BitString Content { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public int ContentLength => Content?.BitLength ?? 0;
        public long FullLength { get; set; }
        public ExpansionMode ExpansionTechnique { get; set; }
    }
}
