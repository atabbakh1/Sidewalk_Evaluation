using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

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


        public static List<Curve> ReduceCurves(List<Curve> targetCurves)
        {
            List<Curve> reducedCurves = new List<Curve>();
            if (targetCurves != null && targetCurves.Count > 0)
            {
                for(int i=0; i<targetCurves.Count; i++)
                {
                    double t0 = targetCurves[i].Domain.Min; 
                    double t1 = targetCurves[i].Domain.Max; 
                    double t;
                    List<Point3d> discPoints = new List<Point3d>();

                    int count = (int)targetCurves[i].GetLength()/5;
                    if (count > 1000)
                        count = 1000;

                    targetCurves[i].Rebuild(count, 1, true);
                    bool disc  = targetCurves[i].GetNextDiscontinuity(Continuity.C0_continuous, t0, t1, out t);

                    while (disc)
                    {
                        discPoints.Add(targetCurves[i].PointAt(t));
                    }

                    if(discPoints.Count > 0)
                    {
                        reducedCurves.Add(new PolylineCurve(discPoints));
                    }
                }
            }

            return reducedCurves;
        }


    }
}
