using HybridWebApp.Framework;
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
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void WebHost_MessageReceived(HybridWebView sender, ScriptMessage msg)
        {
            switch (msg.Type)
            {
                case KnownMessageTypes.Menu:
                    {
                        //generate a navbar based on this
                        var navItems = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IList<NavItem>>(msg.Payload));

                        foreach (var item in navItems)
                        {
                            var button = new Button
                            {
                                Content = item.Title,
                                Style = this.FindName("BasicButtonStyle") as Style,
                                BorderBrush = new SolidColorBrush(Colors.Transparent),
                                Margin = new Thickness(0, 0, 28, 0)
                            };

                            button.Click += (s, a) => { WebHost.Navigate(new Uri(item.Href)); };

                            AppBarItemsHost.Children.Add(button);
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
            WebHost.WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                await WebHost.Interpreter.EvalAsync("app.readMenu();");
            }, true); //true = run once
        }
    }
}
