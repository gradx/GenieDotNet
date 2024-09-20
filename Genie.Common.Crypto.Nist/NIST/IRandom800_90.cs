using System.Numerics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Math
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public interface IRandom800_90
    {
        BitString GetRandomBitString(int length);
        BitString GetDifferentBitStringOfSameSize(BitString original);
        int GetRandomInt(int minInclusive, int maxExclusive);
        BigInteger GetRandomBigInteger(BigInteger maxInclusive);
        BigInteger GetRandomBigInteger(BigInteger minInclusive, BigInteger maxInclusive);
        string GetRandomAlphaCharacters(int length);
        string GetRandomString(int length);
        decimal GetRandomDecimal();
        int Next();
        void NextBytes(byte[] buffer);
    }
}
