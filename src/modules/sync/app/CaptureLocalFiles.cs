using Microsoft.Data.Sqlite;

namespace azsync;

public record CaptureLocalFiles(string Path, int MaxRecursionDepth) : ICommand { }

public class CaptureLocalFilesHandler : ICommandHandler<CaptureLocalFiles>
{
    private readonly IFileSystem _fs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalFileRepository _localFileRepository;

    public CaptureLocalFilesHandler(IFileSystem fileSystem, IUnitOfWork unitOfWork, ILocalFileRepository localFileRepository)
    {
        _fs = fileSystem;
        _unitOfWork = unitOfWork;
        _localFileRepository = localFileRepository;
    }

    public void Handle(CaptureLocalFiles command)
    {
        CreateTempTable();

        foreach(var file in _fs.GetFilesInDirectory(directoryPath: command.Path, maxRecursionDepth: command.MaxRecursionDepth))
        {
            _localFileRepository.Add(new LocalFile(file.Path, file.Name, file.LastModified, file.PathHash));
        }

        _unitOfWork.SaveChanges();
    }

    private void CreateTempTable()
    {
        using var connection = new SqliteConnection(connectionString: "Data Source=hello.db");
        using var sqlCommand = connection.CreateCommand();
        sqlCommand.CommandText = @"CREATE TEMP TABLE IF NOT EXISTS LocalFile
        (
            Path VARCHAR(256) NOT NULL,
            Name VARCHAR(256) NOT NULL,
            PathHash VARCHAR(32) NOT NULL,
            LastModified DATETIME NOT NULL
        )";

        connection.Open();

        sqlCommand.ExecuteNonQuery();
    }
}
