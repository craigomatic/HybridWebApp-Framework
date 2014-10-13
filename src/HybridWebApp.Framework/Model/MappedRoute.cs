using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.Model
{
    public class MappedRoute
    {
        public Func<Uri,bool,int, Task> Action { get; set; }

        public bool RunOnce { get; set; }

        public int Hits { get; set; }

        public RouteTiming Timing { get; set; }
    }
}
