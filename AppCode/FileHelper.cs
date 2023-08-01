using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BatchHardLink.AppCode
{
    public static class FileHelper
    {
        public static Action<string>? DoAfterCreate = null;

        /// <summary>
        /// 系统类库 创建硬链接
        /// </summary>
        /// <param name="linkName"></param>
        /// <param name="sourceName"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        extern static bool CreateHardLink(string linkName, string sourceName, IntPtr attribute);

        //static void CreateHardLink2(string linkName, string sourceName)
        //{
        //    Process.Start("mklink /H", String.Format("{0} {1}", linkName, sourceName));
        //}
        
        /// <summary>
        /// 创建硬链接
        /// 对外暴露方法
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        public static void BatchCreateHardLink(string sourcePath, string targetPath,  HashSet<string> exceptPrefix)
        {
            var source = new DirectoryInfo(sourcePath);
            if (!source.Exists)
            {
                return;
            }

            CreateInEachDirectory(source, targetPath, exceptPrefix);
            OpenDirectory(targetPath);
        }

        private static void CreateInEachDirectory(DirectoryInfo source, string targetPath, HashSet<string> exceptPrefix)
        {
            var target = new DirectoryInfo(targetPath);
            if (!target.Exists)
            {
                Directory.CreateDirectory(targetPath);
            }

            foreach (var item in source.EnumerateFiles())
            {
                if (exceptPrefix.Count > 0 && exceptPrefix.Contains(item.Extension.ToLower()))
                {
                    continue;
                }

                CreateHardLink(Path.Combine(targetPath, item.Name), item.FullName, IntPtr.Zero);
                DoAfterCreate?.Invoke(item.FullName);
            }

            foreach (var directory in source.EnumerateDirectories())
            {
                CreateInEachDirectory(directory, Path.Combine(targetPath, directory.Name), exceptPrefix);
            }
        }

        internal static bool PathCheck(string source, string target, string tip, ref StringBuilder error)
        {
            if (!string.IsNullOrEmpty(tip))
            {
                error.Append("后台创建中，请稍后");
                return false;
            }

            if (string.IsNullOrEmpty(source))
            {
                error.Append("请选择源文件地址");
                return false;
            }

            if (string.IsNullOrEmpty(target))
            {
                error.Append("请选择硬链接地址");
                return false;
            }

            if (source.Equals(target))
            {
                error.Append("源文件与硬链接地址不允许相同");
                return false;
            }

            var sourceInfo = new DirectoryInfo(source);
            var sourceRoot = sourceInfo.Root;

            var targetInfo = new DirectoryInfo(target);
            var targetRoot = targetInfo.Root;

            if (sourceRoot.Name != targetRoot.Name)
            {
                error.Append("源文件与硬链接地址只支持在同一个盘");
                return false;
            }

            var drive = new DriveInfo(sourceRoot.FullName);
            if (drive.DriveFormat != "NTFS")
            {
                error.Append("文件系统仅支持NTFS格式");
                return false;
            }
            return true;
        }

        static void OpenDirectory(string path)
        {
            Process.Start("explorer.exe", path);
        }
    }
}
