using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HybridWebApp.Windows10
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

        private void WebHost_Ready(object sender, EventArgs e)
        {
            WebHost.HideNavigatingOverlay();
        }

        private void WebHost_MessageReceived(Toolkit.Controls.HybridWebView sender, Framework.Model.ScriptMessage args)
        {

        }

        private async void WebHost_PermissionRequested(Toolkit.Controls.HybridWebView sender, WebViewPermissionRequestedEventArgs args)
        {
            //Note: on first run the OS will show a permission dialog of it's own related to location.
            //Developers may want to prompt the user each time, or just allow the sysetm managed dialogs to take care of it.


            //var dialog = new MessageDialog(args.PermissionRequest.PermissionType.ToString(), "Permission Request");
            //dialog.Commands.Add(new UICommand("Allow", a => { args.PermissionRequest.Allow(); }));
            //dialog.Commands.Add(new UICommand("Deny", d => { args.PermissionRequest.Deny(); }));

            //await dialog.ShowAsync();
        }

        private void BackgroundAudio_Click(object sender, RoutedEventArgs e)
        {
            WebHost.Navigate(new Uri("http://hybridwebapp.azurewebsites.net/audio-demo/"));
        }

        private void Geolocation_Click(object sender, RoutedEventArgs e)
        {
            WebHost.Navigate(new Uri("http://hybridwebapp.azurewebsites.net/geolocation-demo/"));
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            this.ShellSplitView.IsPaneOpen = !this.ShellSplitView.IsPaneOpen;
        }

        private void DontCheck(object sender, RoutedEventArgs e)
        {
            (sender as RadioButton).IsChecked = false;
        }
    }
}
