using System.Security.Cryptography;
using System.Text;

namespace azpush;

public interface IStringProtector
{
    string ProtectString(string plainText);
    string UnprotectString(string base64ProtectedString);
}

public class WindowsStringProtector : IStringProtector
{
    const DataProtectionScope SCOPE = DataProtectionScope.LocalMachine;
    readonly Encoding StringEncoding = Encoding.UTF8;

    public string ProtectString(string plainText)
    {
        var buffer = ProtectedData.Protect(StringEncoding.GetBytes(plainText), optionalEntropy: null, scope: SCOPE);

        return Convert.ToBase64String(buffer);
    }

    public string UnprotectString(string base64ProtectedString)
    {
        var buffer = Convert.FromBase64String(base64ProtectedString);

        var plainText = StringEncoding.GetString(ProtectedData.Unprotect(buffer, optionalEntropy: null, scope: SCOPE));

        return plainText;
    }
}

public class NullStringProtector : IStringProtector
{
    public string ProtectString(string plainText)
    {
        return plainText;
    }

    public string UnprotectString(string base64ProtectedString)
    {
        return base64ProtectedString;
    }
}