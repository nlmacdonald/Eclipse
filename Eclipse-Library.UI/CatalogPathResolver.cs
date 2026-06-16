using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Eclipse_Library.UI
{
    public sealed class CatalogPathResolver
    {
        public string Find(params string[] relativeParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                // Docs files may be created by the app, so finding the docs directory is enough.
                if (relativeParts.Length >= 1 && string.Equals(relativeParts[0], "docs", StringComparison.OrdinalIgnoreCase))
                {
                    var docsDir = Path.Combine(directory.FullName, "docs");
                    if (Directory.Exists(docsDir))
                    {
                        return Path.Combine(new[] { docsDir }.Concat(relativeParts.Skip(1)).ToArray());
                    }
                }

                var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeParts).ToArray());
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return Path.GetFullPath(Path.Combine(new[] { AppContext.BaseDirectory }.Concat(relativeParts).ToArray()));
        }

        public string FindDocsFile(string fileName)
        {
            return Find("docs", fileName);
        }
    }
}
