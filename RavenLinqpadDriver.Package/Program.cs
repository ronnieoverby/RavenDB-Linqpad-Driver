using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zip;
using System.Reflection;
using System.Diagnostics;

namespace RavenLinqpadDriver.Package
{
    class Program
    {
        static void Main(string[] args)
        {
            // get paths to everything
            var basePath = Environment.CurrentDirectory.Up(3);
            var driver40Path = Path.Combine(basePath, @"RavenLinqpadDriver\bin\Release");
            var driver35Path = Path.Combine(basePath, @"RavenLinqpadDriver35\bin\Release");
            var bridge40Path = Path.Combine(basePath, @"RavenLinqpadDriver.Bridge\bin\Release");
            var bridge35Path = Path.Combine(basePath, @"RavenLinqpadDriver.Bridge35\bin\Release");
            var releaseDir = Path.Combine(basePath, "Release", "");
            Directory.CreateDirectory(releaseDir);

            // get version from driver assembly
            var assemblyPath = Path.Combine(driver40Path, "RavenLinqpadDriver.dll");
            var assembly = Assembly.LoadFile(assemblyPath);
            var version = assembly.GetName().Version;

            // zip up both release folders as lpx files	
            using (var ms = new MemoryStream())
            using (var releaseZip = new ZipFile())
            using (var lpx40 = new ZipFile())
            using (var lpx35 = new ZipFile())
            {
                var releaseZipPath = Path.Combine(releaseDir, string.Format("RavenLinqpadDriver {0}.zip", version));

                // 40 driver lpx file
                lpx40.AddFiles(GetFiles(driver40Path), "");
                lpx40.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                releaseZip.AddEntry("RavenLinqpadDriver.lpx", ms);

                // reset the memorystream	
                releaseZip.Save(releaseZipPath);
                ms.SetLength(0);

                // 35 driver lpx file
                lpx35.AddFiles(GetFiles(driver35Path), "");
                lpx35.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                releaseZip.AddEntry("RavenLinqpadDriver35.lpx", ms);

                // bridges
                releaseZip.AddFiles(new[]
                {
                 Path.Combine( bridge35Path,  "RavenLinqpadDriver.Bridge35.dll"),
                    Path.Combine(bridge40Path,"RavenLinqpadDriver.Bridge.dll")
                }, false, "Bridge");

                // readme
                releaseZip.AddFile(Path.Combine(basePath, "readme.txt"), "");
                releaseZip.Save(releaseZipPath);
            }

            // open
            Process.Start(releaseDir);
        }

        static IEnumerable<string> GetFiles(string dirPath)
        {
            var di = new DirectoryInfo(dirPath);
            return di.GetFiles().Select(x => x.FullName);
        }
    }
}
