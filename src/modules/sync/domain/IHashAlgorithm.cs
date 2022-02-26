namespace azpush
{
    public interface IHashAlgorithm
    {
        string ComputeHash(string plainText);
        Task<string> ComputeHashAsync(Stream stream);
    }
}