using System.Security.Cryptography;
using System.Text;
using Utf8StringInterpolation;

namespace Genie.Common.Crypto.Adapters;
public class AesAdapter : ISymmetricBase
{
    // Aes ISymmetricBase is GCM 
    public (byte[] Result, byte[] Tag) Encrypt(byte[] data, string key, string nonce)
    {
        return GcmEncryptData(data, key, nonce);
    }

    // Aes ISymmetricBase is GCM 
    public byte[] Decrypt(byte[] data, string key, string nonce, byte[] tag)
    {
        return GcmDecryptData(data.AsSpan(), key, nonce, tag.AsSpan()).ToArray();
    }

    public static void CbcEncryptStream(string inputFile, string outputFile, byte[] key, byte[] iv)
    {
        FileStream fsIn = new(inputFile, FileMode.Open);
        FileStream fsCrypt = new(outputFile, FileMode.Create, FileAccess.Write);

        Aes c = Aes.Create();
        c.Mode = CipherMode.CBC;
        c.KeySize = 256;
        c.BlockSize = 128;
        c.Padding = PaddingMode.PKCS7;

        var encryptor = c.CreateEncryptor(key, iv);

        byte[] buffer = new byte[4096];
        int read;
        var cs = new CryptoStream(fsCrypt, encryptor, CryptoStreamMode.Write);
        while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
            cs.Write(buffer, 0, read);

        cs.Close();
        fsCrypt.Close();
        fsIn.Close();
    }

    public static void CbcDecryptStream(string inputFile, string outputFile, byte[] key, byte[] iv)
    {
        FileStream fsCrypt = new(inputFile, FileMode.Open);
        FileStream fsOut = new(outputFile, FileMode.Create, FileAccess.Write);

        Aes c = Aes.Create();
        c.Mode = CipherMode.CBC;
        c.KeySize = 256;
        c.BlockSize = 128;
        c.Padding = PaddingMode.PKCS7;

        var decryptor = c.CreateDecryptor(key, iv);

        byte[] buffer = new byte[4096];
        int read;

        var cs = new CryptoStream(fsCrypt, decryptor, CryptoStreamMode.Read);
        while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
            fsOut.Write(buffer, 0, read);

        cs.Close();
        fsCrypt.Close();
        fsOut.Close();
    }

    public static (byte[] Result, byte[] Tag) GcmEncryptData(Span<byte> data, string key, string nonce)
    {
        AesGcm c = new(Utf8String.Format($"{key}").AsSpan(), 16);
        byte[] result = new byte[data.Length];
        var spanned = result.AsSpan();
        var tag = new byte[16];
        c.Encrypt(Utf8String.Format($"{nonce}"), data, spanned, tag.AsSpan());
        return (result, tag);
    }

    public static Span<byte> GcmDecryptData(Span<byte> data, string key, string nonce, Span<byte> tag)
    {
        AesGcm c = new(Utf8String.Format($"{key}").AsSpan(), 16);
        var result = new byte[data.Length];
        var spanned = result.AsSpan();
        c.Decrypt(Utf8String.Format($"{nonce}"), data, tag, spanned);
        return spanned;
    }
}