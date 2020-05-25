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
        /// Latest Version from NuGet Org
        /// </summary>
        public string LatestVersion { get; set; }

        private int GetValue(int index)
        {
            int value = 0;
            if (!string.IsNullOrWhiteSpace(this.Version))
            {
                var segs = this.Version.Split(new char[] { '.' }, System.StringSplitOptions.None);
                if (segs.Length > index)
                {
                    if (int.TryParse(segs[index], out int temp)) value = temp;
                }
            }
            return value;

        }

        /// <summary>
        /// Parsed Major Version
        /// </summary>
        public int Major
        {
            get
            {
                return GetValue(0);
            }
        }

        /// <summary>
        /// Parsed Minor Version
        /// </summary>
        public int Minor
        {
            get
            {
                return GetValue(1);
            }
        }

        /// <summary>
        /// Parsed Buid Version
        /// </summary>
        public int Build
        {
            get
            {
                return GetValue(2);
            }
        }

        public override string ToString()
        {
            var proj = string.Empty;
            if (!string.IsNullOrWhiteSpace(this.ProjectFile))
            {
                var fi = new FileInfo(this.ProjectFile);
                proj = fi.Name;
            }
            return $"{proj}: {Id} {Version}";
        }
    }
}
