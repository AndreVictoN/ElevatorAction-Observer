using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClassDiagramGenerator
{
    public class ScriptEntry
    {
        public string Path;
        public bool IsSelected = true;
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Directory => System.IO.Path.GetDirectoryName(Path);
    }

    public class ScriptSelectionManager
    {
        public List<ScriptEntry> Scripts { get; private set; } = new List<ScriptEntry>();

        public void Scan(string folder, string extension = ".cs")
        {
            Scripts.Clear();
            if (!Directory.Exists(folder)) return;
            var files = Directory.GetFiles(folder, "*" + extension, SearchOption.AllDirectories);
            Scripts = files.Select(f => new ScriptEntry { Path = f, IsSelected = true }).ToList();
        }

        public List<ScriptEntry> GetSelected() => Scripts.Where(s => s.IsSelected).ToList();
    }
}