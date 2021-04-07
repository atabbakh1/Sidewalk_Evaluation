using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Sidewalk_Evaluation.Utility;


namespace Sidewalk_Evaluation
{
    class Sidewalk
    {
        private Curve sw_Curve = null;
        List<Curve> sw_Bldgs = new List<Curve>();
        bool sw_IsROW = false;
        double sw_Area = 0;
        bool sw_HasTrees = false;
        List<Curve> sw_Trees = new List<Curve>();
        bool sw_HasSubway = false;
        Curve sw_Subway = null;
        int sw_Capacity = 0;
        int sw_Population = 0;




        /// <summary>
        /// Sidewalk Rhino curve
        /// </summary>
        public Curve Sidewalk_Curve
        {
            get
            {
                return sw_Curve;
            }
            set
            {
                sw_Curve = value;
            }
        }

        /// <summary>
        /// The center point of the sidewalk curve
        /// </summary>
        public Point3d Sidewalk_Centroid
        {
            get
            {
                return GeometricOps.CalculateCentroid(sw_Curve);
            }
        }

        /// <summary>
        /// Sidewalk building curves as a list
        /// </summary>
        public List<Curve> Sidewalk_Buildings
        {
            get
            {
                return sw_Bldgs;
            }
            set
            {
                sw_Bldgs = value;
            }
        }

        /// <summary>
        /// Is this a Right Of Way type sidewalk
        /// </summary>
        public bool IsROW
        {
            get
            {
                return sw_IsROW;
            }
            set
            {
                sw_IsROW = value;
            }
        }

        /// <summary>
        /// The sidewalk total area excluding building and trees (if any)
        /// </summary>
        public double Sidewalk_Area
        {
            get
            {
                return sw_Area;
            }
            set
            {
                sw_Area = value;
            }
        }

        /// <summary>
        /// Indicates if any trees belong to this sidewalk
        /// </summary>
        public bool HasTrees 
        {
            get
            {
                return sw_HasTrees;
            }
            set
            {
                sw_HasTrees = value;
            }
        }

        /// <summary>
        /// the sidewalk tree curves as a list (if any)
        /// </summary>
        public List<Curve> Sidewalk_Trees
        {
            get
            {
                return sw_Trees;
            }
            set
            {
                sw_Trees = value;
            }
        }

        /// <summary>
        /// Indicates if this sidewalk has any subway entrances
        /// </summary>
        public bool HasSubway
        {
            get
            {
                return sw_HasSubway;
            }
            set
            {
                sw_HasSubway = value;
            }
        }

        /// <summary>
        /// the sidewalk subway point (if any)
        /// </summary>
        public Curve Sidewalk_Subway
        {
            get
            {
                return sw_Subway;
            }
            set
            {
                sw_Subway = value;
            }
        }

        /// <summary>
        /// The sidewalk estimated pedastrian capacity
        /// </summary>
        public int Capacity
        {
            get
            {
                return sw_Capacity;
            }
            set
            {
                sw_Capacity = value;
            }
        }

        /// <summary>
        /// The current pedastrian population of the sidewalk
        /// </summary>
        public int Population
        {
            get
            {
                return sw_Population;
            }
            set
            {
                sw_Population = value;
            }
        }


        public Sidewalk(Curve sidewalkCurve)
        {
            if(sidewalkCurve !=null)
                sw_Curve = sidewalkCurve;

        }

        /// <summary>
        /// Check if any of the buildings closed curves are fully contained within this sidewalk
        /// </summary>
        /// <param name="buildingCurves">building curves to check against</param>
        /// <param name="foundBldgs"></param>
        /// <returns></returns>
        public bool CheckForBuildings (Curve[] buildingCurves)
        {
            bool foundBuildings = false;
            List<Curve> insideThisSidewalk = new List<Curve>();
            
            if(buildingCurves != null && buildingCurves.Length > 0)
            {
                for (int i=0; i< buildingCurves.Length; i++)
                {
                    if(buildingCurves[i].IsClosed)
                    {
                        if (GeometricOps.IsInsideCurve(sw_Curve, buildingCurves[i]))
                            insideThisSidewalk.Add(buildingCurves[i]);
                    }

                }
            }

            if (insideThisSidewalk.Count > 0)
            {
                foundBuildings = true;
                sw_IsROW = true;
                sw_Bldgs = insideThisSidewalk;
                sw_Area = GeometricOps.CalculateStencilArea(sw_Curve, insideThisSidewalk);
            }
            else
            {
                Sidewalk_Area = GeometricOps.CalculateArea(sw_Curve);
            }

            return foundBuildings;
        }

        /// <summary>
        /// Check if any of the trees circles are either contained or intersecting with the sidewalk curve
        /// </summary>
        /// <param name="treesCircles">circles representing trees</param>
        /// <returns></returns>
        public bool CheckForTrees(List<Circle> treesCircles)
        {
            bool foundTrees = false;

            if (treesCircles != null && treesCircles.Count > 0)
            {
                for (int i = 0; i < treesCircles.Count; i++)
                {
                    if(treesCircles[i].IsValid)
                    {
                        List<Curve> trees = new List<Curve>();
                        ArcCurve treeCurve = new ArcCurve(treesCircles[i]);                    //convert to ArcCurve to access Curve tools
                        if (GeometricOps.InsideOrIntersecting(sw_Curve, treeCurve))       //check for either intersection or containment with current sidewalk curve
                        {
                            trees.Add(treeCurve);                                         //add to this sidewalk trees property list
                            Sidewalk_Area -= GeometricOps.CalculateArea(treeCurve);       //subtract the tree area from the current sidewalk area
                        }

                        if (trees.Count > 0)
                        {
                            foundTrees = true;
                            HasTrees = true;
                            Sidewalk_Trees = trees;
                        }
                    }
                }
            }
            

            return foundTrees;
        }

        /// <summary>
        /// Check if any of the subway entrances closed curves either contained or intersecting with the sidewalk curve
        /// </summary>
        /// <param name="subwayEntrances">closed curves representing subway entrances</param>
        /// <returns></returns>
        public bool CheckForSubway (List<Curve> subwayEntrances)
        {
            bool foundSubway = false;

            if(subwayEntrances != null && subwayEntrances.Count > 0)
            {
                for (int i = 0; i < subwayEntrances.Count; i++)
                {
                    if (subwayEntrances[i] != null && subwayEntrances[i].IsClosed)
                    {
                        if (GeometricOps.InsideOrIntersecting(sw_Curve, subwayEntrances[i]))        //check for either intersection or containment with current sidewalk curve
                        {
                            HasSubway = true;
                            Sidewalk_Subway = subwayEntrances[i];                                   //add to this sidewalk subway propery
                        }
                    }
                }
            }
            return foundSubway;
        }

        /// <summary>
        /// calculate the estimated number of pedastrians this sidewalk can handle at 100% capacity
        /// </summary>
        /// <param name="socialRadius">the radius of the circle each pedastrian will represent in the evaluation</param>
        /// <returns></returns>
        public int CalculateCapacity(double socialRadius)
        {
            int capacity = 0;

            if(sw_Area > 0)
            {
                if (socialRadius <= 0) socialRadius = 1;
                double pedastrianZoneArea = (3.14) * socialRadius * socialRadius;
                capacity = Convert.ToInt32(sw_Area / pedastrianZoneArea);
            }

            sw_Capacity = capacity;

            return capacity;
        }


        /// <summary>
        /// calculate the number of pedastrians to estimate based on user parameters
        /// </summary>
        /// <param name="capacityUtilization">the percentage of the full sidewalk capacity to utilize in the evaluation</param>
        /// <param name="subwayInfluence">a population multiplyer to apply to sidewalks that have subway entrances</param>
        /// <returns></returns>
        public int CalculatePopulation (double capacityUtilization, double subwayInfluence)
        {
            int population = 0;

            //clamp values between 0 and a 100
            if (capacityUtilization > 100) capacityUtilization = 100;
            if (capacityUtilization < 0) capacityUtilization = 0;

            //modify the capacity based on default or user input percentage
            population = Convert.ToInt32((capacityUtilization / 100) * (double)sw_Capacity);

            //if sidewalk has a subway then increase the population by the user defined multiplyer
            if (HasSubway == true)
            {
                //clamp values between 1 and 2
                if (subwayInfluence < 1) subwayInfluence = 1;
                else if (subwayInfluence > 2) subwayInfluence = 2;

                population = Convert.ToInt32((double)population * subwayInfluence);
            }

            sw_Population = population;

            return population;
        }
    }
}
