using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RavenLinqpadDriver
{
    static class Extensions
    {
        public static IEnumerable<Type> TypesImplementing<T>(this Assembly a)
        {
            try
            {
                return a.GetTypes().Where(t => typeof(T).IsAssignableFrom(t)).ToArray();
            }
            catch
            {
                return new Type[0];
            }
        }
    }
}
