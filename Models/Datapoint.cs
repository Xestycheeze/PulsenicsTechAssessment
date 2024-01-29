using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PulsenicsTechAs.Interfaces;

namespace PulsenicsTechAs.Models
{
    public class Datapoint : IDatapoint
    {
        public int DatapointId { get; set; }
        public double XCoor { get; set; }
        public double YCoor { get; set; }
    }
}