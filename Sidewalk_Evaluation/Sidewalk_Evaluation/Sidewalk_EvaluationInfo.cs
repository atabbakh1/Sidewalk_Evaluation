using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Sidewalk_Evaluation
{
    public class Sidewalk_EvaluationInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "SidewalkEvaluation";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Evaluate sidewalk population with social distancing parameters and site factors like trees and subway entrances.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("2fdead7e-522f-4271-b452-a24b0b0506f9");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Ahmad Tabbakh";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "ahmadtabbakh.com";
            }
        }
    }
}
