using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable 8618

namespace J4JSoftware.EFCoreUtilities
{
    public class DatabaseConfig : IDatabaseConfig
    {
        public string Path { get; set; }
        public bool CreateNew { get; set; }
    }
}
