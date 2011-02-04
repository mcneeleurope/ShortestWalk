using System;
using System.Reflection;
using Grasshopper.Kernel;
using ShortestWalk.Gh.Properties;

namespace ShortestWalk
{
    [CLSCompliant(false)]
    public class ShortestWalk : GH_AssemblyInfo
    {
        public override string AssemblyDescription
        {
            get
            {
                return "Finds the shortest path in a network of curves";
            }
        }

        public override System.Drawing.Bitmap AssemblyIcon
        {
            get
            {
                return Resources.shortest_walk24;
            }
        }

        public override GH_LibraryLicense AssemblyLicense
        {
            get
            {
                return GH_LibraryLicense.opensource;
            }
        }

        public override string AssemblyName
        {
            get
            {
                return "Shortest Walk in Gh";
            }
        }

        public override string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public override string AuthorContact
        {
            get
            {
                return "giulio@mcneel.com";
            }
        }

        public override string AuthorName
        {
            get
            {
                return "McNeel Europe";
            }
        }
    }
}
