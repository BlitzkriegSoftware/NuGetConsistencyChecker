using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetConsistencyChecker
{
    /// <summary>
    /// Data Stucture to Hold NuGet info
    /// </summary>
    public class NugetInfo
    {
        /// <summary>
        /// What folder are we consuming the package in?
        /// </summary>
        public string ProjectFolder { get; set; }
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
    }
}
