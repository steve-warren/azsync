namespace azpush;

public interface IStringProtector
{
    byte[] ProtectString(string plainText);
    string UnprotectString(byte[] buffer);
}