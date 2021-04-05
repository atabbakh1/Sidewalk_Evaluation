using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry.Intersect;
using System.Diagnostics;

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
            pManager.AddCurveParameter("Region Curve", "R", "A curve defining the scope of sidewalk organization -- For optimized performance", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("ROW", "R", "Sidewalks directly adjacent to buildings -- Right Of Way", GH_ParamAccess.list);
            pManager.AddCurveParameter("Interior", "IN", "Sidewalks with no direct relation to buildings -- pathways", GH_ParamAccess.list);
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
            List<Curve> sidewalks_Row_ouput = new List<Curve>();
            List<Curve> sidewalks_Interior_ouput = new List<Curve>();


            if (!DA.GetDataList(0, sidewalkCurvesInput)) return;
            if (!DA.GetDataList(1, buildingCurvesInput)) return;
            if (!DA.GetData(2, ref regionCurve)) return;


            List<Curve> filteredSidewalkInput = new List<Curve>();

            if (regionCurve != null && sidewalkCurvesInput.Count > 0)
            {
                for(int i=0; i<sidewalkCurvesInput.Count; i++)
                {
                    RegionContainment relationship = Curve.PlanarClosedCurveRelationship(regionCurve, sidewalkCurvesInput[i], Plane.WorldXY, 0.1);
                    if (relationship == RegionContainment.BInsideA || relationship == RegionContainment.MutualIntersection)
                    {
                        filteredSidewalkInput.Add(sidewalkCurvesInput[i]);
                    }
                }

            }

            else if(regionCurve == null)
            {
                filteredSidewalkInput = sidewalkCurvesInput;
            }

            //boolean union all building foot prints
            Curve[] joinedBuildings = null;
            if(buildingCurvesInput.Count > 0)
            {
                joinedBuildings = Curve.CreateBooleanUnion(buildingCurvesInput, 0.1);
            }

            if(filteredSidewalkInput != null && joinedBuildings != null)
            {
                if (filteredSidewalkInput.Count > 0 && joinedBuildings.Length > 0)
                {
                    for (int i = 0; i < filteredSidewalkInput.Count; i++)
                    {
                        bool hasBuildings = false;
                        for (int j = 0; j < joinedBuildings.Length; j++)
                        {
                            //check for building containment against filtered sidewalks
                            RegionContainment relationship = Curve.PlanarClosedCurveRelationship(filteredSidewalkInput[i], joinedBuildings[j], Plane.WorldXY, 0.1);
                            if (relationship == RegionContainment.BInsideA)
                            {
                                hasBuildings = true;
                            }
                        }

                        if (hasBuildings == true)
                        {
                            sidewalks_Row_ouput.Add(filteredSidewalkInput[i]);
                        }
                        else
                        {
                            sidewalks_Interior_ouput.Add(filteredSidewalkInput[i]);
                        }
                    }
                }
            }

            DA.SetDataList(0, sidewalks_Row_ouput);
            DA.SetDataList(1, sidewalks_Interior_ouput);
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