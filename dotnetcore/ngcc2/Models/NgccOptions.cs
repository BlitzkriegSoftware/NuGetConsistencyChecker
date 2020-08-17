using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace ngcc2.Models
{
    public class NgccOptions
    {
        [Option('v',"verbose", Default = false, HelpText ="Enable Verbose")]
        public bool Verbose { get; set; }

        [Option('f', "folder", Default = ".\\", HelpText = "Folder to start in")]
        public string Folder { get; set; }

        [Option('o', "output", Default = "", HelpText = "Path\\{filename}.txt for report")]
        public string Report { get; set; }

        [Option('j', "json", Default = false, HelpText = "JSON Export")]
        public bool Dump { get; set; }

        [Option('h', "html", Default = false, HelpText = "HTML Report")]
        public bool WebReport { get; set; }

        [Option('c', "csv", Default = true, HelpText ="CSV Export")]
        public bool SimpleCsv { get; set; }

        [Option('e', "Exclude Current", Default = false, HelpText = "Exclude Current NuGet Packages")]
        public bool ExcludeCurrent { get; set; } = false;

        [Option('n', "NuGet Configuration Files", Default = "ngcc2-urls.txt", HelpText ="NuGet Configuration File")]
        public string NuGetConfigFile { get; set; }

        private List<string> _nugets = null;

        public List<string> NuGets
        {
            get
            {
                if (_nugets == null)
                {
                    _nugets = new List<string>();

                    var recs = File.ReadAllLines(this.NuGetConfigFile);
                    foreach (var s in recs)
                    {
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            var row = s.Trim();
                            if (!row.StartsWith('#'))
                            {
                                _nugets.Add(row);
                            }
                        }
                    }

                }
                return _nugets;
            }
        }
    }
}
