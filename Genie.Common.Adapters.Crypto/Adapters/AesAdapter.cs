using System.Security.Cryptography;
using System.Text;

namespace Genie.Common.Crypto.Adapters;
public class AesAdapter : ISymmetricBase
{
    public (byte[] Result, byte[] Tag) Encrypt(byte[] data, string key, string nonce)
    {
        return EncryptData(data, key, nonce);
    }

    public byte[] Decrypt(byte[] data, string key, string nonce, byte[] tag)
    {
        return DecryptData(data, key, nonce, tag);
    }

    public static void EncryptStream(string inputFile, string outputFile, byte[] key, byte[] iv)
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

    public static void DecryptStream(string inputFile, string outputFile, byte[] key, byte[] iv)
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

    public static (byte[] Result, byte[] Tag) EncryptData(byte[] data, string key, string nonce)
    {
        AesGcm c = new(Encoding.UTF8.GetBytes(key), 16);
        byte[] result = new byte[data.Length];
        var tag = new byte[16];
        c.Encrypt(Encoding.UTF8.GetBytes(nonce), data, result, tag);
        return (result, tag);
    }

    public static byte[] DecryptData(byte[] data, string key, string nonce, byte[] tag)
    {
        AesGcm c = new(Encoding.UTF8.GetBytes(key), 16);
        var result = new byte[data.Length];
        c.Decrypt(Encoding.UTF8.GetBytes(nonce), data, tag, result);
        return result;
    }
}