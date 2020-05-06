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

        [Option('w', "html", Default = true, HelpText = "HTML Report")]
        public bool WebReport { get; set; }

        [Option('s', "simple-csv", Default = true, HelpText ="Simple alpha list of all NuGets")]
        public bool SimpleCsv { get; set; }

        [Option('t', "text-file", Default = false, HelpText ="Plain Text File")]
        public bool PlainText { get; set; }
    }
}
