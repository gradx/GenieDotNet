#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math.Helpers
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class ByteExtensions
    {
        public static BitString ToBitString(this byte value)
        {
            return BitString.To8BitString(value);
        }
    }
}
