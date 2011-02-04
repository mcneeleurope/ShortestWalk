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
        readonly SearchMode _sm;
        readonly PathMethod _pathSearchMethod;

        public TrackingPointGetter(string prompt, CurvesTopology crvTopology)
            : this(prompt, crvTopology, -1, SearchMode.Links)
        {
        }

        public TrackingPointGetter(string prompt, CurvesTopology crvTopology, int fromIndex, SearchMode mode)
        {
            _crvTopology = crvTopology;
            _fromIndex = fromIndex;

            if(!Enum.IsDefined(typeof(SearchMode), mode))
                throw new ArgumentException("Enum is undefined.", "mode");
            _sm = mode;
            

            if (!string.IsNullOrEmpty(prompt))
                _getPoint.SetCommandPrompt(prompt);

            if (fromIndex != -1)
            {
                switch(mode)
                {
                    case SearchMode.CurveLength:
                        _distances = crvTopology.MeasureAllEdgeLengths();
                        break;
                    case SearchMode.LinearDistance:
                        _distances = crvTopology.MeasureAllEdgeLinearDistances();
                        break;
                    case SearchMode.Links:
                        _distances = null;
                        break;
                    default:
                        throw new ApplicationException("Behaviour for this enum value is undefined.");
                }
            }
            _getPoint.DynamicDraw += DynamicDraw;

            _pathSearchMethod = PathMethod.FromMode(_sm, _crvTopology, _distances);
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
            var oMode = ModelAidSettings.OsnapModes;
            ModelAidSettings.Osnap = true;
            ModelAidSettings.OsnapModes = (OsnapModes)~0;

            if (_getPoint.Get() != GetResult.Point)
            {
                return Result.Cancel;
            }

            index = _crvTopology.GetClosestNode(_getPoint.Point());

            ModelAidSettings.Osnap = oOn;
            ModelAidSettings.OsnapModes = oMode;

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
                            var c = _pathSearchMethod.Cross(_fromIndex, toIndex);

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

        public virtual void Dispose()
        {
            _getPoint.Dispose();
        }
    }
}