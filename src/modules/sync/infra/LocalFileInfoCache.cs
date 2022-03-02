namespace azpush;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;

public class LocalFileInfoCache
{
    private const string TEMP_STORE_TABLE_NAME = "LocalFileInfo";

    private readonly SyncDbContext _context;

    public LocalFileInfoCache(SyncDbContext context)
    {
        _context = context;
    }

    public async Task PrepareAsync()
    {
        var connection = _context.Database.GetDbConnection();

        try
        {
            await connection.OpenAsync();

            await SetInMemoryTempStoreAsync(connection);
            await DropTempTableAsync(connection);
            await CreateTempTableAsync(connection);
        }

        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task AddAsync(IEnumerable<LocalFileInfo> files)
    {
        var connection = _context.Database.GetDbConnection();

        try
        {
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            using var sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = @$"
                INSERT INTO {TEMP_STORE_TABLE_NAME}(Path, Name, PathHash, FileSizeInBytes, LastModified, LocalPathId, ContainerId)
                VALUES($Path, $Name, $PathHash, $FileSizeInBytes, $LastModified, $LocalPathId, $ContainerId)";
            
            var path = sqlCommand.CreateParameter();
            path.ParameterName = "$Path";
            sqlCommand.Parameters.Add(path);
            
            var name = sqlCommand.CreateParameter();
            name.ParameterName = "$Name";
            sqlCommand.Parameters.Add(name);

            var hash = sqlCommand.CreateParameter();
            hash.ParameterName = "$PathHash";
            sqlCommand.Parameters.Add(hash);

            var size = sqlCommand.CreateParameter();
            size.ParameterName = "$FileSizeInBytes";
            sqlCommand.Parameters.Add(size);

            var modified = sqlCommand.CreateParameter();
            modified.ParameterName = "$LastModified";
            sqlCommand.Parameters.Add(modified);

            var localPathId = sqlCommand.CreateParameter();
            localPathId.ParameterName = "$LocalPathId";
            sqlCommand.Parameters.Add(localPathId);

            var containerId = sqlCommand.CreateParameter();
            containerId.ParameterName = "$ContainerId";
            sqlCommand.Parameters.Add(containerId);

            await sqlCommand.PrepareAsync();

            foreach(var file in files)
            {
                path.Value = file.Path;
                name.Value = file.Name;
                hash.Value = file.PathHash;
                size.Value = file.FileSizeInBytes;
                modified.Value = file.LastModified;
                localPathId.Value = file.LocalPathId;
                containerId.Value = file.ContainerId;

                await sqlCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }

        finally
        {
            await connection.CloseAsync();
        }
    }

    public Task<List<LocalFileInfo>> GetNewAsync(int pathId)
    {
        var query = from lf in _context.LocalFiles
                    join sf in _context.BlobFiles on lf.PathHash equals sf.LocalFilePathHash into group_join
                    from default_sf in group_join.DefaultIfEmpty()
                    where lf.LocalPathId == pathId && default_sf == null
                    select lf;
        
        return query.ToListAsync();
    }

    public Task<List<LocalFileInfo>> GetModifiedAsync(int pathId)
    {
        var query = from lf in _context.LocalFiles
                    join sf in _context.BlobFiles on lf.PathHash equals sf.LocalFilePathHash
                    where lf.LocalPathId == pathId && lf.LastModified > sf.LastModified
                    select lf;

        return query.ToListAsync();
    }

    private static async Task CreateTempTableAsync(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @$"CREATE TEMP TABLE IF NOT EXISTS {TEMP_STORE_TABLE_NAME}
        (
            Id INTEGER NOT NULL,
            Path VARCHAR(256) NOT NULL,
            Name VARCHAR(256) NOT NULL,
            PathHash VARCHAR(32) NOT NULL,
            FileSizeInBytes INT NOT NULL,
            LastModified DATETIME NOT NULL,
            LocalPathId INT NOT NULL,
            ContainerId INT NOT NULL,
            PRIMARY KEY('Id' AUTOINCREMENT)
        )";

        await command.ExecuteNonQueryAsync();
    }

    private static async Task SetInMemoryTempStoreAsync(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"PRAGMA temp_store=MEMORY;PRAGMA temp_store;";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DropTempTableAsync(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @$"DROP TABLE IF EXISTS {TEMP_STORE_TABLE_NAME};";
        await command.ExecuteNonQueryAsync();
    }
}
