#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.Hash.ShaWrapper;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IShake : ISha
{
    void Absorb(byte[] message, int bitLength);
    void Squeeze(byte[] output, int outputBitLength);
}
