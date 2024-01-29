using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsenicsTechAs.Interfaces
{
    internal interface IResponseMeta
    {
        string ResponseCode { get; set; }
        string ResponseMessage { get; set; }
    }
}
