using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PulsenicsTechAs.Utils
{
    public class UtilsMisc
    {
        public static bool IsCurveFittingReady(int numbDatapoints, int reqPolyOrder)
        {
            return numbDatapoints >= (reqPolyOrder + 1) && reqPolyOrder > 0;
        }
    }
}