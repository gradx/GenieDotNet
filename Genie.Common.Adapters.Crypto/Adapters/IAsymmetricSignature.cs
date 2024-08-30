namespace Genie.Common.Crypto.Adapters;

public interface IAsymmetricSignature<T>
{
    byte[] Sign(byte[] data, T key);
    bool Verify(byte[] data, byte[] signature, T key);
    byte[] Export(T key, bool isPrivate);
}