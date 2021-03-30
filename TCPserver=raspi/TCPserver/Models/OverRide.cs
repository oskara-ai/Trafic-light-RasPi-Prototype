using System;
namespace TCPserver.Models
{
    public class OverRide
    {
		public string Command { get; } = "OR";

		public int Green 
		{
			get;
			set;
		}
		public int Yellow
        {
            get;
            set;
        }
		public int Red
        {
            get;
            set;
        }
    }
}
