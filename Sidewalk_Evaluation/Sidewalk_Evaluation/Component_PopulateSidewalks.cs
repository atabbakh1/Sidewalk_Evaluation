using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Sidewalk_Evaluation
{
    public class Component_PopulateSidewalks : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Component_PopulateSidewalks()
          : base("Populate", "P",
              "Populate input sidewalks with circles representing pedestrians",
              "SidewalkEval", "Evaluate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("ROW Sidewalks", "R", "Sidewalks directly adjacent to buildings -- Right Of Way", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager.AddCurveParameter("Interior Sidewalks", "I", "Sidewalks with no direct relation to buildings -- pathways", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddCurveParameter("Trees", "T", "closed curves/circles representing trees", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddCurveParameter("Subway", "S", "closed curves representing subway entry point", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddNumberParameter("Subway Radius", "SR", "The radius of the subway entrance area of influence -- default is 100", GH_ParamAccess.item, 100);
            pManager[4].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Pedastrians", "P", "Circles representing pedastrians", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
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