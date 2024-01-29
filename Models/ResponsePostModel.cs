using PulsenicsTechAs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PulsenicsTechAs.Models
{
    public class ResponsePostModel : IPolyfit, IResponseMeta
    {
        public int PlotId { get; set; }
        public int? PolyfitPower { get; set; }
        public List<double> Scalars { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }

        public ResponsePostModel() 
        {
            Scalars = new List<double>();
        }
    }
}