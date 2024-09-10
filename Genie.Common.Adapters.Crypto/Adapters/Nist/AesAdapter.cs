using System.Security.Cryptography;
using Genie.Common.Crypto.Adapters.Interfaces;
using Org.BouncyCastle.Crypto.Paddings;

namespace Genie.Common.Crypto.Adapters.Nist;
public class AesAdapter : ISymmetricBase
{
    // Aes ISymmetricBase is GCM 
    public (byte[] Result, byte[] Tag) Encrypt(Span<byte> data, Span<byte> key, Span<byte> nonce)
    {
        return GcmEncryptData(data, key, nonce);
    }

    // Aes ISymmetricBase is GCM 
    public Span<byte> Decrypt(Span<byte> data, Span<byte> key, Span<byte> nonce, Span<byte> tag)
    {
        return GcmDecryptData(data, key, nonce, tag).ToArray();
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


    public static (byte[] Result, byte[] Tag) CcmEncryptData(Span<byte> data, Span<byte> key, Span<byte> nonce)
    {
        byte[] ciphertext = new byte[data.Length];
        byte[] tag = new byte[16];
        var aes = new AesCcm(key);

        aes.Encrypt(nonce, data, ciphertext, tag);

        return (ciphertext, tag);
    }

    public static Span<byte> CcmDecryptData(byte[] data, byte[] key, byte[] nonce, byte[] tag)
    {
        byte[] plaintext = new byte[data.Length];
        var aes = new AesCcm(key);
        aes.Decrypt(nonce, data, tag, plaintext);

        return plaintext;
    }

    public static (byte[] Result, byte[] Tag) GcmEncryptData(Span<byte> data, Span<byte> key, Span<byte> nonce)
    {
        AesGcm c = new(key, 16);
        byte[] result = new byte[data.Length];
        var spanned = result.AsSpan();
        var tag = new byte[16];
        
        c.Encrypt(nonce, data, spanned, tag.AsSpan());
        
        return (result, tag);
    }

    public static Span<byte> GcmDecryptData(Span<byte> data, Span<byte> key, Span<byte> nonce, Span<byte> tag)
    {
        AesGcm c = new(key, 16);
        var result = new byte[data.Length];
        var spanned = result.AsSpan();
        
        c.Decrypt(nonce, data, tag, spanned);

        return spanned;
    }
}