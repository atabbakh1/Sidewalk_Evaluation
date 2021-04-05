using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using Sidewalk_Evaluation.Utility;
using Grasshopper;

namespace Sidewalk_Evaluation
{
    class Sidewalk
    {
        public GH_Curve Sidewalk_Curve { get; set; }
        public List<GH_Curve> Sidewalk_Buildings { get; set; }
        public GH_Boolean IsInterior { get; set; }
        public GH_Number Sidewalk_Area { get; set; }
        public List<GH_Circle> Sidewalk_Trees { get; set; }
        public GH_Point Sidewalk_Subway { get; set; }


        public Sidewalk()
        {

        }
    }
}
