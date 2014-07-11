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
        public List<Uri> UriHistory { get; private set; }

        public Uri CurrentUri { get; private set; }

        public Uri Root { get; private set; }

        public Interpreter Interpreter { get; private set; }

        public IBrowser Browser { get; private set; }

        private Dictionary<string, List<MappedRoute>> _MappedRoutes;

        private List<CustomRoute> _CustomRoutes;

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

            _CustomRoutes = new List<CustomRoute>();
            _MappedRoutes = new Dictionary<string, List<MappedRoute>>();
            _MapOnNavigate = mapOnNavigate;

            this.UriHistory = new List<Uri>();
        }

        /// <summary>
        /// Adds a route that when matched should be considered an internal route regardless of any internal matching rules
        /// </summary>
        /// <param name="knownRoute"></param>
        public void AddCustomRoute(CustomRoute customRoute)
        {
            _CustomRoutes.Add(customRoute);
        }

        /// <summary>
        /// Maps a route to an action that is invoked when the browser loaded completed event has been raised
        /// </summary>
        /// <param name="fragment"></param>
        /// <param name="action"></param>
        /// <param name="runOnce">When true the mapped action will only be invoked once (upon successful navigation)</param>
        public void Map(string fragment, Func<Uri, bool, int,Task> action, bool runOnce = false)
        {
            List<MappedRoute> mappedRoutes = null;

            if (!_MappedRoutes.TryGetValue(fragment, out mappedRoutes))
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

        private async Task _EvaluateMappedRoutesAsync(Uri uri, bool isSuccess, int webErrorStatus)
        {
            this.CurrentUri = uri;

            if (this.CurrentUri.Host != this.Root.Host) //ignore requests to other hosts
            {
                return;
            }

            try
            {
                //look for a custom route first
                var customRouteAction = CustomRouteAction.None;
                var customRoute = _FindCustomRoute(uri, out customRouteAction);

                if (customRoute != null)
                {
                    await customRoute.Action(uri, isSuccess, webErrorStatus);
                    return;
                }

                var mappedRoutes = _MappedRoutes.Where(r => uri.AbsolutePath.Contains(r.Key));

                foreach (var mappedRoute in mappedRoutes)
                {
                    foreach (var route in mappedRoute.Value)
                    {
                        if (!route.RunOnce || (route.RunOnce && route.Hits == 0) && isSuccess)
                        {
                            route.Hits++;
                            await route.Action(uri, isSuccess, webErrorStatus);
                        }
                    }
                }
            }
            finally
            {
                this.UriHistory.Add(uri);
            }
        }

        #region Browser event handlers

        private async void Browser_LoadCompleted(object sender, WrappedNavigatedEventArgs args)
        {
            if (!_MapOnNavigate)
            {
                await _EvaluateMappedRoutesAsync(args.Uri, args.IsSuccess, args.WebErrorStatus);
            }
        }

        private void Browser_Navigating(object sender, WrappedNavigatingEventArgs e)
        {
            var customRouteAction = CustomRouteAction.None;
            var customRoute = _FindCustomRoute(e.Uri, out customRouteAction);

            switch (customRouteAction)
            {
                case CustomRouteAction.ForceInternal:
                    {
                        return;
                    }
                case CustomRouteAction.Cancel:
                    {
                        e.Cancel = true;
                        return;
                    }
                case CustomRouteAction.Redirect:
                    {
                        e.Cancel = true;
                        this.Browser.Navigate(customRoute.Redirect);
                        return;
                    }
                case CustomRouteAction.None:
                    {
                        //do nothing, continue processing
                        break;
                    }
            }

            var hostComparison = Uri.Compare(e.Uri, this.Root, UriComponents.Host, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase);
            
            var isValidPath = e.Uri.AbsolutePath.StartsWith(this.Root.AbsolutePath);

            if (hostComparison != 0 || !isValidPath)
            {
                if (_OtherHostsAction != null)
                {
                    _OtherHostsAction(e.Uri);

                    e.Cancel = true;
                }
            }
        }

        private CustomRoute _FindCustomRoute(Uri uri, out CustomRouteAction action)
        {
            action = CustomRouteAction.None;

            //first evaluate custom routes
            foreach (var customRoute in _CustomRoutes)
            {
                action = customRoute.Evaluate(uri);

                if (action != CustomRouteAction.None) //first route found that matches with a CustomRouteAction other than None is the winner
                {
                    return customRoute;
                }
            }

            return null;
        }

        private async void Browser_Navigated(object sender, WrappedNavigatedEventArgs e)
        {
            if (_MapOnNavigate)
            {
                await _EvaluateMappedRoutesAsync(e.Uri, e.IsSuccess, e.WebErrorStatus);
            }
        }

        async void Browser_NavigationFailed(object sender, WrappedFailedEventArgs e)
        {
            await _EvaluateMappedRoutesAsync(e.Uri, false, 0);
        }

        #endregion
    }
}
