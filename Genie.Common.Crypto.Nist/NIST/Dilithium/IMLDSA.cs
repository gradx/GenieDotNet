using System.Collections;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Crypto.Common.PQC.Dilithium;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IMLDSA
{
    public (byte[] pk, byte[] sk) GenerateKey(BitArray seed);
    public byte[] Sign(byte[] sk, BitArray message, bool deterministic);
    public bool Verify(byte[] pk, byte[] signature, BitArray message);
}
