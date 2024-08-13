namespace Genie.Common.Crypto.Adapters;
public interface ISymmetricBase
{
    (byte[] Result, byte[] Tag) Encrypt(byte[] data, string key, string nonce);
    byte[] Decrypt(byte[] data, string key, string nonce, byte[] tag);
}