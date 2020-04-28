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

        [Option('d', "dump", Default = false, HelpText = "Dump data to the file.json")]
        public bool Dump { get; set; }

        [Option('c', "Compliance", Default = false, HelpText = "Compliance Report")]
        public bool Compliance { get; set; }
    }
}
