using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class Package
    {
        public string Char { get; set; }
        public string Date { get; set; }

        public string MachineName { get; set; }

        public string OS { get; set; }
    }
}
