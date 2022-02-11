namespace azsync;

public class ConfigurationSettingRepository : IConfigurationSettingRepository
{
    private readonly SyncDbContext _context;

    public ConfigurationSettingRepository(SyncDbContext context)
    {
        _context = context;
    }

    public Task<ConfigurationSetting> FindAsync(string key)
    {
        return _context.ConfigurationSettings.FindAsync(key).AsTask();
    }
    
    public void AddRange(IEnumerable<ConfigurationSetting> settings)
    {
        _context.ConfigurationSettings.AddRange(settings);
    }

    public void RemoveRange(IEnumerable<ConfigurationSetting> settings)
    {
        _context.ConfigurationSettings.RemoveRange(settings);
    }
}