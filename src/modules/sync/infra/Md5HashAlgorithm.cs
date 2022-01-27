namespace azsync;
using System.Security.Cryptography;
using System.Text;

public class Md5HashAlgorithm : IHashAlgorithm
{
    public string ComputeHash(string plainText)
    {
        return Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(plainText)));
    }
}
