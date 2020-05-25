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

        [Option('h', "html", Default = true, HelpText = "HTML Report")]
        public bool WebReport { get; set; }

        [Option('c', "csv", Default = true, HelpText ="CSV Export")]
        public bool SimpleCsv { get; set; }

    }
}
