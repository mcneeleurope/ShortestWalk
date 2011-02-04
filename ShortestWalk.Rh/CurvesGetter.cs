using System;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace ShortestWalk.Rh
{
    class CurvesGetter : GetObject
    {
        public CurvesGetter(string prompt)
            : base()
        {
            this.SetCommandPrompt(prompt);
            this.GeometryFilter = ObjectType.Curve;
            this.GeometryAttributeFilter = GeometryAttributeFilter.OpenCurve;
            this.EnablePreSelect(true, true);
            this.EnablePressEnterWhenDonePrompt(false);
        }

        /// <summary>
        /// Use 0 as max if you do not want to set it.
        /// </summary>
        /// <param name="min">The minimun number of lines</param>
        /// <param name="max">The maximum, as in GetObject.GetMultiple()</param>
        /// <param name="lines"></param>
        /// <returns>True if the getting supplied the requested lines, false otherwise.</returns>
        public bool Curves(int min, int max, out Curve[] lines)
        {
            GetResult a;
            if (min == 1 && max == 1)
                a = this.Get();
            else
                a = this.GetMultiple(min, max);

            if (a == GetResult.Object)
            {
                if (this.ObjectCount > 0)
                {
                    int realCount = 0;

                    lines = new Curve[this.ObjectCount];
                    for (int i = 0; i < this.ObjectCount; i++)
                    {
                        Curve c = this.Object(i).Curve();
                        if (c != null && c.IsValid)
                        {
                            lines[realCount++] = c;
                        }
                    }
                    Array.Resize(ref lines, realCount);
                    return true;
                }
            }
            lines = null;

            return false;
        }
    }
}
