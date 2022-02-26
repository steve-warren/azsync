namespace azsync;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;

public class LocalFileRepository
{
    private readonly SyncDbContext _context;

    public LocalFileRepository(SyncDbContext context)
    {
        _context = context;
    }

    public void ReplaceAll(IEnumerable<LocalFile> files)
    {
        var connection = _context.Database.GetDbConnection();

        try
        {
            connection.Open();

            CreateTempTable(connection);

            using var transaction = connection.BeginTransaction();
            using var sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = @"
                INSERT INTO LocalFile(Path, Name, PathHash, FileSizeInBytes, LastModified, LocalPathId, ContainerId)
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

            sqlCommand.Prepare();

            foreach(var file in files)
            {
                path.Value = file.Path;
                name.Value = file.Name;
                hash.Value = file.PathHash;
                size.Value = file.FileSizeInBytes;
                modified.Value = file.LastModified;
                localPathId.Value = file.LocalPathId;
                containerId.Value = file.ContainerId;

                sqlCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        finally
        {
            connection.Close();
        }
    }

    public Task<List<LocalFile>> GetNew(int pathId)
    {
        var query = from lf in _context.LocalFiles
                    join sf in _context.SyncFiles on lf.PathHash equals sf.LocalFilePathHash into group_join
                    from default_sf in group_join.DefaultIfEmpty()
                    where lf.LocalPathId == pathId && default_sf == null
                    select lf;
        
        return query.ToListAsync();
    }

    public IQueryable<LocalFile> GetModified(int pathId)
    {
        var query = from lf in _context.LocalFiles
                    join sf in _context.SyncFiles on lf.PathHash equals sf.LocalFilePathHash
                    where lf.LocalPathId == pathId && lf.LastModified > sf.LastModified
                    select lf;

        return query;
    }

    private static void CreateTempTable(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
        PRAGMA temp_store=MEMORY;PRAGMA temp_store;
        DROP TABLE IF EXISTS LocalFile;
        CREATE TEMP TABLE IF NOT EXISTS LocalFile
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

        command.ExecuteNonQuery();
    }
}
