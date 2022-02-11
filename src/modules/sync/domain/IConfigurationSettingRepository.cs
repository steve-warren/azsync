namespace azsync
{
    public interface IConfigurationSettingRepository
    {
        Task<ConfigurationSetting> FindAsync(string key);
        void AddRange(IEnumerable<ConfigurationSetting> settings);
        void RemoveRange(IEnumerable<ConfigurationSetting> settings);
    }
}