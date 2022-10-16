using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AsepriteRead;
using Microsoft.Win32;

namespace AsepriteReadTest
{
    internal class Program
    {
        public readonly string TempPath = Path.GetTempPath();

        private static void Main(string[] args)
        {
            Task.Run(() =>
            {

                var aseRegFileOpenWithPath = GetAseRegFileOpenWithPath();
                var aseRead = new AseReadFile();
                while (true)
                {
                    Console.Write("请放入ASE文件：");
                    var path = Console.ReadLine();

                    if (aseRead.Read(path))
                        return;
                    Console.WriteLine("Error：传入的可能不是文件或者不是ASE项目文件！");
                }
            }).Wait();

        }

        private static string GetAseRegFileOpenWithPath()
        {
            var classesRoot = Registry.ClassesRoot;
            var software = classesRoot.OpenSubKey("AsepriteFile\\shell\\open\\command");
            var value = ((string)software?.GetValue(""))?.Replace("\"", "").Replace("%1", "").TrimEnd();
            return value;
        }

        public static Uri FilePathToFileUrl(string filePath)
        {
            var uri = new StringBuilder();
            foreach (var v in filePath)
                if (v >= 'a' && v <= 'z' || v >= 'A' && v <= 'Z' || v >= '0' && v <= '9' ||
                    v == '+' || v == '/' || v == ':' || v == '.' || v == '-' || v == '_' || v == '~' ||
                    v > '\x80')
                    uri.Append(v);
                else if (v == Path.DirectorySeparatorChar || v == Path.AltDirectorySeparatorChar)
                    uri.Append('/');
                else
                    uri.Append($"%{(int)v:X2}");
            if (uri.Length >= 2 && uri[0] == '/' && uri[1] == '/') // UNC path
                uri.Insert(0, "file:");
            else
                uri.Insert(0, "file:///");

            try
            {
                return new Uri(uri.ToString());
            }
            catch
            {
                return new Uri(filePath);
            }
        }
    }

}

