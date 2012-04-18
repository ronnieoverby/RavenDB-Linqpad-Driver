using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RavenLinqpadDriver.Package
{
    static class Extensions
    {
        public static string Up(this string path, int n = 1)
        {
            var di = new DirectoryInfo(path);

            for (int i = 0; i < n; i++)
                di = di.Parent;

            return di.FullName;
        }
    }
}
