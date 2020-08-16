using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Io
{
    public interface IFileSystem
    {
        void CopyFile(string sourceFileName, string destinationFileName);
        Task<FileStream> CreateAsync(string path);
        void CreateDirForFileIfNotExist(string fileName);
        void CreateDirIfNotExist(string fileName);
        Task DeleteAsync(string path);
        Task<bool> DirectoryExistAsync(string path);

        IEnumerable<DirectoryInfo> EnumerateDirectories(DirectoryInfo directoryInfo, string fileMask,
            SearchOption searchOption);

        IEnumerable<string> EnumerateFilesInDirectory(string directory, string fileMask);
        bool FileExists(string path);
        Task<bool> FileExistsAsync(string path);
        Task ForceDeleteDirectory(string path);
        string GetCurrentDirectory();
        string GetExecutingAssemblyLocation();
        Task MoveAsync(string sourceFileName, string destFileName);
        string ReadTextFile(string fileName);
        Task<string> ReadTextFileAsync(string fileName, CancellationToken cancellationToken = default);
        void Remove(string fileName);
        void WriteTextToFile(string fileName, string content);
        Task WriteTextToFileAsync(string fileName, string content, CancellationToken cancellationToken = default);
    }
}