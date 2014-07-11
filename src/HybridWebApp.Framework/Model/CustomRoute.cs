using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.Model
{
    public class CustomRoute
    {
        public Uri Redirect { get; set; }

        /// <summary>
        /// Function 
        /// </summary>
        public Func<Uri, CustomRouteAction> Evaluate { get; set; }

        public Func<Uri, bool, int, Task> Action { get; set; }
    }
}
