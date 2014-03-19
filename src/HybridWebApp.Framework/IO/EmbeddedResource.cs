using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.IO
{
    public class EmbeddedResource
    {
        public static async Task<string> ReadAsStringAsync(Assembly assembly, string path)
        {
            var embeddedStream = assembly.GetManifestResourceStream(path);

            using (var sr = new StreamReader(embeddedStream))
            {
                return await sr.ReadToEndAsync();
            }
        }

        public static string ReadAsString(Assembly assembly, string path)
        {
            var embeddedStream = assembly.GetManifestResourceStream(path);

            using (var sr = new StreamReader(embeddedStream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
