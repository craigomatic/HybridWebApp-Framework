using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace HybridWebApp.Framework.Test
{
    public class AsyncUITestRunner
    {
        private Func<Task> _Action;

        public AsyncUITestRunner(Func<Task> action)
        {
            _Action = action;
        }

        public async Task RunAsync()
        {
            var taskSource = new TaskCompletionSource<object>();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    await _Action();

                    taskSource.SetResult(null);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });

            await taskSource.Task;
        }
    }
}
