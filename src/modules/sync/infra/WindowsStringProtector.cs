using System.Security.Cryptography;
using System.Text;

namespace azpush;

public class WindowsStringProtector : IStringProtector
{
    const DataProtectionScope SCOPE = DataProtectionScope.CurrentUser;
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
