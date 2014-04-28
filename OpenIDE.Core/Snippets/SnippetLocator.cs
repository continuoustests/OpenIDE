using System;
using System.Collections.Generic;
using System.IO;
using OpenIDE.Core.Profiles;

namespace OpenIDE.Core.Snippets
{
    public class Snippet
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public Snippet(string path) {
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Path = path;
        }
    }

    public class SnippetLocator
    {
        private string _token;

        public SnippetLocator(string token) {
            _token = token;
        }

        public List<Snippet> GetSnippets() {
            var list = new List<Snippet>();
            var locator = new ProfileLocator(_token);
            var paths = locator.GetPathsCurrentProfiles();
            foreach (var path in paths) {
                var dir = System.IO.Path.Combine(path, "snippets");
                if (Directory.Exists(dir))
                    addFiles(list, dir);
                scanLanguages(list, path);
            }
            return list;
        }

        private void scanLanguages(List<Snippet> list, string path) {
            var languageDir = System.IO.Path.Combine(path, "languages");
            if (!Directory.Exists(languageDir))
                return;
            var dirs = Directory.GetDirectories(languageDir);
            foreach (var dir in dirs) {
                var snippetDir = System.IO.Path.Combine(dir, "snippets");
                if (!Directory.Exists(snippetDir))
                    continue;
                addFiles(list, snippetDir);
            }
        }

        private void addFiles(List<Snippet> list, string path) {
            var files = Directory.GetFiles(path, "*.snippet");
            foreach (var file in files)
                list.Add(new Snippet(file));
        }
    }
}