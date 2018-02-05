using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace RealTimeServer
{
    public interface IClient
    {
        uint ClientId { get; set; }
        string Name { get; set; }
        EndPoint EP { get; set; }

        int RefuseTimes { get; set; }


        //last timestamp for received packets foreach type of packet


        //position
        //rotation
    }
}
