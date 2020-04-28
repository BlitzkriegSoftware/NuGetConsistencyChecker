using System.IO;

namespace ngcc2.Models
{
    /// <summary>
    /// Data Stucture to Hold NuGet info
    /// </summary>
    public class NugetInfo
    {
        /// <summary>
        /// What folder are we consuming the package in?
        /// </summary>
        public string ProjectFile { get; set; }
        /// <summary>
        /// Package Name
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Version of Package
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Version of .NET 
        /// </summary>
        public string TargetFramework { get; set; }

        public override string ToString()
        {
            var proj = string.Empty;
            if(!string.IsNullOrWhiteSpace(this.ProjectFile))
            {
                var fi = new FileInfo(this.ProjectFile);
                proj = fi.Name;
            }
            return $"{proj}: {Id} {Version}";
        }
    }
}
