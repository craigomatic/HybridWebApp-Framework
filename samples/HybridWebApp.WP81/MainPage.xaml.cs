using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HybridWebApp.WP81.Resources;
using System.Threading.Tasks;
using HybridWebApp.Framework.Model;
using Newtonsoft.Json;
using HybridWebApp.Framework;
using HybridWebApp.Toolkit.WP8;
using HybridWebApp.Toolkit.WP8.Controls;

namespace HybridWebApp.WP81
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void WebHost_MessageReceived(HybridWebView sender, ScriptMessage msg)
        {
            switch (msg.Type)
            {
                case KnownMessageTypes.Error:
                    {
                        var errorInfo = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ErrorInfo>(msg.Payload));

#if DEBUG
                        MessageBox.Show(errorInfo.Message, "Error", MessageBoxButton.OK);
#endif

                        break;
                    }
                case KnownMessageTypes.Menu:
                    {
                        var navItems = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IList<NavItem>>(msg.Payload));

                        foreach (var item in navItems)
                        {
                            var pivotItem = new PivotItem
                            {
                                Header = item.Title,
                                Tag = item
                            };

                            Dispatcher.BeginInvoke(() => { MainPivot.Items.Add(pivotItem); });
                        }

                        break;
                    }
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var navItem = (e.AddedItems[0] as PivotItem).Tag as NavItem;
            WebHost.Navigate(new Uri(navItem.Href)); 
        }

        private void WebHost_Ready(object sender, EventArgs e)
        {
            //redirect links that contain google.com to bing.com
            WebHost.WebRoute.AddCustomRoute(new CustomRoute
            {
                Action = async (uri, success, errorCode) =>
                {
                    //if (success)
                    //{
                    //    await WebHost.Interpreter.LoadFrameworkAsync();
                    //    await WebHost.Interpreter.LoadCssAsync("app.css");
                    //    await WebHost.Interpreter.LoadAsync("app.js");

                    //    _HideNavigatingOverlay();
                    //}
                    //else
                    //{
                    //    _HideNavigatingOverlay();
                    //    _ShowOfflineOverlay();
                    //}
                },
                Evaluate = (uri) =>
                {
                    if (uri.OriginalString.Contains("google.com"))
                    {
                        return CustomRouteAction.Redirect;
                    }

                    return CustomRouteAction.None;
                },
                Redirect = new Uri("http://bing.com")
            });

            //only load the menu once
            if (MainPivot.Items.Count == 0)
            {
                WebHost.WebRoute.Map("/", async (uri, success, errorCode) =>
                {
                    await WebHost.Interpreter.EvalAsync("app.readMenu();");
                }, true);
            }
        }
    }
}