using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PackegeContext : DbContext
    {
        public PackegeContext() : base("Packeges")
        {
        }
        public DbSet<Package> Packages { get; set; }
        
    }
    public class Package
    {
        public int Id { get; set; }

        public string Char { get; set; }
        public string Date { get; set; }

        public string MachineName { get; set; }

        public string OS { get; set; }
    }
}
