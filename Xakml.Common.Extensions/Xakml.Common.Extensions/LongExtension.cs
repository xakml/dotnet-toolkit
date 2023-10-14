using System;
using System.Collections.Generic;
using System.Text;

namespace Xakml.Common.Extensions
{
    public static class LongExtension
    {
        private static readonly long MB_Value = 1024 * 1024; //1MB对应的字节数
        private static readonly long GB_Value = 1024 * 1024 * 1024; //1MB对应的字节数
        /// <summary>
        /// 转换字节数对应的友好书写方式(精确到两位小数)
        /// </summary>
        /// <param name="bytes"></param>
        /// <para
        /// <returns></returns>
        public static string GetFriendlyReadStyleOfBytes(this long bytes)
        {
            string friendlyText = "";
            if (bytes < 1024)
            {
                friendlyText = $"{bytes} Byte/s";
            }
            else if (bytes >= 1024 && bytes < (MB_Value))
            {
                friendlyText = Math.Round((double)bytes / 1024, 2) + " KB";
            }
            else if (bytes >= MB_Value && bytes < GB_Value)
            {
                friendlyText = Math.Round((double)bytes / (MB_Value), 2) + " MB";
            }
            else
            {
                friendlyText = Math.Round((double)bytes / (GB_Value), 2) + " GB";
            }
            return friendlyText;
        }
    }
}
