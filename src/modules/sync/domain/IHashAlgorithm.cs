namespace azsync
{
    public interface IHashAlgorithm
    {
        string ComputeHash(string plainText);
    }
}