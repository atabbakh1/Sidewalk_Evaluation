using Rhino.Geometry;
using Rhino;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
namespace Sidewalk_Evaluation.Utility
{
    class GeometricOps
    {
        /// <summary>
        /// build a Rhino circle based on input provided by a CSV data record
        /// </summary>
        /// <param name="csVDataRecord">the csv data line that holds the information for the target circle</param>
        /// <param name="x_index">the index of the column holding the X coordinates of the circle center</param>
        /// <param name="y_index">the index of the column holding the Y coordinates of the circle center</param>
        /// <param name="dia_index">the index of the column holding the diameter value of the circle</param>
        /// <returns></returns>
        public static Circle CreateCircleFromCSV(string csvDataRecord, int x_index, int y_index, int dia_index, out Point3d center)
        {
            string[] circleRecord = csvDataRecord.Split(',');

            double d;
            double x;
            double y;

            //if all values are valid doubles
            if (double.TryParse(circleRecord[x_index], out x) && double.TryParse(circleRecord[y_index], out y) && double.TryParse(circleRecord[dia_index], out d))
            {
                center = new Point3d(x, y, 0);
                return new Circle(center, d / 2);
            }

            center = Point3d.Unset;
            return Circle.Unset;
        }

        /// <summary>
        /// Rebuilds a curve by converting it into a reduced polyline and then returning the polyline curve
        /// </summary>
        /// <param name="targetCrv">curve to reduce/rebuild</param>
        /// <returns></returns>
        public static Curve RebuildCurve(Curve targetCrv)
        {
            Curve rebuiltCurve;

            if (targetCrv != null)
            {
                PolylineCurve plc = targetCrv.ToPolyline(0.1, 1.0, 6, 100);

                int point_count = plc.PointCount;
                Polyline pl = new Polyline(point_count);
                for (int i = 0; i < plc.PointCount; ++i)
                {
                    pl.Add(plc.Point(i));
                }

                if (pl != null)
                {
                    pl.MergeColinearSegments(0.1, true);
                    rebuiltCurve = pl.ToPolylineCurve();
                    //return Curve.ProjectToPlane(rebuiltCurve, Plane.WorldXY); 
                    return rebuiltCurve;
                }
                else
                {
                    return targetCrv;
                }
            }

            else
            {
                return targetCrv;
            }
        }

        /// <summary>
        /// Validate a curve to check if it is: 1. closed, 2. valid
        /// </summary>
        /// <param name="targetCurves"></param>
        /// <returns></returns>
        public static bool ValidateCurve(Curve targetCurve)
        {
            if (targetCurve.IsClosed == true && targetCurve.IsValid == true)
            {
                Debug.WriteLine("CURVE IS VALID");
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// validate and rebuild a list of curves 
        /// </summary>
        /// <param name="targetCurves">curves to clean</param>
        /// <returns></returns>
        public static List<Curve> CleanCurves(List<Curve> targetCurves)
        {
            List<Curve> cleanedCurves = new List<Curve>();

            for (int i = 0; i < targetCurves.Count; i++)
            {
                if (ValidateCurve(targetCurves[i]) == true)
                    cleanedCurves.Add(RebuildCurve(targetCurves[i]));
            }


            return cleanedCurves;
        }

    }
}
