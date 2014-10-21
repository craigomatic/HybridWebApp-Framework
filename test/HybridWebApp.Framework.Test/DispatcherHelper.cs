using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;

namespace HybridWebApp.Framework.Test
{
    public static class DispatcherHelper
    {
        public static IAsyncAction Dispatch(DispatchedHandler handler)
        {
            return CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
        }
    }
}
