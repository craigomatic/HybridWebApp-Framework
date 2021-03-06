﻿using HybridWebApp.Framework;
using HybridWebApp.Framework.Model;
using HybridWebApp.Toolkit.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HybridWebApp.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string _ParamForNavigation;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var param = e.Parameter as string;

            if (!string.IsNullOrWhiteSpace(param))
            {
                _ParamForNavigation = param;
            }

            // If your application contains multiple pages, ensure that you are
            // handling the hardware Back button
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s,a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
                else if(WebHost.CanGoBack)
                {
                    WebHost.GoBack();
                    a.Handled = true;
                }
            };
        }        

        //event handler for messages that have been posted back from the website to the host app
        private async void WebHost_MessageReceived(HybridWebView sender, Framework.Model.ScriptMessage msg)
        {
            switch (msg.Type)
            {
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

                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { MainPivot.Items.Add(pivotItem); });
                        }

                        if (navItems.Any())
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { MainPivot.Visibility = Windows.UI.Xaml.Visibility.Visible; });
                        }

                        break;
                    }
                case KnownMessageTypes.WindowOpen:
                    {
                        //result of calls to window.open('...');
                        var navItem = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<NavItem>(msg.Payload));

                        //just passing them through exactly as the browser would anyway
                        //a more real-world example is handling Facebook login correctly via
                        //native UI in the host app vs. it opening in a new IE window which will fail
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(navItem.Href));

                        break;
                    }
                case KnownMessageTypes.Log:
                    {
                        //result of calls to framework.log('message');
                        var logMessage = msg.Payload;

                        System.Diagnostics.Debug.WriteLine(logMessage);

                        break;
                    }
                case "myCustomMessageType":
                    {
                        //var myCustomObject = JsonConvert.DeserializeObject<MyCustomObject>(msg.Payload);

                        break;
                    }
            }
        }

        private void WebHost_Ready(object sender, EventArgs e)
        {
            if (MainPivot.Items.Count == 0) //avoid duplicate mappings due to frame navigation causing this event to fire
            {
                WebHost.WebRoute.Map("/", async (uri, success, errorCode) =>
                {
                    await WebHost.Interpreter.EvalAsync("app.readMenu();");
                }, true); //true = run once
            }
            
            if (!string.IsNullOrWhiteSpace(_ParamForNavigation))
            {
                //Note: only expecting params that can be appended to the end of the root URI 

                WebHost.Navigate(new Uri(WebHost.WebUri, _ParamForNavigation));

                _ParamForNavigation = null;
            }
            else
            {
                WebHost.Navigate(WebHost.WebUri);
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

        private void WebHost_NavigationStarting(HybridWebView sender, Uri uri)
        {
            CommandBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void WebHost_DOMContentLoaded(HybridWebView sender, Uri uri)
        {
            CommandBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            //make sure the pivot and the view are in sync
            var expectedPivotSelection = MainPivot.Items.Cast<PivotItem>().Where(p => (p.Tag as NavItem).Href == uri.OriginalString).FirstOrDefault();

            if (expectedPivotSelection != null && MainPivot.SelectedItem != expectedPivotSelection)
            {
                MainPivot.SelectedItem = expectedPivotSelection;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchPage));
        }
    }
}
