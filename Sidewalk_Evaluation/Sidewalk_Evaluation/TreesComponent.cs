using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Sidewalk_Evaluation.Utility;
using Grasshopper.Kernel.Parameters;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Sidewalk_Evaluation
{
    public class TreesComponent : GH_Component
    {
        private string[] NYC_BOROUGHS = new string[] {"Manhattan", "Queens", "Brooklyn", "Bronx", "Staten Island" };


        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TreesComponent()
          : base("NYC Trees", "T",
              "Create Rhino circles representing tree data loaded from a CSV file",
              "SidewalkEval", "Factors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CSV_Path", "P", "The path to the CSV file containing the trees dataset.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("X_Index", "X", "The index of the CSV column that contains the X coordinates of the tree location.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Y_Index", "Y", "The index of the CSV column that contains the Y coordinates of the tree location.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("DBH_Index", "D", "The index of the CSV column that contains the DBH (Diameter at Breast Height) of the tree.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Borough_Index", "B", "The index of the CSV column that contains the borough information the tree belongs to.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Borough", "TB", "The target borough to load the trees from -- Default value is 0: \n\n" +
                                                          "0 = Manhattan \n" +
                                                          "1 = Queens \n" +
                                                          "2 = Brooklyn \n" +
                                                          "3 = Bronx \n" +
                                                          "4 = Staten Island", GH_ParamAccess.item, 0);

            Param_Integer param = pManager[5] as Param_Integer;
            for(int i=0; i< NYC_BOROUGHS.Length; i++)
            {
                param.AddNamedValue(NYC_BOROUGHS[i], i);
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCircleParameter("Trees", "T", "Circles representing trees.", GH_ParamAccess.list);
            pManager.AddPointParameter("Centers", "C", "Points represnting the centers of ouput trees.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            string path = "";
            int x_i = 0;
            int y_i = 0;
            int dbh_i = 0;
            int borough_i = 0;
            int boroughTarget = 0;

            if (!DA.GetData(0, ref path)) return;
            if (!DA.GetData(1, ref x_i)) return;
            if (!DA.GetData(2, ref y_i)) return;
            if (!DA.GetData(3, ref dbh_i)) return;
            if (!DA.GetData(4, ref borough_i)) return;
            if (!DA.GetData(5, ref boroughTarget)) return;


            // a list to store tree circles
            List<Circle> trees = new List<Circle>();
            List<Point3d> treesCenters = new List<Point3d>();
            //retreive filtered tree data based on the requested borough
            string[] csvData = Helpers.RetreiveCSVData(path, borough_i, NYC_BOROUGHS[boroughTarget]);


            if (csvData != null && csvData.Length > 0)
            { 
                //create a tree circle for each data record
                for (int i = 1; i < csvData.Length; i++)
                {
                    Point3d center;
                    trees.Add(GeometricOps.CreateCircleFromCSV(csvData[i], x_i, y_i, dbh_i, out center));
                    treesCenters.Add(center);
                }

                //return the tree circles as first ouptut
                DA.SetDataList(0, trees);
                DA.SetDataList(1, treesCenters);
            }

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return Properties.Resources.Icons_Base_Tree.ToBitmap();
               
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3712ea0e-e7f5-4a8a-b73c-12e27c30e38d"); }
        }
    }
}
