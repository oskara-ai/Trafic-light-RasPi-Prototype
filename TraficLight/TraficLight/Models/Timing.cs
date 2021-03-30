using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraficLight.Models
{
    class Timing
    {
        public int WaitTime { get; set; }
        public List<OverRide> Leds { get; set; }
    }
}
