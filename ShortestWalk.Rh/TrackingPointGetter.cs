using System;
using System.Drawing;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using ShortestWalk.Geometry;

namespace ShortestWalk.Rh
{
    class TrackingPointGetter : IDisposable
    {
        readonly GetPoint _getPoint = new GetPoint();
        readonly int _fromIndex;
        readonly double[] _distances;
        readonly CurvesTopology _crvTopology;

        public TrackingPointGetter(string prompt, CurvesTopology crvTopology)
            : this(prompt, crvTopology, -1)
        {
        }

        public TrackingPointGetter(string prompt, CurvesTopology crvTopology, int fromIndex)
        {
            _crvTopology = crvTopology;
            _fromIndex = fromIndex;

            if (!string.IsNullOrEmpty(prompt))
                _getPoint.SetCommandPrompt(prompt);

            if (fromIndex != -1)
            {
                _distances = crvTopology.MeasureAllEdgeLengths();
            }
            _getPoint.DynamicDraw += DynamicDraw;
        }

        public double[] DistanceCache
        {
            get
            {
                return _distances;
            }
        }

        public Result GetPointOnTopology(out int index)
        {
            index = -1;

            var oOn = ModelAidSettings.Osnap;
            var oMode = ModelAidSettings.OSnapModes;
            ModelAidSettings.Osnap = true;
            ModelAidSettings.OSnapModes |= (int)OSnapModes.End;

            if (_getPoint.Get() != GetResult.Point)
            {
                return Result.Cancel;
            }

            index = _crvTopology.GetClosestNode(_getPoint.Point());

            ModelAidSettings.Osnap = oOn;
            ModelAidSettings.OSnapModes = oMode;

            if (index == -1)
                return Result.Failure;

            return Result.Success;
        }

        void DynamicDraw(object sender, GetPointDrawEventArgs e)
        {
            try
            {
                Point3d to = e.CurrentPoint;

                if (to.IsValid)
                {
                    int toIndex = _crvTopology.GetClosestNode(to);

                    if (toIndex != -1)
                    {
                        e.Display.DrawPoint(_crvTopology.VertexAt(toIndex), PointStyle.X, 7, Color.Azure);

                        if (_fromIndex != -1 && toIndex != _fromIndex)
                        {
                            var c = PathMethods.AStar(_crvTopology, _fromIndex, toIndex, _distances);

                            if (c != null)
                                e.Display.DrawCurve(c, Color.Black, 3);
                        }
                    }
                }

                if(_fromIndex != -1)
                    e.Display.DrawPoint(_crvTopology.VertexAt(_fromIndex), PointStyle.X, 7, Color.Azure);
            }
            catch (Exception ex)
            {
                _getPoint.DynamicDraw -= DynamicDraw;
                RhinoApp.WriteLine("An error happened in the display pipeline: {0}, {1}", ex.GetType().Name, ex.Message);
            }
        }

        public void Dispose()
        {
            _getPoint.Dispose();
        }
    }
}