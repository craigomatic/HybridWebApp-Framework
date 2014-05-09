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

        private bool _MapOnNavigate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootUri"></param>
        /// <param name="interpreter"></param>
        /// <param name="browser"></param>
        /// <param name="mapOnNavigate">When true, the mappings will be evaluated when the Browser Navigated event is raised. When false, the mappings will be evaluated when the Browser Loaded event is raised.</param>
        public WebRoute(Uri rootUri, Interpreter interpreter, IBrowser browser, bool mapOnNavigate = true)
        {
            this.Root = rootUri;
            this.Interpreter = interpreter;
            this.Browser = browser;
            this.Browser.LoadCompleted += Browser_LoadCompleted;
            this.Browser.NavigationFailed += Browser_NavigationFailed;
            this.Browser.Navigating += Browser_Navigating;
            this.Browser.Navigated += Browser_Navigated;

            _MappedRoutes = new Dictionary<string, List<MappedRoute>>();
            _MapOnNavigate = mapOnNavigate;
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

        private async Task _EvaluateMappedRoutesAsync(Uri uri)
        {
            this.CurrentUri = uri;

            var mappedRoutes = _MappedRoutes.Where(r => uri.AbsolutePath.Contains(r.Key));

            foreach (var mappedRoute in mappedRoutes)
            {
                foreach (var route in mappedRoute.Value)
                {
                    if (!route.RunOnce || (route.RunOnce && route.Hits == 0))
                    {
                        route.Hits++;
                        await route.Action();
                    }
                }
            }
        }

        #region Browser event handlers

        private async void Browser_LoadCompleted(object sender, Uri uri)
        {
            if(!_MapOnNavigate)
            {
                await _EvaluateMappedRoutesAsync(uri);
            }
        }

        private void Browser_Navigating(object sender, WrappedNavigatingEventArgs e)
        {
            if (!e.Uri.AbsoluteUri.Contains(this.Root.AbsoluteUri)) //often mobile websites are example.org/m/ or m.example.org - best to do a contains comparison
            {
                if (_OtherHostsAction != null)
                {
                    _OtherHostsAction(e.Uri);

                    e.Cancel = true;
                }
            }
        }

        private async void Browser_Navigated(object sender, WrappedNavigatedEventArgs e)
        {
            if (_MapOnNavigate)
            {
                await _EvaluateMappedRoutesAsync(e.Uri);
            }
        }

        void Browser_NavigationFailed(object sender, Uri e)
        {
            this.CurrentUri = e;
        }

        #endregion
    }
}
