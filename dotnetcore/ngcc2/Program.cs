using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CommandLine;
using Newtonsoft.Json;
using ngcc2.Models;

namespace ngcc2
{
    class Program
    {
        /// <summary>
        /// Zero = success, non-Zero = failure
        /// </summary>
        static int exitCode = 0;

        /// <summary>
        /// List of packages
        /// </summary>
        static readonly List<NugetInfo> info = new List<NugetInfo>();

        /// <summary>
        /// Compliance Report
        /// <para>This was intended for orgs that need to track what libraries and versions are in use</para>
        /// </summary>
        /// <param name="reportName">Report Name</param>
        /// <param name="folder">Where did we start looking</param>
        static void Compliance(string reportName, string folder)
        {
            reportName = Path.ChangeExtension(reportName, ".html");
            if (File.Exists(reportName)) File.Delete(reportName);

            if (info.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                var last_id = string.Empty;
                var last_v = string.Empty;

                var data = info.AsQueryable<NugetInfo>();
                var results = data.OrderBy(p => p.Id).ThenBy(p => p.Version)
                    .Select(p => new { p.Id, p.Version, p.ProjectFile }).ToList();

                sb.Append(ComplianceFormat.Top);

                sb.Append("<h1>Compliance Report</h1>\n");

                foreach(var item in results)
                {
                    if(item.Id != last_id)
                    {
                        sb.Append("<h2>" + item.Id + "</h2>\n");
                        last_id = item.Id;
                        last_v = string.Empty;
                    }

                    if(item.Version != last_v)
                    {
                        sb.Append("<h3>" + item.Version + "</h3>\n");
                        last_v = item.Version;
                    }

                    sb.Append(item.ProjectFile.Replace(folder, ""));
                    sb.Append("<br/>\n");
                }

                sb.Append(ComplianceFormat.Bottom);

                File.WriteAllText(reportName, sb.ToString());
            }

            Console.WriteLine("Compliance: {0}", reportName);
        }

        /// <summary>
        /// Dump of data structure for developers
        /// </summary>
        /// <param name="reportName">reportName</param>
        static void Dump(string reportName)
        {
            reportName = Path.ChangeExtension(reportName, ".json");
            if (File.Exists(reportName)) File.Delete(reportName);

            var json = JsonConvert.SerializeObject(info);

            File.WriteAllText(reportName, json);

            Console.WriteLine("Dump: {0}", reportName);
        }

        /// <summary>
        /// Main Report Generator (DEV Centric)
        /// </summary>
        /// <param name="folder">Where did we start looking</param>
        /// <param name="reportName">Path to report file</param>
        static void Report(string folder, string reportName)
        {
            reportName = Path.ChangeExtension(reportName, ".txt");
            if (File.Exists(reportName)) File.Delete(reportName);

            if (info.Count > 0)
            {
                var data = info.AsQueryable<NugetInfo>();
                var results = data.OrderBy(p => p.TargetFramework).ThenBy(p => p.Id).ThenBy(p => p.Version).ThenBy(p => p.ProjectFile).Select(p => new { p.TargetFramework, p.Id, p.Version, p.ProjectFile }).ToList();

                string last_T = null;
                string last_I = string.Empty;
                string last_V = string.Empty;

                using (System.IO.StreamWriter outs = new System.IO.StreamWriter(reportName))
                {
                    outs.WriteLine("Base Folder: {0}\n", folder);

                    foreach (var r in results)
                    {
                        bool doit = false;
                        if (r.TargetFramework != last_T)
                        {
                            outs.WriteLine("Target Framework: {0}", r.TargetFramework);
                            last_T = r.TargetFramework;
                            doit = true;
                        }

                        if(r.Id != last_I)
                        {
                            outs.WriteLine("\tPackage: {0}", r.Id);
                            last_I = r.Id;
                            doit = true;
                        }
                        if (r.Version != last_V)
                        {
                            outs.WriteLine("\t\tVersion: {0}", r.Version);
                            last_V = r.Version;
                            doit = true;
                        }
                        if(doit) outs.WriteLine("\t\t\t{0}", r.ProjectFile.Replace(folder, ""));
                    }
                }
                
                Console.WriteLine("Report: {0}", reportName);
            } else
            {
                Console.Error.WriteLine("No *.csproj files found");
                exitCode = 2;
            }
        }

        /// <summary>
        /// Process a packages.json file to enrich metadata
        /// </summary>
        /// <param name="fi">FileInfo</param>
        static void Process(FileInfo fi, bool verbose = false)
        {
            XElement doc =  XElement.Load(fi.FullName);

            IEnumerable<XElement> itemGroups =
                from el in doc.Elements()
                where el.Name == "ItemGroup"
                select el;

            foreach (var ig in itemGroups)
            {
                IEnumerable<XElement> packages =
                    from pr in ig.Elements()
                    where pr.Name == "PackageReference"
                    select pr;
                
                foreach (var pk in packages)
                {
                    var dep = pk.Attribute("Include").Value;
                    
                    var ver = string.Empty;
                    if (pk.Attribute("Version") != null) ver = pk.Attribute("Version").Value;
                    
                    var ngi = new Models.NugetInfo()
                    {
                        Id = dep,
                        Version = ver,
                        ProjectFile = fi.FullName,
                        TargetFramework = "Core 3.1 LTS"
                    };

                    if (verbose) Console.WriteLine($"{ngi}");

                    info.Add(ngi);
                }
            }

        }

        /// <summary>
        /// Find Packages Files
        /// </summary>
        /// <param name="dirInfo">DirectoryInfo</param>
        static void FindPackages(DirectoryInfo dirInfo, bool verbose = false)
        {
            foreach (var fi in dirInfo.GetFiles("*.csproj", SearchOption.TopDirectoryOnly))
            {
                if(verbose) Console.WriteLine($"FindPackages({fi.FullName})");
                Process(fi, verbose);
            }
        }

        /// <summary>
        /// Recursive Traverse
        /// </summary>
        /// <param name="dirInfo">DirectoryInfo</param>
        static void Traverse(DirectoryInfo dirInfo, bool verbose = false)
        {
            foreach (var di in dirInfo.GetDirectories())
            {
                if (verbose) Console.WriteLine($"Traverse({di.FullName})");
                FindPackages(di, verbose);
                Traverse(di, verbose);
            }
        }

        /// <summary>
        /// Handle Command Line Parsing Errors
        /// </summary>
        /// <param name="errors">(sic)</param>
        private static void HandleParseError(IEnumerable<Error> errors)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in errors) sb.AppendLine(e.Tag.ToString());
            Console.Error.WriteLine("{0}", sb.ToString());
        }

        /// <summary>
        /// Do main logic e.g. command line swiches were ok
        /// </summary>
        /// <param name="opts">Options</param>
        private static void RunOptionsAndReturnExitCode(NgccOptions opts)
        {
            if (opts is null)
            {
                throw new ArgumentNullException(nameof(opts));
            }

            if (!Directory.Exists(opts.Folder))
            {
                Console.Error.WriteLine("No such folder: {0}", opts.Folder);
                exitCode = 1;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(opts.Report)) opts.Report = ".\\NGCC2.txt";
 
                var di = new DirectoryInfo(opts.Folder);
                FindPackages(di, opts.Verbose);
                Traverse(di, opts.Verbose);

                Report(opts.Folder, opts.Report);

                if (opts.Dump)
                {
                    Dump(opts.Report);
                }

                if(opts.Compliance)
                {
                    Compliance(opts.Report, opts.Folder);

                }

                Environment.ExitCode = exitCode;
            }

        }
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<NgccOptions>(args)
                .WithParsed<NgccOptions>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<NgccOptions>((errs) => HandleParseError(errs));
        }
    }
}
