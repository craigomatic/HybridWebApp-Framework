﻿using HybridWebApp.Framework.Model;
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

        private Dictionary<string, Action> _MappedRoutes;

        public WebRoute(Uri rootUri, Interpreter interpreter, IBrowser browser)
        {
            this.Root = rootUri;
            this.Interpreter = interpreter;
            this.Browser = browser;
            this.Browser.LoadCompleted += Browser_LoadCompleted;

            _MappedRoutes = new Dictionary<string, Action>();
        }

        void Browser_LoadCompleted(object sender, Uri e)
        {
            this.CurrentUri = e;

            Action action = null;

            if (_MappedRoutes.TryGetValue(e.Fragment, out action))
            {
                action();
            }
        }

        public void Map(string fragment, Action action)
        {
            _MappedRoutes.Add(fragment, action);
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
    }
}
