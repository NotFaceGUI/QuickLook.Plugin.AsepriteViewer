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
    }
}

