using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ShortestWalk.Geometry;
using ShortestWalk.Gh.Properties;
using Grasshopper.Kernel.Types;

namespace ShortestWalk.Gh
{
    [Obsolete("There is a newer component with more functionality")]
    [CLSCompliant(false)]
    public class ShortestWalkComponent_OBSOLETE : GH_Component
    {
        public ShortestWalkComponent_OBSOLETE()
            : base("Shortest Walk (Obsolete)", "ShortWalk", "Calculates the shortest walk in a network of curves",
            "Curve", "Util")
        { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_CurveParam("Curves", "C", "The curves group", GH_ParamAccess.list);
            pManager.Register_LineParam("Wanted path", "L", "The lines from the start to the end of the path", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_CurveParam("Shortest walk", "W", "The shortest connections");
        }

        static Predicate<Curve> _removeNullAndInvalidDelegate = RemoveNullAndInvalid;
        static Predicate<Line> _removeInvalidDelegate = RemoveInvalid;

        private static bool RemoveNullAndInvalid(Curve obj)
        {
            return obj == null || !obj.IsValid;
        }

        private static bool RemoveInvalid(Line obj)
        {
            return !obj.IsValid;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curves = new List<Curve>();
            var lines = new List<Line>();

            if (DA.GetDataList(0, curves) && DA.GetDataList(1, lines))
            {
                curves.RemoveAll(_removeNullAndInvalidDelegate);
                lines.RemoveAll(_removeInvalidDelegate);

                if (curves.Count < 1)
                    return;

                CurvesTopology top = new CurvesTopology(curves, GH_Component.DocumentTolerance());

                var distances = top.MeasureAllEdgeLengths();

                List<Curve> result = new List<Curve>();

                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];

                    int fromIndex = top.GetClosestNode(line.From);
                    int toIndex = top.GetClosestNode(line.To);

                    if (fromIndex == toIndex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The start and end positions are equal");
                        result.Add(null);
                        continue;
                    }

                    var pathSearch = new AStar(top, distances);
                    var current = pathSearch.Cross(fromIndex, toIndex);

                    if (current == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            string.Format("No walk found for line at position {0}. Are end points isolated?", i.ToString()));
                    }
                    result.Add(current);
                }

                DA.SetDataList(0, result);
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Resources.shortest_walk24;
            }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.hidden;
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{A51E8547-743C-4cb5-BD58-70665A8A065D}");
            }
        }
    }
}
