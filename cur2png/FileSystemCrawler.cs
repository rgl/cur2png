// developed by Rui Lopes (ruilopes.com). licensed under GPLv3.

using System.IO;

namespace cur2png
{
    public class FileSystemCrawler
    {
        public delegate bool EnterDirectoryDelegate(DirectoryInfo directory, int depth);
        public delegate void LeaveDirectoryDelegate(DirectoryInfo directory, int depth);
        public delegate void FoundFileDelegate(DirectoryInfo directory, FileInfo file, int depth);

        public event EnterDirectoryDelegate OnEnterDirectory;
        public event LeaveDirectoryDelegate OnLeaveDirectory;
        public event FoundFileDelegate OnFoundFile;

        public void Crawl(DirectoryInfo directory, string filesSearchPattern = "*.*", int depth = 0)
        {
            if (!OnEnterDirectory(directory, depth))
                return;

            foreach (var d in directory.EnumerateDirectories())
            {
                Crawl(d, filesSearchPattern, depth + 1);
            }

            foreach (var f in directory.EnumerateFiles(filesSearchPattern))
            {
                OnFoundFile(directory, f, depth);
            }

            OnLeaveDirectory(directory, depth);
        }
    }
}