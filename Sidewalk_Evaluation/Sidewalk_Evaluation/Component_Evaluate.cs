using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sidewalk_Evaluation
{
    public class Component_Evaluate : GH_Component
    {

        List<Sidewalk> sidewalkInstances = new List<Sidewalk>();


        private readonly List<string> textTags = new List<string>();
        private readonly List<Point3d> textTagsAnchors = new List<Point3d>();
        private readonly List<Curve> sidewalkOutlines = new List<Curve>();

        Color tagsColor = Color.LimeGreen;
        Color outlinesColor = Color.LimeGreen;




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
            pManager.AddNumberParameter("Social Radius", "SR", "The radius of the circle each pedastrian will rpresent -- default is 3", GH_ParamAccess.item, 3);
            pManager[2].Optional = true;
            pManager.AddCircleParameter("Trees", "T", "(optional) Circles representing trees to consider in the evaluation", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddCurveParameter("Subway", "S", "(optional) closed curves representing subway entry point to consider in the evaluation", GH_ParamAccess.list);
            pManager[4].Optional = true;
            pManager.AddNumberParameter("Subway Influence", "SI", "population multiplier for sidewalks with subway -- between 1 and 2\n\n 1 = No influence \n 2 = double population", GH_ParamAccess.item, 1.25);
            pManager[5].Optional = true;
            pManager.AddNumberParameter("Capacity Utilization", "CU", "Percentage of the individual sidewalk capacity to populate -- between 0 and 100", GH_ParamAccess.item, 10);
            pManager[6].Optional = true;
            pManager.AddColourParameter("Tags Color", "TC", "Color to apply to count tags", GH_ParamAccess.item, Color.LimeGreen);
            pManager[7].Optional = true;
            pManager.AddColourParameter("Sidewalks Color", "SC", "Color to apply to sidewalk outlines", GH_ParamAccess.item, Color.Orange);
            pManager[8].Optional = true;
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
            double socialRadiusInput = 3;
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

            if (!DA.GetDataList(0, sidewalkInputCurves)) return;
            if (!DA.GetDataList(1, buildingFootprintsInput)) considerBuildings = false;
            DA.GetData(2, ref socialRadiusInput);
            if (!DA.GetDataList(3, treesCirclesInput)) considerTrees = false;
            if (!DA.GetDataList(4, subwayCurvesInput)) considerSubway = false;
            DA.GetData(5, ref subwayInfluence);
            DA.GetData(6, ref capacityUtilization);
            DA.GetData(7, ref tagsColor);
            DA.GetData(8, ref outlinesColor);
            


            if (buildingFootprintsInput != null && buildingFootprintsInput.Count > 0)
            {
                considerBuildings = true;
                //union all adjacent building for more efficency (union twice to get rid of interior courts -- donuts)
                joinedBuildings = Curve.CreateBooleanUnion(Curve.CreateBooleanUnion(buildingFootprintsInput, 0.1), 0.1);
            }
            if(treesCirclesInput != null && treesCirclesInput.Count > 0) considerTrees = true;
            if (subwayCurvesInput != null && subwayCurvesInput.Count > 0) considerSubway = true;


            //iterate through each sidewalk curve and create an object for it
            if (sidewalkInputCurves != null && sidewalkInputCurves.Count > 0)
            {
                for (int i = 0; i < sidewalkInputCurves.Count; i++)
                {
                    if(sidewalkInputCurves[i].IsClosed)     //some curves are not closed from the dataset
                    {

                        //init new sidewalk object with the current curve
                        Sidewalk sw = new Sidewalk(sidewalkInputCurves[i]);

                        //if user provided building curves
                        if (considerBuildings == true) sw.CheckForBuildings(joinedBuildings);
                        //if user provided tree curves
                        if (considerTrees == true) sw.CheckForTrees(treesCirclesInput);
                        //if user provided subway entry curves
                        if (considerSubway == true) sw.CheckForSubway(subwayCurvesInput);

                        sw.CalculateCapacity(socialRadiusInput);      //capacity based on social radius
                        sw.CalculatePopulation(capacityUtilization, subwayInfluence);       //population based on requested utilization and subway influence multiplyer

                        //for previewing count information as 3D text and outline sidewalks
                        textTags.Add(sw.Population.ToString());
                        textTagsAnchors.Add(sw.Sidewalk_Centroid);
                        sidewalkOutlines.Add(sw.Sidewalk_Curve);
                        sidewalkOutlines.AddRange(sw.Sidewalk_Buildings);

                        //add to the global list of sidewalk instances
                        sidewalkInstances.Add(sw);
                    }
                   
                }

            }


            //populate the datatrees with sidewalk data
            if (sidewalkInstances.Count > 0)
            {
                //to make sure all data trees have equal branches
                GH_Path path = new GH_Path(0);

                for (int i=0; i<sidewalkInstances.Count; i++)
                {
                    if (sidewalkInstances[i].Population > 0)
                    {
                        //add sidewalk curve
                        sidewalksDataTree.Add(sidewalkInstances[i].Sidewalk_Curve, path);

                        //add buildings (if any)
                        if (sidewalkInstances[i].IsROW == true) buildingsDataTree.AddRange(sidewalkInstances[i].Sidewalk_Buildings, path);
                        else buildingsDataTree.Add(null, path); //if no buildings keep the branch but add a null data item

                        //add population count
                        countsDataTree.Add(sidewalkInstances[i].Population, path);

                        //increment tree path
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
            //reset the supplied data for the preview on each solution
            textTags.Clear();
            textTagsAnchors.Clear();
            sidewalkOutlines.Clear();


            base.BeforeSolveInstance();
        }


        /// <summary>
        /// Draw all meshes in this method
        /// </summary>
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
        /// Draw all wires and points in this method.
        /// </summary>
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            for (int i = 0; i < sidewalkOutlines.Count; i++)
            {
                args.Display.DrawCurve(sidewalkOutlines[i], outlinesColor, 5);
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