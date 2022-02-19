namespace azsync;

public class ConfigurationSettingRepository : IConfigurationSettingRepository
{
    private readonly SyncDbContext _context;

    public ConfigurationSettingRepository(SyncDbContext context)
    {
        _context = context;
    }

    public async Task<ConfigurationSetting> FindAsync(string key)
    {
        var setting = await _context.ConfigurationSettings.FindAsync(key).AsTask();
        if (setting is null) throw new KeyNotFoundException();

        return setting;
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