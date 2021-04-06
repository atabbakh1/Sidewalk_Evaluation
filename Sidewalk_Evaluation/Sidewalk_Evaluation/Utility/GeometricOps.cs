using Rhino.Geometry;
using Rhino;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;

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

            if(targetCurve != null)
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

            return false;

        }

        /// <summary>
        /// validate and rebuild a list of curves 
        /// </summary>
        /// <param name="targetCurves">curves to clean</param>
        /// <returns></returns>
        public static List<Curve> CleanCurves(List<Curve> targetCurves)
        {
            List<Curve> cleanedCurves = new List<Curve>();

            if (targetCurves != null && targetCurves.Count > 0)
            {
                for (int i = 0; i < targetCurves.Count; i++)
                {
                    if (ValidateCurve(targetCurves[i]) == true)
                        cleanedCurves.Add(RebuildCurve(targetCurves[i]));
                }

            }

            return cleanedCurves;
        }

        /// <summary>
        /// Creates a planar mesh using a set of closed curves
        /// </summary>
        /// <param name="inputClosedCurves">closed curves for mesh creation</param>
        /// <returns></returns>
        public static Brep CreatePlanarMesh(List<Curve> inputClosedCurves)
        {
            Mesh finalPlanarMesh = new Mesh();

            Brep surf = new Brep();
            if(inputClosedCurves != null && inputClosedCurves.Count > 0)
            {
                //create a brep planar surface using the input curves
                Brep[] tempSurf = Brep.CreatePlanarBreps(inputClosedCurves, 0.1);

                if(tempSurf.Length > 0)
                {
                    surf = tempSurf[0];
                }

                /*
                Mesh[] meshArray = null;

                MeshingParameters minimal = MeshingParameters.Minimal;

                //build a mesh using the brep
                for (int i = 0; i < tempSurf.Length; i++)
                {
                    meshArray = Mesh.CreateFromBrep(tempSurf[i], minimal);
                }

                //join meshes
                if (meshArray.Length > 1)
                {
                    for (int i = 0; i < meshArray.Length; i++)
                    {
                        finalPlanarMesh.Append(meshArray[i]);
                    }
                }
                else
                {
                    finalPlanarMesh = meshArray[0];
                }
                */
            }

           

            return surf;
        }

        /// <summary>
        /// calcuate the area of a curve excluding the area of any interior curves
        /// </summary>
        /// <param name="outsideCurve"></param>
        /// <param name="insideCurves"></param>
        /// <returns></returns>
        public static double CalculateStencilArea(Curve outsideCurve, List<Curve> insideCurves)
        {
            double insideAreas = 0;
            double outsideArea;

            outsideArea= AreaMassProperties.Compute(outsideCurve).Area;

            for (int i = 0; i < insideCurves.Count; i++)
            {
                insideAreas += AreaMassProperties.Compute(insideCurves[i]).Area;
            }

            return outsideArea - insideAreas;
        }

        /// <summary>
        /// Calculate the area of a single curve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static double CalculateArea(Curve curve)
        {
            return AreaMassProperties.Compute(curve).Area;
        } 

        public static Point3d ReturnCurveCentroid(Curve curve)
        {
            return AreaMassProperties.Compute(curve).Centroid;
        }
        /// <summary>
        /// check if a curve is contained inside another
        /// </summary>
        /// <param name="outerCurve"></param>
        /// <param name="curveToCheck"></param>
        /// <returns></returns>
        public static bool IsInsideCurve(Curve outerCurve, Curve curveToCheck)
        {
            if (Curve.PlanarClosedCurveRelationship(outerCurve, curveToCheck, Plane.WorldXY, 0.1) == RegionContainment.BInsideA)
                return true;

            return false;
            
        }

        /// <summary>
        /// check if a curves are intersecting
        /// </summary>
        /// <param name="outerCurve"></param>
        /// <param name="curveToCheck"></param>
        /// <returns></returns>
        public static bool AreIntersecting(Curve curveA, Curve curveB)
        {
            if (Curve.PlanarClosedCurveRelationship(curveA, curveB, Plane.WorldXY, 0.1) == RegionContainment.MutualIntersection)
                return true;

            return false;
        }


        public static bool InsideOrIntersecting(Curve outerCurve, Curve curveToCheck)
        {

            RegionContainment relation = Curve.PlanarClosedCurveRelationship(outerCurve, curveToCheck, Plane.WorldXY, 0.1);
            if (relation == RegionContainment.BInsideA || relation == RegionContainment.MutualIntersection)
                return true;

            return false;


        }

        public static void Repel(Point3d pointToRepel, Point3d destination, List<Curve> bouncers)
        {
            Vector3d vector = destination - pointToRepel;

            for(int i =0; i<bouncers.Count; i++)
            {
                bouncers[i].ClosestPoint(pointToRepel, out double t);
                double distance = pointToRepel.DistanceTo(bouncers[i].PointAt(t));
                if (distance > 3) continue;

            }
        }



    }
}
