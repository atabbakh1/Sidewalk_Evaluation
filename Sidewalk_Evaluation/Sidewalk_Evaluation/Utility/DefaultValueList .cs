using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Sidewalk_Evaluation.Utility
{
    public class DefaultValueList : Grasshopper.Kernel.Special.GH_ValueList
    {
        public DefaultValueList(string Nickname, List<string> Keys, List<string> Values, int DefaultIndexValue, Grasshopper.Kernel.Special.GH_ValueListMode Mode)
        {
            var source = new Grasshopper.Kernel.Special.GH_ValueList();
            source.NickName = Nickname;
            source.ListItems.Clear();
            source.ClearData();
            for (int i = 0; i < Keys.Count; i++)
            {
                source.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem(Keys[i], Values[i]));
            }
            source.ListMode = Mode;
            source.SelectItem(DefaultIndexValue);
        }
    }
}