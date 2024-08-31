namespace Genie.Common.Crypto.Adapters;
public interface ISymmetricBase
{
    (byte[] Result, byte[] Tag) Encrypt(Span<byte> data, Span<byte> key, Span<byte> nonce);
    Span<byte> Decrypt(Span<byte> data, Span<byte> key, Span<byte> nonce, Span<byte> tag);
}