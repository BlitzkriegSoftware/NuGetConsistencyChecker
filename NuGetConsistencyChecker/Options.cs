using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
namespace NuGetConsistencyChecker
{
    public class Options
    {
        [Option('f', "folder", Default = ".\\", HelpText = "Folder to start in")]
        public string Folder { get; set; }

        [Option('o', "output", Default = "", HelpText = "Path\\{filename}.txt for report")]
        public string Report { get; set; }

        [Option('d', "dump", Default = false, HelpText="Dump data to the file.json")]
        public bool Dump { get; set; }

        [Option('c', "Compliance", Default = false, HelpText = "Complaince Report" )]
        public bool Compliance { get; set; }
    }
}
