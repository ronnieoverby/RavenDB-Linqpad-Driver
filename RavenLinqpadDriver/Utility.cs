using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenLinqpadDriver
{
    public static class Utility
    {
        public static bool IsNullOrWhitespace(this string source)
        {
            return source == null || source.Trim() == string.Empty;
            
        }
    }
}
