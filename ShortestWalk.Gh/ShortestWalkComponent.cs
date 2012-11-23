using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ShortestWalk.Geometry;
using ShortestWalk.Gh.Properties;

namespace ShortestWalk.Gh
{
    [CLSCompliant(false)]
    public class ShortestWalkComponent : GH_Component
    {
        public ShortestWalkComponent()
            : base("Shortest Walk", "ShortWalk", "Calculates the shortest walk in a network of curves",
            "Curve", "Util")
        { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_CurveParam("Curves", "C", "The curves group", GH_ParamAccess.list);
            pManager.Register_DoubleParam("Lengths", "L", "One length for each curve. If the number of lengths is less than the one of curves, length values are repeated in pattern.\nIf there are no lengths, then the physical length of the curves is computed.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.Register_LineParam("Wanted path", "P", "The lines from the start to the end of the path", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_CurveParam("Shortest walk", "W", "The shortest connection curve");
            pManager.Register_IntegerParam("Succession", "S", "The indices of the curves that form the result");
            pManager.Register_BooleanParam("Direction", "D", "True if the curve in succession is walked from start to end, false otherwise");
            pManager.Register_DoubleParam("Length", "L", "The total length, as an aggregation of the input lengths measured along the walk");
        }

        static Predicate<Curve> _removeNullAndInvalidDelegate = RemoveNullAndInvalid;
        static Predicate<Line> _removeInvalidDelegate = RemoveInvalid;
        static Predicate<double> _isNegative = IsNegative;

        private static bool RemoveNullAndInvalid(Curve obj)
        {
            return obj == null || !obj.IsValid;
        }

        private static bool RemoveInvalid(Line obj)
        {
            return !obj.IsValid;
        }

        private static bool IsNegative(double number)
        {
            return number < 0;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curves = new List<Curve>();
            var lengths = new List<double>();
            var lines = new List<Line>();

            if (DA.GetDataList(0, curves) && DA.GetDataList(2, lines))
            {
                DA.GetDataList(1, lengths);

                int negativeIndex = lengths.FindIndex(_isNegative);
                if (negativeIndex != -1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        string.Format("Distances cannot be negative. At least one negative value encounter at index {0}.",
                        negativeIndex));
                    return;
                }

                curves.RemoveAll(_removeNullAndInvalidDelegate);
                lines.RemoveAll(_removeInvalidDelegate);

                if (curves.Count < 1)
                    return;
                
                CurvesTopology top = new CurvesTopology(curves, GH_Component.DocumentTolerance());
                //CurvesTopologyPreview.Mark(top, Color.BurlyWood, Color.Bisque);

                PathMethod pathSearch;
                if (lengths.Count == 0)
                {
                    IList<double> distances = top.MeasureAllEdgeLengths();
                    pathSearch = new AStar(top, distances);
                }
                else if (lengths.Count == 1)
                {
                    pathSearch = new Dijkstra(top, lengths[0]);
                }
                else
                {
                    IList<double> interfLengths = lengths;

                    if (interfLengths.Count < top.EdgeLength)
                        interfLengths = new ListByPattern<double>(interfLengths, top.EdgeLength);

                    bool isAlwaysShorterOrEqual = true;
                    for(int i=0; i<top.EdgeLength; i++)
                    {
                        if (top.LinearDistanceAt(i) > interfLengths[i])
                        {
                            isAlwaysShorterOrEqual = false;
                            break;
                        }
                    }

                    if (isAlwaysShorterOrEqual)
                        pathSearch = new AStar(top, interfLengths);
                    else
                        pathSearch = new Dijkstra(top, interfLengths);
                }

                var resultCurves = new List<Curve>();
                var resultLinks = new GH_Structure<GH_Integer>();
                var resultDirs = new GH_Structure<GH_Boolean>();
                var resultLengths = new List<double>();

                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];

                    int fromIndex = top.GetClosestNode(line.From);
                    int toIndex = top.GetClosestNode(line.To);

                    if (fromIndex == toIndex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The start and end positions are equal");
                        resultCurves.Add(null);
                        continue;
                    }

                    int[] nodes, edges; bool[] dir; double tot;
                    var current = pathSearch.Cross(fromIndex, toIndex, out nodes, out edges, out dir, out tot);

                    if (current == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            string.Format("No walk found for line at position {0}. Are end points isolated?", i.ToString()));
                    }
                    else
                    {
                        var pathLinks = DA.ParameterTargetPath(1).AppendElement(i);
                        //GH 0.8 code
                        //var pathLinks = DA.get_ParameterTargetPath(1).AppendElement(i);

                        resultLinks.AppendRange(GhWrapTypeArray<int, GH_Integer>(edges), pathLinks);
                        resultDirs.AppendRange(GhWrapTypeArray<bool, GH_Boolean>(dir), pathLinks);
                        resultLengths.Add(tot);
                    }

                    resultCurves.Add(current);
                }

                DA.SetDataList(0, resultCurves);
                DA.SetDataTree(1, resultLinks);
                DA.SetDataTree(2, resultDirs);
                DA.SetDataList(3, resultLengths);
            }
        }

        private TGh[] GhWrapTypeArray<T, TGh>(T[] input)
            where TGh : GH_Goo<T>, new()
        {
            if (input == null)
                return null;

            var newArray = new TGh[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                var gh = new TGh();
                gh.Value = input[i];
                newArray[i] = gh;
            }
            return newArray;
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                return Resources.shortest_walk24;
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{2F1AC11C-5E8B-4692-83BE-CAF24EACE13B}");
            }
        }
    }
}
