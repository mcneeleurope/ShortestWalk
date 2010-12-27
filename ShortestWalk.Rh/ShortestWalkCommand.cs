using System;
using System.Drawing;
using System.Text;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using ShortestWalk.Geometry;

namespace ShortestWalk.Rh
{
    public class ShortestPathInCurvesCommand : Command
    {
        public override string EnglishName
        {
            get
            {
                return "ShortestWalk";
            }
        }

        public override Guid Id
        {
            get
            {
                return new Guid("{68B3FDAA-2B59-4080-B988-6833F38167DE}");
            }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Curve[] curves;
            OptionToggle tog = new OptionToggle(false, "Hide", "Show");
            OptionDouble tol = new OptionDouble(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true, 0.0);
            using (CurvesGetter getLines = new CurvesGetter("Select curves meeting at endpoints. Press Enter when done"))
            {
                for( ; ;)
                {
                    getLines.ClearCommandOptions();
                    getLines.EnableClearObjectsOnEntry(false);
                    int showInt = getLines.AddOptionToggle("Topology", ref tog);
                    int tolInt = getLines.AddOptionDouble("Tolerance", ref tol);

                    if (getLines.Lines(1, 0, out curves))
                        break;
                    else
                    {
                        RhinoApp.WriteLine("Less than three lines were selected");

                        if (getLines.Result() == GetResult.Option)
                            continue;
                        else
                            return Result.Cancel;
                    }
                }
            }
            CurvesTopology crvTopology = new CurvesTopology(curves, tol.CurrentValue);

            Guid[] ids = null;
            if (tog.CurrentValue)
                ids = CurvesTopologyPreview.Mark(crvTopology, Color.LightBlue, Color.LightCoral);

            int walkFromIndex;
            using (var getStart = new TrackingPointGetter("Select the start point of the walk on the curves", crvTopology))
            {
                if (getStart.GetPointOnTopology(out walkFromIndex) != Result.Success)
                {
                    EndOperations(ids);
                    return Result.Cancel;
                }
            }

            int walkToIndex;
            double[] distances;
            using (var getEnd = new TrackingPointGetter("Select the end point", crvTopology, walkFromIndex))
            {
                if (getEnd.GetPointOnTopology(out walkToIndex) != Result.Success)
                {
                    EndOperations(ids);
                    return Result.Cancel;
                }
                distances = getEnd.DistanceCache;
            }

            if (walkFromIndex == walkToIndex)
            {
                RhinoApp.WriteLine("Start and end points are equal");
                EndOperations(ids);
                return Result.Nothing;
            }

            int[] nIndices, eIndices;
            bool[] eDirs;
            Curve c = PathMethods.AStar(crvTopology, walkFromIndex, walkToIndex, distances, out nIndices, out eIndices, out eDirs);

            Result wasSuccessful;
            if (c != null && c.IsValid)
            {
                if (tog.CurrentValue)
                {
                    RhinoApp.WriteLine("Vertices: {0}", FormatNumbers(nIndices));
                    RhinoApp.WriteLine("Edges: {0}", FormatNumbers(eIndices));
                }

                var a = RhinoDoc.ActiveDoc.CreateDefaultAttributes();
                Guid g = RhinoDoc.ActiveDoc.Objects.AddCurve(c, a);

                var obj = RhinoDoc.ActiveDoc.Objects.Find(g);
                if (obj != null)
                {
                    obj.Select(true);
                    wasSuccessful = Result.Success;
                }
                else
                    wasSuccessful = Result.Failure;
            }
            else
            {
                RhinoApp.WriteLine("No path was found. Either points are isolated, or an error occurred.");
                wasSuccessful = Result.Nothing;
            }

            EndOperations(ids);
            return wasSuccessful;
        }

        private static void EndOperations(Guid[] ids)
        {
            CurvesTopologyPreview.Unmark(ids);
            RhinoDoc.ActiveDoc.Views.Redraw();
        }

        private static string FormatNumbers(int[] arr)
        {
            if (arr == null || arr.Length == 0)
                return "(empty)";

            StringBuilder s = new StringBuilder(arr[0].ToString());
            for (int i = 1; i < arr.Length; i++)
            {
                s.Append(", ");
                s.Append(arr[i].ToString());
            }
            return s.ToString();
        }
    }
}
