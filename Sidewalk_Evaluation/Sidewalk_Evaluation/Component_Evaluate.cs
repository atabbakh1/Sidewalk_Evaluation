using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using Sidewalk_Evaluation.Utility;
using System.Drawing;

namespace Sidewalk_Evaluation
{
    public class Component_Evaluate : GH_Component
    {


        private readonly List<string> textTags = new List<string>();
        private readonly List<Point3d> textTagsAnchors = new List<Point3d>();
        Color tagsColor = Color.LimeGreen;


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Component_Evaluate()
          : base("Evaluate", "E",
              "Evaluate input sidewalks population based on a social distancing radius",
              "SidewalkEval", "Evaluate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Sidewalk Curves", "SW", "Curves curves representing sidewalk outer edge", GH_ParamAccess.list);
            pManager.AddCurveParameter("Building Curves", "B", "(optional) Closed curves representing building footprints to consider in the evaluation", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddNumberParameter("Radius", "R", "The radius of the circle each pedastrian will rpresent -- default is 3", GH_ParamAccess.item, 3);
            pManager[2].Optional = true;
            pManager.AddCircleParameter("Trees", "T", "(optional) Circles representing trees to consider in the evaluation", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddCurveParameter("Subway", "S", "(optional) closed curves representing subway entry point to consider in the evaluation", GH_ParamAccess.list);
            pManager[4].Optional = true;
            pManager.AddNumberParameter("Subway Influence", "SI", "population multiplier for sidewalks with subway -- between 1 and 2\n\n 1 = No influence \n 2 = double population", GH_ParamAccess.item, 1.25);
            pManager[5].Optional = true;
            pManager.AddNumberParameter("Capacity Utilization", "CU", "Percentage of the individual sidewalk capacity to populate -- between 0 and a 100", GH_ParamAccess.item, 10);
            pManager[6].Optional = true;
            pManager.AddColourParameter("Color", "CO", "Color to apply to count tags", GH_ParamAccess.item, Color.LimeGreen);
            pManager[7].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Sidewalks_T", "S", "Sidewalks data tree", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Buildings_T", "B", "Building data tree", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Counts_T", "C", "Pedastrian Counts per sidewalk", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //input
            List<Curve> sidewalkInputCurves = new List<Curve>();
            List<Curve> buildingFootprintsInput = new List<Curve>();
            double radiusInput = 3;
            List<Circle> treesCirclesInput = new List<Circle>();
            List<Curve> subwayCurvesInput = new List<Curve>();
            double subwayInfluence = 1.25;
            double capacityUtilization = 50;

            //output
            DataTree<Curve> sidewalksDataTree = new DataTree<Curve>();
            DataTree<Curve> buildingsDataTree = new DataTree<Curve>();
            DataTree<int> countsDataTree = new DataTree<int>();



            bool considerBuildings = false;
            bool considerTrees = false;
            bool considerSubway = false;
            Curve[] joinedBuildings = null;
            int population;

            if (!DA.GetDataList(0, sidewalkInputCurves)) return;
            if (!DA.GetDataList(1, buildingFootprintsInput)) considerBuildings = false;
            DA.GetData(2, ref radiusInput);
            if (!DA.GetDataList(3, treesCirclesInput)) considerTrees = false;
            if (!DA.GetDataList(4, subwayCurvesInput)) considerSubway = false;
            DA.GetData(5, ref subwayInfluence);
            DA.GetData(6, ref capacityUtilization);
            DA.GetData(7, ref tagsColor);
            

            if (buildingFootprintsInput != null && buildingFootprintsInput.Count > 0)
            {
                considerBuildings = true;
                //union all adjacent building for more efficency (union twice to get rid of interior courts -- donuts)
                joinedBuildings = Curve.CreateBooleanUnion(Curve.CreateBooleanUnion(buildingFootprintsInput, 0.1), 0.1);
            }
            if(treesCirclesInput != null && treesCirclesInput.Count > 0)
            {
                considerTrees = true;
            }

            if (subwayCurvesInput != null && subwayCurvesInput.Count > 0)
            {
                considerSubway = true;
            }

            double pedastrianArea = (3.14) * radiusInput * radiusInput;


            #region PROCESS SIDEWALK CURVES

            List<Sidewalk> sidewalkInstances = new List<Sidewalk>();


            if (sidewalkInputCurves != null && sidewalkInputCurves.Count > 0)
            {

                for (int i = 0; i < sidewalkInputCurves.Count; i++)
                {
                    if(sidewalkInputCurves[i].IsClosed)     //some curves are not closed from the dataset
                    {

                        //init new sidewalk object
                        Sidewalk sw = new Sidewalk();
                        sw.Sidewalk_Curve = sidewalkInputCurves[i];                                         //store the sidewalk curve
                        sw.Sidewalk_Centroid = GeometricOps.CalculateCentroid(sidewalkInputCurves[i]);      //store the center point
                        sw.HasSubway = false;                                                               //assume it doesn't have a subway first
                        sw.HasTrees = false;                                                                //assume it doesn't have trees first


                        #region HANDLE BUILDING FOOTPRINTS
                        //if user provided building curves
                        if (considerBuildings == true)
                        {
                            List<Curve> insideThisSidewalk = new List<Curve>();

                            //check for containment against the current sidewalk curve
                            if (joinedBuildings != null && joinedBuildings.Length > 0)
                            {
                                for (int j = 0; j < joinedBuildings.Length; j++)
                                {
                                    if (GeometricOps.IsInsideCurve(sidewalkInputCurves[i], buildingFootprintsInput[j]))
                                    {
                                        insideThisSidewalk.Add(buildingFootprintsInput[j]);
                                    }
                                }
                            }

                            //if any buildings are inside this sidewalk curve
                            if (insideThisSidewalk.Count > 0)
                            {
                                sw.IsROW = true;                                                                                    //sidewalk instance is a Right-Of-Way type 
                                sw.Sidewalk_Buildings = insideThisSidewalk;                                                         //store buildings that belong to this sidewalk
                                sw.Sidewalk_Area = GeometricOps.CalculateStencilArea(sidewalkInputCurves[i], insideThisSidewalk);   //calculate the area of the sidewalk minus that of the buildings 
                            }
                        }
                        //if the sidewalk doesn't have any building then just calculate its individual area
                        if (sw.IsROW == false)
                        {
                            sw.Sidewalk_Area = GeometricOps.CalculateArea(sidewalkInputCurves[i]);      //single part area
                        }

                        #endregion

                        #region HANDLE TREES
                        //if user provided tree curves
                        if (considerTrees == true)
                        {
                            for (int j = 0; j < treesCirclesInput.Count; j++)
                            {
                                List<Curve> trees = new List<Curve>();
                                ArcCurve treeCurve = new ArcCurve(treesCirclesInput[j]);                    //convert to ArcCurve to access Curve tools
                                if (GeometricOps.InsideOrIntersecting(sidewalkInputCurves[i], treeCurve))   //check for either intersection or containment with current sidewalk curve
                                {
                                    trees.Add(treeCurve);                                                   //add to this sidewalk trees property list
                                    sw.Sidewalk_Area -= GeometricOps.CalculateArea(treeCurve);              //subtract the tree area from the current sidewalk area
                                }
                                sw.HasTrees = true;
                                sw.Sidewalk_Trees = trees;
                            }
                        }
                        #endregion

                        #region HANDLE SUBWAY

                        //if user provided subway entry curves
                        if (considerSubway == true)
                        {
                            for (int j = 0; j < subwayCurvesInput.Count; j++)
                            {
                                if (GeometricOps.InsideOrIntersecting(sidewalkInputCurves[i], subwayCurvesInput[j]))    //check for either intersection or containment with current sidewalk curve
                                {
                                    sw.HasSubway = true;
                                    sw.Sidewalk_Subway = subwayCurvesInput[j];                                          //add to this sidewalk subway propery
                                }
                            }
                        }

                        #endregion


                        //calculate the estimated number of circles this sidewalk can fit based on the final area and pedastrian radius
                        sw.Capacity = Convert.ToInt32(sw.Sidewalk_Area / pedastrianArea);
                        //if user inputs a number larger than a 100
                        if (capacityUtilization > 100)
                            capacityUtilization = 100;

                        //modify the capacity based on default or user input percentage
                        population = Convert.ToInt32((capacityUtilization / 100) * (double)sw.Capacity);

                        //if sidewalk has a subway then increase the population by the user defined multiplyer
                        if (sw.HasSubway == true)
                        {
                            //clamp values between 1 and 2
                            if (subwayInfluence < 1)
                                subwayInfluence = 1;
                            else if (subwayInfluence > 2)
                                subwayInfluence = 2;

                            population = Convert.ToInt32((double)population * subwayInfluence);

                        }

                        sw.Population = population;
                        textTags.Add(sw.Population.ToString());
                        textTagsAnchors.Add(sw.Sidewalk_Centroid);

                        //add to the global list of sidewalk instances
                        sidewalkInstances.Add(sw);
                    }
                   
                }

            }
            #endregion

            if (sidewalkInstances.Count > 0)
            {

                GH_Path path = new GH_Path(0); ;

                for (int i=0; i<sidewalkInstances.Count; i++)
                {
                    if (sidewalkInstances[i].Population > 0)
                    {
                        sidewalksDataTree.Add(sidewalkInstances[i].Sidewalk_Curve, path);

                        if (sidewalkInstances[i].IsROW == true) buildingsDataTree.AddRange(sidewalkInstances[i].Sidewalk_Buildings, path);
                        else buildingsDataTree.Add(null, path);

                        countsDataTree.Add(sidewalkInstances[i].Population, path);

                        path = new GH_Path(countsDataTree.BranchCount);
                    }

                }
            }


            DA.SetDataTree(0, sidewalksDataTree);
            DA.SetDataTree(1, buildingsDataTree);
            DA.SetDataTree(2, countsDataTree);
        }

        protected override void BeforeSolveInstance()
        {
            textTags.Clear();
            textTagsAnchors.Clear();

            base.BeforeSolveInstance();
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            
            for (int i = 0; i < textTags.Count; i++)
            {

                Plane plane;
                args.Viewport.GetCameraFrame(out plane);
                plane.Origin = textTagsAnchors[i];

                double pixelsPerUnit;
                args.Viewport.GetWorldToScreenScale(textTagsAnchors[i], out pixelsPerUnit);

                args.Display.Draw3dText(textTags[i], tagsColor, plane, 25 / pixelsPerUnit, "Lucida Console");
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icons_Base_Populate.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d21e034b-2d5e-4321-b39b-16de6c5721f0"); }
        }
    }
}