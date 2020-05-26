using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        static IHttpClientFactory _httpClientFactory;
        private static Dictionary<string, string> NuGetVersionList = new Dictionary<string, string>();

        private static readonly string[] VersionAnnotationsToExclude = new string[] { "preview", "pre", "alpha", "beta", "m", "rc", "final", "dev" };

        #region "Reports and Exports"

        /// <summary>
        /// Compliance Report
        /// <para>This was intended for orgs that need to track what libraries and versions are in use</para>
        /// </summary>
        /// <param name="reportName">Report Name</param>
        /// <param name="folder">Where did we start looking</param>
        static void DumpHtml(string reportName, string folder)
        {
            reportName = Path.ChangeExtension(reportName, ".html");
            if (File.Exists(reportName)) File.Delete(reportName);

            if (info.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                var last_id = string.Empty;
                var last_v = string.Empty;

                var data = info.AsQueryable<NugetInfo>();
                var results = data.OrderBy(p => p.Id).ThenBy(p => p.Major).ThenBy(p => p.Minor).ThenBy(p => p.Build).ThenBy(p => p.ProjectFile)
                    .Select(p => new Models.NugetInfo()
                    {
                        Version = string.IsNullOrWhiteSpace(p.Version) ? "0.0.0" : p.Version,
                        Id = p.Id,
                        ProjectFile = p.ProjectFile,
                        LatestVersion = p.LatestVersion
                    }).ToList();

                sb.Append(HtmlReportResource.Top);

                sb.Append($"<h1>NuGet Report: {DateTime.Now:f}</h1>\n");

                sb.Append($"<p>Folder: {folder}</p>");

                foreach (var item in results)
                {
                    if (item.Id != last_id)
                    {
                        if (!string.IsNullOrWhiteSpace(last_id))
                        {
                            sb.Append("</ul>");
                        }
                        sb.Append("<h2>" + item.Id + "</h2>\n");
                        last_id = item.Id;
                        last_v = string.Empty;
                    }

                    if (item.Version != last_v)
                    {
                        if (!string.IsNullOrWhiteSpace(last_v))
                        {
                            sb.Append("</ul>");
                        }

                        var stigil = " is latest";
                        if (!item.Version.Equals(item.LatestVersion))
                        {
                            stigil = $" (old), latest: {item.LatestVersion}";
                        }

                        sb.Append($"<h3>{item.Version} {stigil}</h3>\n");
                        last_v = item.Version;
                        sb.Append("<ul>");
                    }

                    sb.Append("<li class='pfile'>");
                    sb.Append(item.ProjectFile.Replace(folder, ""));
                    sb.Append("</li>\n");
                }

                sb.Append("</ul>");
                sb.Append(HtmlReportResource.Bottom);

                File.WriteAllText(reportName, sb.ToString());
            }

            Console.WriteLine("HTML: {0}", reportName);
        }

        /// <summary>
        /// Dump of data structure for developers
        /// </summary>
        /// <param name="reportName">reportName</param>
        static void DumpJson(string reportName)
        {
            reportName = Path.ChangeExtension(reportName, ".json");
            if (File.Exists(reportName)) File.Delete(reportName);

            var json = JsonConvert.SerializeObject(info);

            File.WriteAllText(reportName, json);

            Console.WriteLine("JSON: {0}", reportName);
        }

        /// <summary>
        /// Dump as CSV for Excel Sorting and Filtering
        /// </summary>
        /// <param name="reportName"></param>
        static void DumpCsv(string reportName)
        {
            reportName = Path.ChangeExtension(reportName, ".csv");
            if (File.Exists(reportName)) File.Delete(reportName);
            if (info.Count > 0)
            {
                var data = info.AsQueryable<NugetInfo>();
                var results = data.OrderBy(p => p.Id).ThenBy(p => p.Major).ThenBy(p => p.Minor).ThenBy(p => p.Build).ThenBy(p => p.ProjectFile)
                    .Select(p => new { p.Id, p.Version, p.LatestVersion, p.ProjectFile }).ToList();

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(reportName))
                {
                    file.Write('"');
                    file.Write("NuGet Package");
                    file.Write('"');
                    file.Write(',');
                    file.Write('"');
                    file.Write("Version");
                    file.Write('"');
                    file.Write(',');
                    file.Write('"');
                    file.Write("Latest");
                    file.Write('"');
                    file.Write(',');
                    file.Write('"');
                    file.Write("CsProj");
                    file.WriteLine('"');

                    foreach (var item in results)
                    {
                        file.Write('"');
                        file.Write(item.Id);
                        file.Write('"');
                        file.Write(",");
                        file.Write('"');
                        file.Write(item.Version);
                        file.Write('"');
                        file.Write(",");
                        file.Write('"');
                        file.Write(item.LatestVersion);
                        file.Write('"');
                        file.Write(",");
                        file.Write('"');
                        file.Write(item.ProjectFile);
                        file.WriteLine('"');
                    }
                }
                Console.WriteLine("CSV: {0}", reportName);
            }
        }

        #endregion

        #region "Processing"

        /// <summary>
        /// Process a packages.json file to enrich metadata
        /// </summary>
        /// <param name="fi">FileInfo</param>
        static void Process(FileInfo fi, bool verbose = false)
        {
            XElement doc = XElement.Load(fi.FullName);

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

                    string latest = string.Empty;
                    if (!NuGetVersionList.ContainsKey(dep))
                    {
                        latest = LastVersion(dep);
                        if (!string.IsNullOrEmpty(latest))
                        {
                            NuGetVersionList.Add(dep, latest);
                        }
                    }
                    else
                    {
                        latest = NuGetVersionList[dep];
                    }

                    var ngi = new Models.NugetInfo()
                    {
                        Id = dep,
                        Version = ver,
                        LatestVersion = latest,
                        ProjectFile = fi.FullName
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
                if (verbose) Console.WriteLine($"FindPackages({fi.FullName})");
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
        /// Find latest version of package
        /// </summary>
        /// <param name="nugetPackageName">NuGet Package Name</param>
        /// <returns></returns>
        static string LastVersion(string nugetPackageName)
        {

            string lastVersion = string.Empty;
            string path = string.Empty;

            try
            {
                using (var webClient = _httpClientFactory.CreateClient())
                {
                    webClient.BaseAddress = new Uri("https://api.nuget.org");
                    path = $"/v3-flatcontainer/{nugetPackageName}/index.json";

                    var json = webClient.GetStringAsync(path).GetAwaiter().GetResult();

                    JObject jo = JObject.Parse(json);

                    var okVersions = new List<string>();

                    var versions = (from p in jo["versions"]
                                   select p.Value<string>()).ToList();

                    foreach(var v in versions)
                    {
                        bool isOk = true;
                        foreach(var e in VersionAnnotationsToExclude)
                        {
                            if(v.Contains(e))
                            {
                                isOk = false;
                                break;
                            }
                        }

                        if(isOk)
                        {
                            okVersions.Add(v);
                        }
                    }

                    lastVersion = okVersions.LastOrDefault();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"{path}: {ex.Message}");
                lastVersion = string.Empty;
            }
            return lastVersion;
        }

        #endregion

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

                if (opts.Dump)
                {
                    DumpJson(opts.Report);
                }

                if (opts.WebReport)
                {
                    DumpHtml(opts.Report, opts.Folder);
                }

                if (opts.SimpleCsv)
                {
                    DumpCsv(opts.Report);
                }

                Environment.ExitCode = exitCode;
            }

        }

        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            CommandLine.Parser.Default.ParseArguments<NgccOptions>(args)
                .WithParsed<NgccOptions>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<NgccOptions>((errs) => HandleParseError(errs));
        }
    }
}
