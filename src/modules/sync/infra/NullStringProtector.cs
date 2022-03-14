namespace azpush;

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