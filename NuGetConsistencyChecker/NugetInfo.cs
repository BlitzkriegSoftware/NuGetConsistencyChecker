using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetConsistencyChecker
{
    public class NugetInfo
    {
        public string ProjectFolder { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string TargetFramework { get; set; }
    }
}
