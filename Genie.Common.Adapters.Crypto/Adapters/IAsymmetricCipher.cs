namespace Genie.Common.Crypto.Adapters;

public interface IAsymmetricCipher<T>
{
    byte[] Encrypt(T provider, byte[] data);
    byte[] Decrypt(T provider, byte[] data);
}