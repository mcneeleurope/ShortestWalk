using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace ShortestWalk.Geometry
{
    /// <summary>
    /// Contains static methods to search paths in networks
    /// </summary>
    public static class PathMethods
    {
        /// <summary>
        /// The A* search algorithm, simple form
        /// See http://en.wikipedia.org/wiki/A*_search_algorithm for description
        /// </summary>
        /// <param name="top">An input topology</param>
        /// <param name="from">The vertex index of departure</param>
        /// <param name="to">The vertex index of arrival</param>
        /// <param name="dist">A series of distances. These cannot be less than the physical distance between starts and ends, but might be suitably longer</param>
        /// <returns>A Curve that shows the entire walk, or null on error</returns>
        public static Curve AStar(CurvesTopology top, int from, int to, double[] dist)
        {
            int[] nodes;
            int[] edges;
            bool[] eDirs;
            return AStar(top, from, to, dist, out nodes, out edges, out eDirs);
        }

        /// <summary>
        /// The A* search algorithm.
        /// See http://en.wikipedia.org/wiki/A*_search_algorithm for description
        /// </summary>
        /// <param name="top">An input topology</param>
        /// <param name="from">The vertex index of departure</param>
        /// <param name="to">The vertex index of arrival</param>
        /// <param name="dist">A series of distances. These cannot be less than the physical distance between starts and ends, but might be suitably longer</param>
        /// <param name="nodes">Output parameter. The specific walked nodes, or null on error</param>
        /// <param name="edges">Output parameter. The walked edges indices, or null on error</param>
        /// <param name="eDirs">Output parameter. Whether the edges were walked from front to end or in the opposite direction, or null on error</param>
        /// <returns>A Curve that shows the entire walk, or null on error</returns>
        public static Curve AStar(CurvesTopology top, int from, int to, double[] dist, out int[] nodes, out int[] edges, out bool[] eDirs)
        {
            if (top == null)
                throw new ArgumentNullException("top");
            if (from < 0)
                throw new ArgumentOutOfRangeException("from", "from is less than 0");
            if (to < 0)
                throw new ArgumentOutOfRangeException("to", "to is less than 0");
            if (from >= top.VertexLength)
                throw new ArgumentOutOfRangeException("from", "from is more than vertex length");
            if (to >= top.VertexLength)
                throw new ArgumentOutOfRangeException("to", "to is more than vertex length");
            if (from == to)
                throw new ArgumentException("walkFromIndex and walkToIndex are the same");

            Dictionary<int, byte> closed = new Dictionary<int, byte>(top.EdgeLength / 4);
            SortedList<int, byte> open = new SortedList<int, byte>(top.EdgeLength / 5);
            // We do not need the byte generic type in either collections. List<> works and
            // is smaller in memory but is much slower for large datasets

            open.Add(from, default(byte));

            double[] gScore = new double[top.VertexLength];
            double[] hScore = new double[top.VertexLength];
            double[] fScore = new double[top.VertexLength];
            int[] came_from = new int[top.VertexLength];
            for (int i = 0; i < came_from.Length; i++)
                came_from[i] = -1;

            hScore[from] = HeuristicEstimateDistance(top, from, to);
            fScore[from] = hScore[from];

            while (open.Count > 0)
            {
                int n = FindMinimumFScoreAmongOpen(open.Keys, fScore);
                if (n == to)
                    return ReconstructPath(top, dist, came_from, to, out nodes, out edges, out eDirs); //Found the path

                open.Remove(n);
                closed.Add(n, default(byte));

                var node = top.NodeAt(n);
                for (int i = 0; i < node.EdgeCount; i++)
                {
                    int ei = node.EdgeIndexAt(i, top);
                    int y = top.EdgeAt(ei).OtherVertex(n);

                    if (closed.ContainsKey(y))
                        continue;
                    double tentative_g_score = gScore[n] + dist[ei];

                    bool tentativeIsBetter;
                    if (!open.ContainsKey(y))
                    {
                        open.Add(y, default(byte));
                        tentativeIsBetter = true;
                    }
                    else if (tentative_g_score < gScore[y])
                        tentativeIsBetter = true;
                    else
                        tentativeIsBetter = false;

                    if (tentativeIsBetter)
                    {
                        came_from[y] = n;

                        gScore[y] = tentative_g_score;
                        hScore[y] = HeuristicEstimateDistance(top, y, to);
                        fScore[y] = gScore[y] + hScore[y];
                    }
                }
            }
            nodes = edges = null;
            eDirs = null;
            return null;    //no path found. Error
        }

        private static double HeuristicEstimateDistance(CurvesTopology top, int to, int y)
        {
            return top.VertexAt(y).DistanceTo(top.VertexAt(to)) - 0.01;
        }

        private static int FindMinimumFScoreAmongOpen(IList<int> open, double[] f_score)
        {
            int n = open[0];
            double cc = f_score[n];
            for (int i = 1; i < open.Count; i++)
            {
                int possibleBetterIndex = open[i];
                double current = f_score[possibleBetterIndex];
                if (current < cc)
                {
                    n = possibleBetterIndex;
                    cc = current;
                }
            }
            return n;
        }

        private static Curve ReconstructPath(CurvesTopology top, double[] dist, int[] cameFrom, int currentNode, out int[] nodes, out int[] edges, out bool[] edgeDir)
        {
            List<int> resultNodes = new List<int>();
            for (; ; )
            {
                if (currentNode == -1)
                    break;
                resultNodes.Add(currentNode);
                currentNode = cameFrom[currentNode];
            }
            resultNodes.Reverse();
            nodes = resultNodes.ToArray();

            List<int> resultEdges = new List<int>();
            List<bool> resultEdgesRev = new List<bool>();
            currentNode = nodes[0];
            for (int i = 1; i < nodes.Length; i++)
            {
                int nxt = nodes[i];
                bool rev;
                int edgeIndex = FindEdge(top, dist, currentNode, nxt, out rev);
                resultEdges.Add(edgeIndex);
                resultEdgesRev.Add(rev);
                currentNode = nxt;
            }
            edges = resultEdges.ToArray();
            edgeDir = resultEdgesRev.ToArray();

            PolyCurve pc = new PolyCurve();

            for (int i = 0; i < resultEdges.Count; i++)
            {
                int ei = resultEdges[i];
                var cv = top.CurveAt(resultEdges[i]).DuplicateCurve();
                if (!resultEdgesRev[i])
                    cv.Reverse();
                pc.Append(cv);
            }
            pc.RemoveNesting();

            return pc;
        }

        private static int FindEdge(CurvesTopology top, double[] dist, int currentNode, int nxt, out bool rev)
        {
            NodeAddress node = top.NodeAt(currentNode);
            double minDist = double.MaxValue;
            int bestEi = -1;
            rev = false;

            for (int j = 0; j < node.EdgeCount; j++)
            {
                var ei = node.EdgeIndexAt(j, top);
                var edge = top.EdgeAt(ei);
                if (edge.OtherVertex(currentNode) == nxt)
                {
                    if (dist[ei] < minDist)
                    {
                        rev = node.RevAt(j, top);
                        bestEi = ei;
                        minDist = dist[ei];
                    }
                }
            }

            if(bestEi == -1)
                throw new KeyNotFoundException("Vertex currentNode is not linked to nxt");

            return bestEi;
        }
    }
}
