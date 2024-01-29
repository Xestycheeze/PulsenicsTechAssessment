using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PulsenicsTechAs.Interfaces
{
    internal interface IDatapoint
    {
        // Comparing floating point values is inconsistent. Comparing their Ids are more stable at the expense of memory costs
        int DatapointId { get; set; } 
        double XCoor { get; set; }
        double YCoor { get; set; }
    }
}
