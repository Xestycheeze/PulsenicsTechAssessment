using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PulsenicsTechAs.Interfaces;

namespace PulsenicsTechAs.Models
{
    public class ResponseGetModel: IResponseMeta
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<ResponseGet> LstPlot { get; set; }

        public ResponseGetModel()
        {
            LstPlot = new List<ResponseGet>();
        }
    }

    

}