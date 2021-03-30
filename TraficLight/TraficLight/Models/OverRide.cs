using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraficLight.Models
{
    class OverRide
    {
        // Class for getting and setting information about leds state
        public string Command { get; } = "OR";
        public int Green { get; set; } = 0;
        public int Yellow { get; set; } = 0;
        public int Red { get; set; } = 0;
    }
}
