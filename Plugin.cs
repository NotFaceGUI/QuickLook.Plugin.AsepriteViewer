using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using QuickLook.Plugin.AsepriteViewer.AseRead;
using QuickLook.Plugin.ImageViewer;

namespace QuickLook.Plugin.AsepriteViewer
{
    public class Plugin : IViewer
    {
        public static string ExePath;
        public static bool NotFound;

        private readonly string[] _formats = { ".ase", ".aseprite" };
        private readonly string _tempPath = Path.GetTempPath() + "quick-look-ase";

        private string _imagePath;

        private readonly string[] _exeSparePaths =
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\Aseprite\Aseprite.exe",
        };

        private ImagePanel _image;
        private MetaProvider _meta;


        public int Priority => 0;

        public void Init()
        {
            if (!Directory.Exists(_tempPath))
                Directory.CreateDirectory(_tempPath);

            ExePath = GetAseRegFileOpenWithPath();

            if (ExePath != default) return;

            foreach (var exeSparePath in _exeSparePaths)
            {
                if (File.Exists(exeSparePath))
                    ExePath = exeSparePath;
            }

            var tempPathConfig = _tempPath + "/config";
            if (File.Exists(tempPathConfig))
            {
                var readLines = File.ReadLines(tempPathConfig);
                foreach (var readLine in readLines)
                    ExePath = readLine;
            }
            else
                File.Create(tempPathConfig);
            if (ExePath == default)
                NotFound = true;
        }

        public bool CanHandle(string path)
        {
            return !Directory.Exists(path) && _formats.Contains(Path.GetExtension(path)?.ToLower());
        }

        public void Prepare(string path, ContextObject context)
        {


            if (NotFound)
            {
                context.PreferredSize = new Size { Width = 600, Height = 400 };
            }
            else
            {
                var fileName = Path.GetFileName(path);
                var tempPath = Path.GetTempPath() + "quick-look-ase";
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                var fileInfo = new FileInfo(path);
                var fileInfoLastWriteTime = fileInfo.LastWriteTime;

                var identifier = GetMd5(tempPath + "\\" + fileName + fileInfoLastWriteTime);

                var aseReadFile = new AseReadFile();
                var readAseIsGif = aseReadFile.ReadAseIsGIF(path, _formats);
                if (readAseIsGif)
                    _imagePath = tempPath + "\\" + identifier + ".gif";
                else
                    _imagePath = tempPath + "\\" + identifier + ".png";

                if (!File.Exists(_imagePath))
                {
                    var cmdProcess = new Process
                    {
                        StartInfo =
                        {
                            FileName = "cmd.exe",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            Arguments =
                                $"/c {ExePath} -b {path} --save-as {_imagePath}"
                        }
                    };

                    cmdProcess.Start();
                    cmdProcess.WaitForExit();
                    cmdProcess.Close();

                }

                _meta = new MetaProvider(_imagePath);
                var size = _meta.GetSize();
                if (!size.IsEmpty)
                    context.SetPreferredSizeFit(size, 0.8);
                else
                    context.PreferredSize = new Size(800, 600);

                context.Theme = (Themes)SettingHelper.Get("LastTheme", 1, "QuickLook.Plugin.ImageViewer");
            }
        }

        public void View(string path, ContextObject context)
        {
            if (NotFound)
            {
                var viewer = new Label { Content = "default open with?\nplan A: Set default open with  \nplan B: open %temp%\\quick-look-ase\nmodify config file Add the path of your Aseprite.exe" };

                context.ViewerContent = viewer;
                context.Title = $"{Path.GetFileName(path)}";

                context.IsBusy = false;
            }
            else
            {
                _image = new ImagePanel
                {
                    ContextObject = context,
                    Meta = _meta,
                    Theme = context.Theme,
                    RenderMode = BitmapScalingMode.NearestNeighbor,
                    MaxZoomFactor = 64d
                };

                var size = _meta.GetSize();
                context.ViewerContent = _image;
                context.Title = size.IsEmpty
                    ? $"{Path.GetFileName(path)}"
                    : $"{size.Width}×{size.Height}: {Path.GetFileName(path)}";

                var uri = new Uri(_imagePath);
                _image.ImageUriSource = uri;
            }

        }

        public void Cleanup()
        {
            _image?.Dispose();
            _image = null;
        }

        private string GetAseRegFileOpenWithPath()
        {
            var classesRoot = Registry.ClassesRoot;
            var software = classesRoot.OpenSubKey("AsepriteFile\\shell\\open\\command");
            if (software == null)
            {
                software = classesRoot.OpenSubKey("aseprite_auto_file\\shell\\open\\command");
                if (software == null)
                    return default;
            }

            var value = ((string)software.GetValue(""))?.Replace("\"", "").Replace("%1", "").TrimEnd();
            return value;
        }

        public static string GetMd5(string filename)
        {
            var md5Hash = MD5.Create();
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(filename));
            var str = new StringBuilder();
            foreach (var t in data)
            {
                str.Append(t.ToString("x2"));
            }
            return str.ToString();
        }
    }
}