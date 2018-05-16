using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CommandLine;

using Newtonsoft.Json;

namespace NuGetConsistencyChecker
{
    class Program
    {
        // Zero = success, non-Zero = failure
        static int exitCode = 0;

        static List<NugetInfo> info = new List<NugetInfo>();

        static void Compliance(string reportName)
        {
            reportName = Path.ChangeExtension(reportName, ".htm");
            if (File.Exists(reportName))
            {
                try
                {
                    File.Delete(reportName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Can't delete file {0} -> {1}", reportName, ex.Message);
                    exitCode = 5;
                    return;
                }
            }

            if (info.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                var last_id = string.Empty;
                var last_v = string.Empty;

                var data = info.AsQueryable<NugetInfo>();
                var results = data.OrderBy(p => p.Id).ThenBy(p => p.Id).ThenBy(p => p.Version).Select(p => new { p.Id, p.Version, p.ProjectFolder }).ToList();

                sb.Append(ComplianceFormat.Top);

                sb.Append("<h1>Compliance Report</h1>");

                foreach(var item in results)
                {
                    if(item.Id != last_id)
                    {
                        sb.Append("<h2>" + item.Id + "</h2>");
                        last_id = item.Id;
                        last_v = string.Empty;
                    }

                    if(item.Version != last_v)
                    {
                        sb.Append("<h3>" + item.Version + "</h3>");
                        last_v = item.Version;
                    }

                    sb.Append(item.ProjectFolder);
                    sb.Append("<br/>");
                }

                sb.Append(ComplianceFormat.Bottom);

                File.WriteAllText(reportName, sb.ToString());
            }

            Console.WriteLine("Dump: {0}", reportName);
        }


        static void Dump(string reportName)
        {
            reportName = Path.ChangeExtension(reportName, ".json");
            if (File.Exists(reportName))
            {
                try
                {
                    File.Delete(reportName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Can't delete file {0} -> {1}", reportName, ex.Message);
                    exitCode = 4;
                    return;
                }
            }

            var json = JsonConvert.SerializeObject(info);

            File.WriteAllText(reportName, json);

            Console.WriteLine("Dump: {0}", reportName);
        }

        static void Report(string dirName, string reportName)
        {
            reportName = Path.ChangeExtension(reportName, ".txt");
            if (File.Exists(reportName))
            {
                try
                {
                    File.Delete(reportName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Can't delete file {0} -> {1}", reportName, ex.Message);
                    exitCode = 3;
                    return;
                }
            }

            if (info.Count > 0)
            {
                var data = info.AsQueryable<NugetInfo>();
                var results = data.OrderBy(p => p.TargetFramework).ThenBy(p => p.Id).ThenBy(p => p.Version).ThenBy(p => p.ProjectFolder).Select(p => new { p.TargetFramework, p.Id, p.Version, p.ProjectFolder }).ToList();

                string last_T = null;
                string last_I = string.Empty;
                string last_V = string.Empty;

                using (System.IO.StreamWriter outs = new System.IO.StreamWriter(reportName))
                {
                    outs.WriteLine("Base Folder: {0}\n", dirName);

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
                        if(doit) outs.WriteLine("\t\t\t{0}", r.ProjectFolder.Replace(dirName, ""));
                    }
                }
                
                Console.WriteLine("Report: {0}", reportName);
            } else
            {
                Console.Error.WriteLine("No Package.config files found");
                exitCode = 2;
            }
        }

        static void Process(FileInfo fi)
        {
            string data = string.Empty;
            var result = new packages();

            try
            {
                data = File.ReadAllText(fi.FullName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Can't process '{0}' -> {1}", fi.FullName, ex.Message);
                return;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(packages));

                using (var stream = new StringReader(data))
                using (var reader = XmlReader.Create(stream))
                {
                    result = (packages)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Can't deserialize '{0}' -> {1}", fi.FullName, ex.Message);
                return;

            }

            // Process info here
            foreach(DataRow dr in result.Tables[0].Rows)
            {
                var i = new NugetInfo()
                {
                    Id = dr[0].ToString(),
                    Version = dr[1].ToString(),
                    ProjectFolder = fi.DirectoryName
                };

                if (dr.ItemArray.Length > 2)
                    i.TargetFramework = dr[2].ToString();
                else
                    i.TargetFramework = string.Empty;

                info.Add(i);
            }
        }

        static void FindPackages(DirectoryInfo dirInfo)
        {
            foreach (var fi in dirInfo.GetFiles("packages.config", SearchOption.TopDirectoryOnly))
            {
                Process(fi);
            }
        }

        static void Traverse(DirectoryInfo dirInfo)
        {
            foreach (var di in dirInfo.GetDirectories())
            {
                FindPackages(di);
                Traverse(di);
            }
        }

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in errors) sb.AppendLine(e.Tag.ToString());
            Console.Error.WriteLine("{0}", sb.ToString());
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {

            if (!Directory.Exists(opts.Folder))
            {
                Console.Error.WriteLine("No such folder: {0}", opts.Folder);
                exitCode = 1;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(opts.Report)) opts.Report = ".\\NuGetConsistencyChecker.txt";
 
                var di = new DirectoryInfo(opts.Folder);
                FindPackages(di);
                Traverse(di);

                Report(opts.Folder, opts.Report);

                if (opts.Dump)
                {
                    Dump(opts.Report);
                }

                if(opts.Compliance)
                {
                    Compliance(opts.Report);

                }

                Environment.ExitCode = exitCode;
            }

        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }
    }
}
