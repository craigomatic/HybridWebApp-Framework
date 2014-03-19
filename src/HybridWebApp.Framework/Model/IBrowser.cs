using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.Model
{
    public interface IBrowser
    {
        event EventHandler<Uri> LoadCompleted;
    }
}
