using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Xakml.Common.Toolkit
{
    public class FileSystemHelper
    {
        /// <summary>
        /// 检查给定的文件名是否为有效的文件名称
        /// <para>无效的文件名, 则第二个参数返回移除无效字符后的文件名</para>
        /// </summary>
        /// <param name="file_name"></param>
        /// <param name="valid_filename"></param>
        /// <returns></returns>
        public static bool CheckFileName(string file_name, ref string valid_filename)
        {
            char[] invalid_chars = System.IO.Path.GetInvalidFileNameChars();
            int index = file_name.IndexOfAny(invalid_chars);
            if (index > 0)
            {
                valid_filename = ReplaceInvalidChars(file_name, invalid_chars);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查给定的文件名是否为有效的文件名称
        /// <para>无效的文件名, 则第二个参数返回移除无效字符后的文件名</para>
        /// </summary>
        /// <param name="file_name"></param>
        /// <param name="newChar">替换的新字符</param>
        /// <param name="valid_filename">如果返回false,此参数则为有效的文件名</param>
        /// <returns></returns>
        public static bool CheckFileName(string file_name, char newChar, ref string valid_filename)
        {
            char[] invalid_chars = System.IO.Path.GetInvalidFileNameChars();
            int index = file_name.IndexOfAny(invalid_chars);
            if (index > 0)
            {
                valid_filename = ReplaceInvalidChars(file_name, invalid_chars, newChar);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 替换给定字符串中的指定字符（替换为空字符）
        /// </summary>
        /// <param name="input_content"></param>
        /// <param name="invalid_chars">无效的字符串</param>
        /// <returns></returns>
        private static string ReplaceInvalidChars(string input_content, char[] invalid_chars)
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(input_content);
            for (int i = 0; i < invalid_chars.Length; i++)
            {
                if (input_content.IndexOf(invalid_chars[i]) > 0)
                    temp.Replace(invalid_chars[i], '\0');
            }
            return temp.ToString();
        }

        /// <summary>
        ///替换给定字符串中的指定字符（替换为指定字符）
        /// </summary>
        /// <param name="input_content"></param>
        /// <param name="invalid_chars"></param>
        /// <param name="newChar"></param>
        /// <returns></returns>
        private static string ReplaceInvalidChars(string input_content, char[] invalid_chars, char newChar)
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(input_content);
            for (int i = 0; i < invalid_chars.Length; i++)
            {
                if (input_content.IndexOf(invalid_chars[i]) > 0)
                    temp.Replace(invalid_chars[i], newChar);
            }
            return temp.ToString();
        }
    
        #region 拷贝和移动文件

        /// <summary>
        /// 拷贝目录(包含文件)
        /// </summary>
        /// <param name="source_dir">原始目录</param>
        /// <param name="dest_dir">目标目录</param>
        public static void CopyDir(string source_dir, string dest_dir)
        {
            CopyDir(source_dir, dest_dir, false);
        }

        /// <summary>
        /// 拷贝目录(包含文件)
        /// </summary>
        /// <param name="source_dir"></param>
        /// <param name="dest_dir"></param>
        /// <param name="overwrite">遇到同名文件是否覆盖</param>
        public static void CopyDir(string source_dir, string dest_dir, bool overwrite)
        {
            var dirList = ComputeCopyDirPath(source_dir, dest_dir);
            var fileList = ComputeCopyFilePath(source_dir, dest_dir);

            if (!Directory.Exists(dest_dir))
                Directory.CreateDirectory(dest_dir);

            foreach (var dir in dirList)
            {
                System.IO.Directory.CreateDirectory(dir.Item2);
            }

            foreach (var items in fileList)
            {
                File.Copy(items.Item1, items.Item2, overwrite);
            }
        }

        /// <summary>
        /// 计算源目录中的文件,映射到目标目录后的绝对位置
        /// </summary>
        /// <param name="source_dir"></param>
        /// <param name="dest_dir"></param>
        /// <returns></returns>
        private static List<Tuple<string, string>> ComputeCopyFilePath(string source_dir, string dest_dir)
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            if (source_dir.EndsWith(new String(new char[] { Path.PathSeparator })))
                source_dir = source_dir.Substring(0, source_dir.Length - 1);
            string[] files = Directory.GetFiles(source_dir, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string relative_path = file.Substring(source_dir.Length + 1);
                string dest_file = Path.Combine(dest_dir, relative_path);
                Tuple<string, string> tuple = new Tuple<string, string>(file, dest_file);
                list.Add(tuple);
            }
            return list;
        }

        /// <summary>
        /// 计算源目录中的文件,映射到目标目录后的绝对位置
        /// </summary>
        /// <param name="source_dir"></param>
        /// <param name="dest_dir"></param>
        /// <returns></returns>
        private static List<Tuple<string, string>> ComputeCopyDirPath(string source_dir, string dest_dir)
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            if (source_dir.EndsWith(new String(new char[] { Path.PathSeparator })))
                source_dir = source_dir.Substring(0, source_dir.Length - 1);
            string[] subdirs = Directory.GetDirectories(source_dir, "*.*", SearchOption.AllDirectories);

            foreach (var dir in subdirs)
            {
                string relative_path = dir.Substring(source_dir.Length + 1);
                string dest_file = Path.Combine(dest_dir, relative_path);
                Tuple<string, string> tuple = new Tuple<string, string>(dir, dest_file);
                list.Add(tuple);
            }
            return list;
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="src_file_name"></param>
        /// <param name="target_file_name"></param>
        /// <param name="cp_action_report">拷贝进度报告（可能出现跨UI线程调度问题）</param>
        public static async Task CopyFile(string src_file_name, string target_file_name, Action<FileCopyReportArgs> cp_action_report = null)
        {
            if (!System.IO.File.Exists(src_file_name))
                throw new IOException("源文件不存在，拷贝失败，请检查。（" + src_file_name + "）");
            if (File.Exists(target_file_name))
                throw new IOException("目标文件已存在，拷贝失败，请检查。（" + target_file_name + "）");
            FileCopyReportArgs report_arg = null;
            if (null != cp_action_report)
                report_arg = new FileCopyReportArgs();

            using (FileStream fs = File.OpenRead(src_file_name))
            {
                if (null != report_arg)
                    report_arg.TotalBytes = fs.Length;

                int readed_bytes = 0;
                byte[] buffer_read = new byte[1024]; //1024字节的缓存（1K）
                byte[] buffer_write = new byte[1024]; //1024字节的缓存（1K）

                using (FileStream fs_write = new FileStream(target_file_name, FileMode.Create, FileAccess.Write))
                {
                    do
                    {
                        readed_bytes = await fs.ReadAsync(buffer_read, 0, buffer_read.Length);
                        await fs_write.WriteAsync(buffer_read, 0, readed_bytes);
                        if (null != report_arg)
                            report_arg.CopiedBytes += readed_bytes;

                    } while (readed_bytes == buffer_read.Length);
                    fs_write.Flush();
                    fs_write.Close();
                }
                fs.Close();
            }
        }

        /// <summary>
        /// 移动(剪切)文件
        /// </summary>
        /// <param name="src_file_name"></param>
        /// <param name="target_file_name"></param>
        /// <param name="mv_action_report">文件移动进度</param>
        public static async Task MoveFile(string src_file_name, string target_file_name, Action<FileCopyReportArgs> mv_action_report = null)
        {
            if (!System.IO.File.Exists(src_file_name))
                throw new IOException("源文件不存在，拷贝失败，请检查。（" + src_file_name + "）");
            if (File.Exists(target_file_name))
                throw new IOException("目标文件已存在，拷贝失败，请检查。（" + target_file_name + "）");
            FileCopyReportArgs report_arg = null;
            if (null != mv_action_report)
                report_arg = new FileCopyReportArgs();

            using (FileStream fs = File.OpenRead(src_file_name))
            {
                if (null != report_arg)
                    report_arg.TotalBytes = fs.Length;

                int readed_bytes = 0;
                byte[] buffer_read = new byte[1024]; //1024字节的缓存（1K）
                byte[] buffer_write = new byte[1024]; //1024字节的缓存（1K）

                using (FileStream fs_write = new FileStream(target_file_name, FileMode.Create, FileAccess.Write))
                {
                    do
                    {
                        readed_bytes = await fs.ReadAsync(buffer_read, 0, buffer_read.Length);
                        await fs_write.WriteAsync(buffer_read, 0, readed_bytes);
                        if (null != report_arg)
                            report_arg.CopiedBytes += readed_bytes;

                    } while (readed_bytes == buffer_read.Length);
                    fs_write.Flush();
                    fs_write.Close();
                }
                fs.Close();
            }
            File.Delete(src_file_name);
        }
        #endregion

        /// <summary>
        /// 获取最新的文件名称（防止同一目录中出现同名文件）
        /// </summary>
        /// <param name="src_file">原始文件名称</param>
        /// <param name="index">如果发现相同路径下有同名文件，则以此编号为后缀重新命名，-1则自动命名，其他值则使用给定的值（默认-1）</param>
        /// <returns>返回有效的文件名称（可能添加索引后缀）</returns>
        public static string GetOutputFileName(string src_file, int index = -1)
        {
            string output_file = string.Empty;
            string file_extension = Path.GetExtension(src_file);
            string filename_without_extension = Path.GetFileNameWithoutExtension(src_file);
            string file_dir = Path.GetDirectoryName(src_file);
            if (index < 0)
                output_file = Path.Combine(file_dir, $"{filename_without_extension}-1{file_extension}");
            else
                output_file = Path.Combine(file_dir, $"{filename_without_extension}-{index}{file_extension}");
            if (File.Exists(output_file))
            {
                if (index < 0)
                    index = 1;
                else
                    index++;
                output_file = GetOutputFileName(src_file, index);
            }
            return output_file;
        }

        /// <summary>
        /// 统计文件夹的磁盘占用大小
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <param name="includeSubDir">是否包含子目录中的文件</param>
        /// <param name="filesCount">文件夹下所有文件的数量</param>
        /// <returns></returns>
        public static long GetDirectorySize(string dirPath, bool includeSubDir, out int filesCount)
        {
            filesCount = 0;
            if (System.IO.File.Exists(dirPath))
                return 0;
            if (!System.IO.Directory.Exists(dirPath))
                return 0;
            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            var filesInfo = di.GetFiles();
            if (filesInfo != null && filesInfo.Length > 0)
            {
                foreach (FileInfo item in filesInfo)
                {
                    filesCount++;
                    len += item.Length;
                }
            }
            if (includeSubDir)
            {
                DirectoryInfo[] dis = di.GetDirectories();
                if (dis.Length > 0)
                {
                    for (int i = 0; i < dis.Length; i++)
                    {
                        len += GetDirectorySize(dis[i].FullName, true, out int subFilesCount);//递归dis.Length个文件夹,得到每隔dis[i]下面所有文件的大小
                        filesCount += subFilesCount;
                    }
                }
            }
            return len;
        }

        #region 常量值
        /// <summary>
        /// 1MB字节对应的基础值
        /// </summary>
        public const long MBUnitValue = 1_048_576; // 1024 * 1024

        /// <summary>
        /// 1GB字节对应的基础值
        /// </summary>
        public const long GBUnitValue = 1_073_741_824; //1GB 对应的字节数, 1024 * 1024 * 1024

        /// <summary>
        /// 10MB基础值
        /// </summary>
        public const long MBValue10 = MBUnitValue * 10;
        #endregion

        #region 生成随机文件
        /// <summary>
        /// 生成指定大小的随机文件
        /// </summary>
        /// <param name="fileSize">文件大小，单位字节</param>
        /// <param name="targetFileName">指定目标文件的路径（为空时，生成在当前工作目录)</param>
        /// <returns>生成后的文件路径</returns>
        public static string CreateRandomFile(long fileSize, string targetFileName = "")
        {
            //参考代码： https://github.com/kakhovsky/BigFileZIP/blob/master/bigfiletest/BigFileCompression/Program.cs

            int bufferSize = 1024 * 1024; //默认缓冲区数据大小为1K
            byte[] buffer = new byte[bufferSize];
            Random rnd = new Random();
            if (string.IsNullOrEmpty(targetFileName))
                targetFileName = Guid.NewGuid().ToString("N") + ".temp";

            bool hasException = false;
            try
            {
                if (fileSize > bufferSize) //指定文件大于缓存时
                {
                    long count = fileSize / bufferSize;
                    long count_mod = fileSize % bufferSize;

                    using (Stream output = File.Create(targetFileName, 1024, FileOptions.WriteThrough))
                    {
                        do
                        {
                            Array.Clear(buffer, 0, buffer.Length); //是否需要清空上次生产的数据
                            rnd.NextBytes(buffer);
                            output.Write(buffer, 0, buffer.Length);
                            count--;
                        } while (count > 0);

                        if (count_mod > 0)
                        {
                            Array.Clear(buffer, 0, buffer.Length); //是否需要清空上次生产的数据
                            rnd.NextBytes(buffer);
                            output.Write(buffer, 0, (int)count_mod);
                        }
                        output.Flush();
                        output.Close();
                    }
                }
                else //指定文件小于等于缓存时
                {
                    using (Stream output = File.Create(Guid.NewGuid().ToString("N"), 1024, FileOptions.WriteThrough))
                    {
                        rnd.NextBytes(buffer);
                        if (bufferSize - fileSize == 0)
                        {
                            output.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            output.Write(buffer, 0, (int)fileSize);
                        }
                        output.Flush();
                        output.Close();
                    }
                }

                return targetFileName;
            }
            catch (Exception ex)
            {
                hasException = true;
                throw ex;
            }
            finally
            {
                if (hasException)
                {
                    try
                    {
                        if (File.Exists(targetFileName))
                            File.Delete(targetFileName);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("生成随机文件出现异常，删除临时文件失败，可手动删除临时文件： " + targetFileName);
                    }
                }
            }
        }

        /// <summary>
        /// 快速生成随机文件
        /// </summary>
        /// <param name="fileSize">文件大小</param>
        /// <param name="sizeUnit">文件大小单位（KB，MB, GB)</param>
        /// <param name="targetFileName">指定目标文件的路径（为空时，生成在当前工作目录)</param>
        /// <returns></returns>
        public static string CreateRandomFile(int fileSize, FileSizeUnit sizeUnit, string targetFileName = "")
        {
            long fileSizeLong = 0L;
            switch (sizeUnit)
            {
                case FileSizeUnit.KB:
                    {
                        fileSizeLong = fileSize * 1024;
                    }
                    break;
                case FileSizeUnit.MB:
                    {
                        fileSizeLong = fileSize * FileSystemHelper.MBUnitValue;
                    }
                    break;
                case FileSizeUnit.GB:
                    {
                        fileSizeLong = fileSize * FileSystemHelper.GBUnitValue;
                    }
                    break;
            }
            string targetFile = CreateRandomFile(fileSizeLong, targetFileName);
            return targetFile;
        }
        #endregion

        /// <summary>
        /// 文件体积单位
        /// </summary>
        public enum FileSizeUnit
        {
            /// <summary>
            /// 
            /// </summary>
            KB,
            /// <summary>
            /// 
            /// </summary>
            MB,
            /// <summary>
            /// 
            /// </summary>
            GB
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ValidCharOption
    {
        /// <summary>
        /// 移除
        /// </summary>
        Remove,

        /// <summary>
        /// 替换
        /// </summary>
        Replace
    }

    /// <summary>
    /// 文件拷贝进度报告
    /// </summary>
    public class FileCopyReportArgs : EventArgs
    {
        /// <summary>
        /// 已经拷贝的数据大小（单位：字节）
        /// </summary>
        public long CopiedBytes { get; set; }

        /// <summary>
        /// 文件大小（单位：字节）
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FileCopyReportArgs()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="copiedBytes"></param>
        /// <param name="totalBytes"></param>
        public FileCopyReportArgs(long copiedBytes, long totalBytes)
        {
            CopiedBytes = copiedBytes;
            TotalBytes = totalBytes;
        }
    }
}
