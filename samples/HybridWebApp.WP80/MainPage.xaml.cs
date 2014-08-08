using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Controls.Primitives;
using HybridWebApp.Toolkit.WP8;
using HybridWebApp.Toolkit.WP8.Views;
using HybridWebApp.Framework;
using System.Reflection;
using Windows.ApplicationModel;
using HybridWebApp.Framework.Model;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HybridWebApp.WP80
{
    public partial class MainPage : PhoneApplicationPage
    {
        private BrowserWrapper _BrowserWrapper;
        private Interpreter _Interpreter;
        private WebRoute _WebRoute;

        public MainPage()
        {
            InitializeComponent();

            Browser.Width = Application.Current.Host.Content.ActualWidth;

            _BrowserWrapper = new BrowserWrapper(Browser, null);

            _Interpreter = new Interpreter(_BrowserWrapper);

            _WebRoute = new WebRoute(_Interpreter, _BrowserWrapper);
            _WebRoute.Root = new Uri("http://hybridwebapp.azurewebsites.net/");

            //links to other hosts should be handled by the native browser
            _WebRoute.MapOtherHosts(a =>
            {
                _HideNavigatingOverlay();

                var webBrowserTask = new Microsoft.Phone.Tasks.WebBrowserTask();
                webBrowserTask.Uri = a;
                webBrowserTask.Show();
            });

            _WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                if (success)
                {
                    await _Interpreter.LoadFrameworkAsync();
                    await _Interpreter.LoadCssAsync("app.css");
                    await _Interpreter.LoadAsync("app.js");

                    _HideNavigatingOverlay();
                }
                else
                {
                    _HideNavigatingOverlay();
                    _ShowOfflineOverlay();
                }
            });

            //redirect links that contain google.com to bing.com
            _WebRoute.AddCustomRoute(new CustomRoute
            {
                Action = async (uri, success, errorCode) =>
                {
                    if (success)
                    {
                        await _Interpreter.LoadFrameworkAsync();
                        await _Interpreter.LoadCssAsync("app.css");
                        await _Interpreter.LoadAsync("app.js");

                        _HideNavigatingOverlay();
                    }
                    else
                    {
                        _HideNavigatingOverlay();
                        _ShowOfflineOverlay();
                    }
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
            _WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                await _Interpreter.EvalAsync("app.readMenu();");
            }, true);

            Browser.IsScriptEnabled = true;
            Browser.ScriptNotify += Browser_ScriptNotify;

            OfflineOverlay.RetryAction = () => { _HideOfflineOverlayAndRetry(); };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                _BrowserWrapper.Navigate(_WebRoute.Root);
            }
        }

        async void Browser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var msg = JsonConvert.DeserializeObject<ScriptMessage>(e.Value);

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
        
        #region Overlays

        private void _ShowNavigatingOverlay()
        {
            LoadingOverlay.Visibility = System.Windows.Visibility.Visible;
            OfflineOverlay.Visibility = System.Windows.Visibility.Collapsed;

            //Browser.Opacity = 0.1d;
            Browser.Visibility = System.Windows.Visibility.Visible;
        }

        private void _HideNavigatingOverlay()
        {
            LoadingOverlay.Visibility = System.Windows.Visibility.Collapsed;
            OfflineOverlay.Visibility = System.Windows.Visibility.Collapsed;

            //Browser.Opacity = 1;
            Browser.Visibility = System.Windows.Visibility.Visible;
        }

        private void _ShowOfflineOverlay()
        {
            LoadingOverlay.Visibility = System.Windows.Visibility.Collapsed;
            OfflineOverlay.Visibility = System.Windows.Visibility.Visible;

            Browser.Visibility = System.Windows.Visibility.Collapsed;//.Opacity = 0.1d;
        }

        /// <summary>
        /// Hides the offline overlay and transitions to the navigating overlay before refreshing the browser
        /// </summary>
        private void _HideOfflineOverlayAndRetry()
        {
            LoadingOverlay.Visibility = System.Windows.Visibility.Visible;
            OfflineOverlay.Visibility = System.Windows.Visibility.Collapsed;

            _BrowserWrapper.Navigate(_WebRoute.CurrentUri);
        }

        #endregion

        private void Browser_Navigating(object sender, NavigatingEventArgs e)
        {
            _ShowNavigatingOverlay();
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0)
            {
                return;
            }

            var navItem = (e.AddedItems[0] as PivotItem).Tag as NavItem;

            if (_WebRoute.CurrentUri.OriginalString != navItem.Href)
            {
                _BrowserWrapper.Navigate(new Uri(navItem.Href));
            }
        }
    }
}
