using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraficLight.Models
{
    class LEDsequence
    {
        public int Timer { get; set; }
        public string Led { get; set; }
        public int Command { get; set; }
        public override string ToString()
        {
            return $"Led:{this.Led}, Timer:\"{this.Timer}\", Command:{this.Command}";
        }
    }
}
