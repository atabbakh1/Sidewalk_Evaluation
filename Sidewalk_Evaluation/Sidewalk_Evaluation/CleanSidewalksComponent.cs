using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Linq; 
using System.Collections.Generic;
using Sidewalk_Evaluation.Utility;
using Grasshopper;

namespace Sidewalk_Evaluation
{
    public class CleanSidewalksComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SidewalkCurvesComponent class.
        /// </summary>
        public CleanSidewalksComponent()
          : base("Clean Sidewalks", "CSW",
              "Extracts and reduce the sidewalk regions from input curves based on building footprints",
              "SidewalkEval", "Factors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("sidewalk curves", "S", "Closed curves representing sidewalk region", GH_ParamAccess.list);
            pManager.AddCurveParameter("Building Curves", "B", "Closed curves representing building footprints to difference from sidewalk regions", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Clean Sidewalks", "S", "Closed curve representing sidewalk regions extracted from the input sidewalk curves and building footprints", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Clean Buildings", "B", "Closed curve reduced building footprints", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //input
            List<Curve> sidewalksInput = new List<Curve>();
            List<Curve> bldgsFootprintInput = new List<Curve>();

            //output
            DataTree<Curve> differencedSidewalks = new DataTree<Curve>();
            Curve[] unionFootprints;
   
            if (!DA.GetDataList(0, sidewalksInput))
                return;
            if (!DA.GetDataList(1, bldgsFootprintInput))
                return;


            List<Curve> cleanedSidewalkCurves = GeometricOps.CleanCurves(sidewalksInput); 
            List<Curve> cleanedbldgsFootprint = GeometricOps.CleanCurves(bldgsFootprintInput);
            List<Point3d> footPrintsCenters = new List<Point3d>();
            List<Curve> cuttingFootPrints = new List<Curve>();
            GH_Path sidewalksTreeBranchPath = new GH_Path(0);

            //boolean union all clean building foot prints -- twice to avoid interior courts
            unionFootprints = Curve.CreateBooleanUnion(Curve.CreateBooleanUnion(cleanedbldgsFootprint,0.1),0.1);

            //store the centers of the resultant building footprints clusters
            for (int i=0; i< unionFootprints.Length; i++)
            {
                footPrintsCenters.Add(AreaMassProperties.Compute(unionFootprints[i]).Centroid);
            }


            for(int i=0; i< cleanedSidewalkCurves.Count; i++)
            {
                //check for footprint cluster center containment against sidewalk curves
                for (int j = 0; j < footPrintsCenters.Count; j++)
                {
                    if (cleanedSidewalkCurves[i].Contains(footPrintsCenters[j], Plane.WorldXY, 0.1) == PointContainment.Inside)
                    {
                        //add that cluster into a list of curves that will be used to difference from the current sidewalk curves
                        cuttingFootPrints.Add(unionFootprints[j]);
                    }
                }
                

                if(cuttingFootPrints.Count > 0)
                {
                    //cute the sidewalk using the contained footrpint cluster (if any)
                    Curve[] cut = Curve.CreateBooleanDifference(cleanedSidewalkCurves[i], cuttingFootPrints, 0.1);

                    //store the curves of the sidewalk result into a tree branch
                    if(cut.Length > 0)
                    {
                        for(int j=0; j<cut.Length; j++)
                        {
                            differencedSidewalks.Add(cut[j], sidewalksTreeBranchPath);
                        }
                    }
                }

                //increment through the tree branch each time a new sidewalk curve is handled
                sidewalksTreeBranchPath = new GH_Path(differencedSidewalks.BranchCount);


            }


            //return results
            DA.SetDataTree(0, differencedSidewalks);
            DA.SetDataList(1, unionFootprints);


        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.Icons_Base_Sidewalk.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("47c5781b-91c9-46a1-baf1-3f52f96d2800"); }
        }
    }
}