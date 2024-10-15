
using DeepEqual.Syntax;
using DeepEqual;
using Microsoft.Extensions.ObjectPool;
using System.Security.Cryptography;
using System.Text;

namespace Genie.Core;


public static class StringExtensions
{
    public static string? Null(this string s) => string.IsNullOrEmpty(s) ? null : s;

    public static string Empty(this string s) => string.IsNullOrEmpty(s) ? "" : s;

    public static string NullOrEmpty(this string s, string value) => string.IsNullOrEmpty(s) ? value : s;

    public static byte[] Hex(this string hex) => Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();

    public static string RandomString(ObjectPool<StringBuilder> pool, int length)
    {
        var res = pool.Get();
        
        var result = RandomString(res, length);
        
        pool.Return(res);

        return result;
    }

    public static string RandomString(StringBuilder sb, int length)
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@";

        while (length-- > 0)
        {
            var rng = RandomNumberGenerator.GetBytes(sizeof(uint));
            uint num = BitConverter.ToUInt32(rng, 0);
            sb.Append(valid[(int)(num % (uint)valid.Length)]);
        }

        var result = sb.ToString();
        return result;
    }
}



