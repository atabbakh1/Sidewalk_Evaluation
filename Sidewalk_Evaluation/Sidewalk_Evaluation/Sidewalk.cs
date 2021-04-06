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
        /// <summary>
        /// Sidewalk Rhino curve
        /// </summary>
        public Curve Sidewalk_Curve { get; set; }

        /// <summary>
        /// Sidewalk building curves as a list
        /// </summary>
        public List<Curve> Sidewalk_Buildings { get; set; }

        /// <summary>
        /// Is this a Right Of Way type sidewalk
        /// </summary>
        public bool IsROW { get; set; }

        /// <summary>
        /// The sidewalk total area excluding building and trees (if any)
        /// </summary>
        public double Sidewalk_Area { get; set; }

        public bool HasTrees { get; set; }
        /// <summary>
        /// the sidewalk tree curves as a list (if any)
        /// </summary>
        public List<Curve> Sidewalk_Trees { get; set; }

        public bool HasSubway { get; set; }
        /// <summary>
        /// the sidewalk subway point (if any)
        /// </summary>
        public Point3d Sidewalk_Subway { get; set; }

        /// <summary>
        /// The sidewalk estimated pedastrian capacity
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        ///The sidewalk pedastrian curves
        /// </summary>
        public List<Circle> Sidewalk_Pedastrians { get; set; }


        public Sidewalk()
        {

        }
    }
}
