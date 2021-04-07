using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry.Intersect;
using System.Diagnostics;
using Sidewalk_Evaluation.Utility;


namespace Sidewalk_Evaluation
{
    public class Componenet_PrepSidewalks : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Componenet_PrepSidewalks class.
        /// </summary>
        public Componenet_PrepSidewalks()
          : base("Prepare Sidewalks", "PSW",
              "Create sidewalk instances for evaluation from a set of curves. Sidewalks are organized into ROW (Right-OF-Way) sidewalks and Interior sidewalks.",
              "SidewalkEval", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("sidewalk curves", "S", "Closed curves representing sidewalk regions", GH_ParamAccess.list);
            pManager.AddCurveParameter("Building Curves", "B", "Closed curves representing building footprints", GH_ParamAccess.list);
            pManager.AddCurveParameter("Region Curve", "R", "A curve defining the scope of sidewalk evaluation -- For optimized performance", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Sidewalks", "S", "Sidewalks inside or intersecting with region curve", GH_ParamAccess.list);
            pManager.AddCurveParameter("Building Clusters", "B", "Joined building footprints inside or intersecting with region curve", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //input
            List<Curve> sidewalkCurvesInput = new List<Curve>();
            List<Curve> buildingCurvesInput = new List<Curve>();
            Curve regionCurve = null;

            //output
            List<Curve> sidewalksOutput = new List<Curve>();
            List<Curve> buildingsOutput = new List<Curve>();


            if (!DA.GetDataList(0, sidewalkCurvesInput)) return;
            if (!DA.GetDataList(1, buildingCurvesInput)) return;
            if (!DA.GetData(2, ref regionCurve)) return;


            //check for sidewalk containment/intersection against region curve
            if (regionCurve != null && sidewalkCurvesInput.Count > 0)
            {
                for(int i=0; i<sidewalkCurvesInput.Count; i++)
                {
                    if(sidewalkCurvesInput[i].IsClosed)     //some curves are not closed from the dataset
                    {
                        if (GeometricOps.InsideOrIntersecting(regionCurve, sidewalkCurvesInput[i]))
                        {
                            sidewalksOutput.Add(Curve.ProjectToPlane(sidewalkCurvesInput[i], Plane.WorldXY));
                        }
                    }

                }

            }

            //check for building containment/intersection against region curve
            if(buildingCurvesInput.Count > 0)
            {
                //union buildings for loop efficiency -- seemed to improve performance 
                Curve[] joinedBuildings = Curve.CreateBooleanUnion(buildingCurvesInput, 0.1);

                for(int i=0; i<joinedBuildings.Length; i++)
                {
                    if(joinedBuildings[i].IsClosed)     //some curves are not closed from the dataset
                    {
                        if (GeometricOps.InsideOrIntersecting(regionCurve, joinedBuildings[i]))
                        {
                            buildingsOutput.Add(Curve.ProjectToPlane(joinedBuildings[i], Plane.WorldXY));
                        }
                    }
                }
            }

            if(buildingsOutput.Count > 0)
            {
                buildingsOutput = Curve.CreateBooleanUnion(buildingsOutput, 0.1).ToList();      //boolean buildings again to get rid of interior courts
            }

            if (sidewalksOutput.Count > 0)
            {
                sidewalksOutput = Curve.CreateBooleanUnion(sidewalksOutput, 0.1).ToList();      //handle pathways within larger sidewalks for now (needs to be modified)
            }

            DA.SetDataList(0, sidewalksOutput);
            DA.SetDataList(1, buildingsOutput);
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
            get { return new Guid("1cd04d51-1571-47ae-ab66-c6350efe2636"); }
        }
    }
}