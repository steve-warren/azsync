namespace azpush;

using System.Buffers;
using System.Security.Cryptography;
using System.Text;

public class Md5HashAlgorithm : IHashAlgorithm
{
    public string ComputeHash(string plainText)
    {
        return Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(plainText)));
    }

    public async Task<string> ComputeHashAsync(Stream stream)
    {
        var hasher = MD5.Create();
        var buffer = await hasher.ComputeHashAsync(stream);

        return Convert.ToBase64String(buffer);
    }
}
