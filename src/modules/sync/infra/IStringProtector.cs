namespace azpush;

public interface IStringProtector
{
    string ProtectString(string plainText);
    string UnprotectString(string base64ProtectedString);
}
