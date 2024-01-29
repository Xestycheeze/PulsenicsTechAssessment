using PulsenicsTechAs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PulsenicsTechAs.Models
{
    public class ResponseGet : IPlot, IPolyfit
    {
        // Id and PlotId declared separately for potential cross-plot interaction in the future
        public int Id { get; set; }
        public int PlotId { get; set; }
        public int PolyPower { get; set; }
        public int? PolyfitPower { get; set; }
        public List<double> Scalars { get; set; }
        public List<Datapoint> LstDatapoint { get; set; }
        public ResponseGet()
        {
            Scalars = new List<double>();
            LstDatapoint = new List<Datapoint>();
        }
    }
}