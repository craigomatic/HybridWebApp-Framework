using HybridWebApp.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework
{
    public class WebRoute
    {
        public Uri CurrentUri { get; private set; }

        public Uri Root { get; private set; }

        public Interpreter Interpreter { get; private set; }

        public IBrowser Browser { get; private set; }

        private Dictionary<string, List<MappedRoute>> _MappedRoutes;

        public WebRoute(Uri rootUri, Interpreter interpreter, IBrowser browser)
        {
            this.Root = rootUri;
            this.Interpreter = interpreter;
            this.Browser = browser;
            this.Browser.LoadCompleted += Browser_LoadCompleted;
            this.Browser.NavigationFailed += Browser_NavigationFailed;
            this.Browser.Navigating += Browser_Navigating;

            _MappedRoutes = new Dictionary<string, List<MappedRoute>>();
        }

        /// <summary>
        /// Maps a route to an action that is invoked when the browser loaded completed event has been raised
        /// </summary>
        /// <param name="fragment"></param>
        /// <param name="action"></param>
        /// <param name="runOnce"></param>
        public void Map(string fragment, Func<Task> action, bool runOnce = false)
        {
            List<MappedRoute> mappedRoutes = null;

            if(!_MappedRoutes.TryGetValue(fragment, out mappedRoutes))
            {
                mappedRoutes = new List<MappedRoute>();

                _MappedRoutes.Add(fragment, mappedRoutes);
            }

            mappedRoutes.Add(new MappedRoute { Action = action, RunOnce = runOnce });
        }

        private Action<Uri> _OtherHostsAction;

        public void MapOtherHosts(Action<Uri> action)
        {
            _OtherHostsAction = action;
        }

        public void NavigateHome()
        {
            this.Interpreter.Eval(string.Format("framework.routeTo('{0}');", this.Root));
        }

        public void Navigate(NavItem navItem)
        {
            this.Navigate(navItem.Href);
        }

        public void Navigate(string href)
        {
            this.Interpreter.Eval(string.Format("framework.routeTo('{0}');", href));
        }

        #region Browser event handlers

        void Browser_Navigating(object sender, WrappedNavigatingEventArgs e)
        {
            if (e.Uri.Host != this.Root.Host)
            {
                if (_OtherHostsAction != null)
                {
                    _OtherHostsAction(e.Uri);

                    e.Cancel = true;
                }
            }
        }

        void Browser_NavigationFailed(object sender, Uri e)
        {
            this.CurrentUri = e;
        }

        private async void Browser_LoadCompleted(object sender, Uri e)
        {
            this.CurrentUri = e;

            List<MappedRoute> mappedRoute = null;

            if (_MappedRoutes.TryGetValue(e.Fragment, out mappedRoute))
            {
                foreach (var route in mappedRoute)
                {
                    if (!route.RunOnce || (route.RunOnce && route.Hits == 0))
                    {
                        route.Hits++;
                        await route.Action();
                    }
                }
            }
        }

        #endregion
    }
}
