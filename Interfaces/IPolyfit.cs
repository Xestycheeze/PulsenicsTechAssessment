using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsenicsTechAs.Interfaces
{
    internal interface IPolyfit
    {
        int PlotId { get; set; }
        int? PolyfitPower { get; set; }
        List<double> Scalars { get; set; } // The first term is the a0 (bias), then a1, a2... etc
    }
}
