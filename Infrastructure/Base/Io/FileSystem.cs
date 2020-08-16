using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Base.Strings;

namespace Base.Io
{
    public class FileSystem : IFileSystem
    {
        public void CopyFile(string sourceFileName, string destinationFileName)
        {
            File.Copy(sourceFileName, destinationFileName);
        }

        public Task<FileStream> CreateAsync(string path)
        {
            return Task.Run(() => File.Create(path));
        }

        public void CreateDirForFileIfNotExist(string fileName)
        {
            Directory.CreateDirectory(fileName.GetBeforeLast(Path.DirectorySeparatorChar.ToString()));
        }

        public void CreateDirIfNotExist(string path)
        {
            Directory.CreateDirectory(path);
        }

        public Task DeleteAsync(string path)
        {
            return Task.Run(() => { File.Delete(path); });
        }

        public Task<bool> DirectoryExistAsync(string path)
        {
            return Task.Run(() => Directory.Exists(path));
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories(DirectoryInfo directoryInfo, string fileMask,
            SearchOption searchOption)
        {
            return directoryInfo.EnumerateDirectories(fileMask, searchOption);
        }

        public IEnumerable<string> EnumerateFilesInDirectory(string directory, string fileMask)
        {
            return Directory.EnumerateFiles(directory, fileMask, SearchOption.TopDirectoryOnly);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Task<bool> FileExistsAsync(string path)
        {
            return Task.Run(() => File.Exists(path));
        }

        public async Task ForceDeleteDirectory(string path)
        {
            if (!await DirectoryExistAsync(path).ConfigureAwait(false)) return;

            var baseFolder = new DirectoryInfo(path);

            foreach (var item in baseFolder.EnumerateDirectories("*", SearchOption.AllDirectories))
                item.Attributes = ResetAttributes(item.Attributes);

            foreach (var item in baseFolder.EnumerateFiles("*", SearchOption.AllDirectories))
                item.Attributes = ResetAttributes(item.Attributes);

            baseFolder.Delete(true);
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public string GetExecutingAssemblyLocation()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public Task MoveAsync(string sourceFileName, string destFileName)
        {
            return Task.Run(() => { File.Move(sourceFileName, destFileName); });
        }

        public string ReadTextFile(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public Task<string> ReadTextFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            return File.ReadAllTextAsync(fileName, cancellationToken);
        }

        public void Remove(string fileName)
        {
            File.Delete(fileName);
        }

        public void WriteTextToFile(string fileName, string content)
        {
            CreateDirForFileIfNotExist(fileName);
            File.WriteAllText(fileName, content);
        }

        public Task WriteTextToFileAsync(string fileName, string content, CancellationToken cancellationToken = default)
        {
            CreateDirForFileIfNotExist(fileName);
            return File.WriteAllTextAsync(fileName, content, cancellationToken);
        }

        private static FileAttributes ResetAttributes(FileAttributes attributes)
        {
            return attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
        }
    }
}