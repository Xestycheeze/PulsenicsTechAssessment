using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PulsenicsTechAs.Interfaces;

namespace PulsenicsTechAs.Models
{
    public class RequestPostModel: IPlot
    {
        public int Id { get; set; }
        public int PolyPower { get; set; }
        public int GrossNumbDatapoint { get; set; }
        public List<Datapoint> LstAddDatapoint { get; set; }
        public List<Datapoint> LstDeleteDatapoint { get; set; }
    }
}