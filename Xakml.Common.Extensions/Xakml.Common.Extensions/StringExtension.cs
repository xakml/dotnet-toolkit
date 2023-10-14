using System.Collections.Generic;
using System.IO;

namespace Xakml.Common.Extensions
{
    /// <summary>
    /// 字符串相关的扩展方法
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 移除文件的扩展名，包含英文句点（.）
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>移除扩展名后的文件路径</returns>
        public static string RemoveFileExtension(this string fileName)
        {
            if(fileName == null || fileName.Length == 0) return null;
            if(fileName.IndexOf(".") == -1 ) return fileName;
            var outputpath = Path.ChangeExtension(fileName, "").TrimEnd(new char[] { '.' });
            return outputpath;
        }

        /// <summary>
        /// 检索给定文件夹下的所有文件
        /// </summary>
        /// <param name="dirName">指定的文件夹路径</param>
        /// <param name="includeSubDir">是否包含子文件夹内的文件</param>
        /// <returns>检索到的文件集合</returns>
        public static List<string> GetFilesOfDirectory(this string dirName,bool includeSubDir = false)
        {
            var paths = new List<string>();
            if (!Directory.Exists(dirName))
                return paths;
            if (includeSubDir)
            {
                var subFolders = Directory.GetDirectories(dirName);
                if (subFolders != null)
                {

                    foreach (var path in subFolders)
                    {
                        paths.AddRange(GetFilesOfDirectory(path,includeSubDir));
                    }
                }
            }
            var subFiles = Directory.GetFiles(dirName);
            if (subFiles != null)
            {
                paths.AddRange(subFiles);
            }


            return paths;
        }

        /// <summary>
        /// 查询目录的磁盘空间占用
        /// </summary>
        /// <param name="dir_path">需要计算的目录路径</param>
        /// <param name="filesCount">统计到的文件数量</param>
        /// <returns>目录的磁盘占用大小（单位：字节）</returns>
        public static long GetDirectorySize(this string dir_path, out int filesCount)
        {
            filesCount = 0;
            if (!System.IO.Directory.Exists(dir_path))
                return 0;
            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dir_path);
            var filesInfo = di.GetFiles();
            if (filesInfo != null && filesInfo.Length > 0)
            {
                foreach (FileInfo item in filesInfo)
                {
                    filesCount++;
                    len += item.Length;
                }
            }
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectorySize(dis[i].FullName, out int subFilesCount);//递归dis.Length个文件夹,得到每隔dis[i]下面所有文件的大小
                    filesCount += subFilesCount;
                }
            }
            return len;
        }

        /// <summary>
        /// 读取指定路径所属逻辑驱动器的剩余空间
        /// </summary>
        /// <param name="path">路径可以是文件夹或文件</param>
        /// <returns>逻辑驱动器磁盘的剩余空间（单位：字节）</returns>
        public static long GetFreespaceOfDisk(this string path)
        {
            if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
                throw new IOException("invalid path");
            if (System.IO.File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                DriveInfo di = new DriveInfo(fileInfo.Directory.Root.FullName);
                return di.AvailableFreeSpace;
            }
            if (System.IO.Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                DriveInfo di = new DriveInfo(dir.Root.FullName);
                return di.AvailableFreeSpace;
            }
            return -1;
        }
    }
}
