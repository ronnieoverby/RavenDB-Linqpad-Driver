using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Ionic.Zip;
using System.Reflection;
using System.Diagnostics;

namespace RavenLinqpadDriver.Package
{
    class Program
    {
        static void Main()
        {
            // get paths to everything
            var basePath = Environment.CurrentDirectory.Up(3);
            var driverPath = Path.Combine(basePath, @"RavenLinqpadDriver\bin\Release");
            var commonPath = Path.Combine(basePath, @"RavenLinqpadDriver.Common\bin\Release");
            var releaseDir = Path.Combine(basePath, "Release", "");
            Directory.CreateDirectory(releaseDir);

            // get version from driver assembly
            var assemblyPath = Path.Combine(driverPath, "RavenLinqpadDriver.dll");
            var assembly = Assembly.LoadFile(assemblyPath);
            var version = assembly.GetName().Version;

            // zip up both release folders as lpx files	
            using (var ms = new MemoryStream())
            using (var releaseZip = new ZipFile())
            using (var lpx40 = new ZipFile())
            {
                var releaseZipPath = Path.Combine(releaseDir, string.Format("RavenLinqpadDriver {0}.zip", version));

                // 40 driver lpx file
                lpx40.AddFiles(GetFiles(driverPath), "");
                lpx40.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                releaseZip.AddEntry("RavenLinqpadDriver.lpx", ms);

                // reset the memorystream	
                releaseZip.Save(releaseZipPath);
                ms.SetLength(0);

                // common
                releaseZip.AddFile(Path.Combine(commonPath, "RavenLinqpadDriver.Common.dll"), "Common");

                // readme
                releaseZip.AddFile(Path.Combine(basePath, "readme.md"), "");
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
