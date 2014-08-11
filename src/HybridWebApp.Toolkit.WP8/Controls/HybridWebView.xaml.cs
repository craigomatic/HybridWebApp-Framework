using HybridWebApp.Framework;
using HybridWebApp.Framework.Model;
using System;
using Windows.Foundation;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.IO;
using HybridWebApp.Toolkit.WP8;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HybridWebApp.Toolkit.WP8.Controls
{
    public sealed partial class HybridWebView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void _OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event TypedEventHandler<HybridWebView, ScriptMessage> MessageReceived;

        public event TypedEventHandler<HybridWebView, Uri> NavigationStarting;

        public event TypedEventHandler<HybridWebView, Uri> DOMContentLoaded;

        /// <summary>
        /// Fires when the HybridWebView is ready for manipulation
        /// </summary>
        public event EventHandler Ready;

        private Brush _ForegroundBrush;

        public Brush ForegroundBrush
        {
            get { return _ForegroundBrush; }
            set
            {
                if(_ForegroundBrush == value)
                {
                    return;
                }

                _ForegroundBrush = value;
                _OnPropertyChanged();
            }
        }

        public Brush BackgroundBrush { get; set; }
        
        public string LoadingBackgroundImage { get; set; }

        public double LoadingBackgroundImageWidth { get; set; }

        public string OfflineBackgroundImage { get; set; }

        public double OfflineBackgroundImageWidth { get; set; }

        /// <summary>
        /// Default CSS resource to load
        /// </summary>
        public string CssResourcePath { get; set; }

        /// <summary>
        /// Additional CSS for devices that identify as a phone
        /// </summary>
        public string CssResourcePathPhone { get; set; }
        
        /// <summary>
        /// Additional CSS for devices that don't identify as a phone
        /// </summary>
        public string CssResourcePathNotPhone { get; set; }

        /// <summary>
        /// Default JS resource to load
        /// </summary>
        public string JsResourcePath { get; set; }

        public Uri WebUri { get; set; }

        public bool NavigateOnLoad { get; set; }

        /// <summary>
        /// When true, all requests do a host/path combination that differs from the WebUri will be opened in an external IE window
        /// </summary>
        public bool OpenOtherHostsExternal { get; set; }

        private Interpreter _Interpreter;

        /// <summary>
        /// Gets the interpreter used to load JS/CSS and run arbitrary JS
        /// </summary>
        public Interpreter Interpreter
        {
            get { return _Interpreter; }
        }
        
        private WebRoute _WebRoute;

        public WebRoute WebRoute
        {
            get { return _WebRoute; }
        }

        public bool CanGoBack
        {
            get { return WebView.CanGoBack; }
        }

        public bool CanGoForward
        {
            get { return WebView.CanGoForward; }
        }        

        private BrowserWrapper _BrowserWrapper;

        private bool _IsInitialised;

        public HybridWebView()
        {
            this.InitializeComponent();

            _BrowserWrapper = new BrowserWrapper(WebView);
            _Interpreter = new Interpreter(_BrowserWrapper);
            _WebRoute = new WebRoute(_Interpreter, _BrowserWrapper);

            this.OpenOtherHostsExternal = true;
            this.NavigateOnLoad = true;

            //always just try to load the current URI during retries
            OfflineOverlay.RetryAction = () => { _BrowserWrapper.Navigate(_WebRoute.CurrentUri); };
        }

        async void HybridWebView_Loaded(object sender, RoutedEventArgs e)
        {
            //bindings aren't working as expected, manually setting it for now
            LoadingOverlay.ForegroundBrush = this.ForegroundBrush;
            LoadingOverlay.BackgroundBrush = this.BackgroundBrush;
            LoadingOverlay.BackgroundImage = this.LoadingBackgroundImage;
            LoadingOverlay.BackgroundImageWidth = this.LoadingBackgroundImageWidth;

            OfflineOverlay.ForegroundBrush = this.ForegroundBrush;
            OfflineOverlay.BackgroundBrush = this.BackgroundBrush;
            OfflineOverlay.BackgroundImageWidth = this.OfflineBackgroundImageWidth;
            OfflineOverlay.BackgroundImage = this.OfflineBackgroundImage;

            if (!_IsInitialised)
            {
                await _Initialise();
            }

            if (this.Ready != null)
            {
                this.Ready(this, EventArgs.Empty);
            }

            if (this.NavigateOnLoad)
            {
                WebView.Navigate(_WebRoute.Root);
            }
        }

        private async Task _Initialise()
        {
            //cache the CSS and JS 
            var cssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePath))
            {
                var cssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePath));
                cssString = new StreamReader((await cssFile.OpenReadAsync()).AsStream()).ReadToEnd();
            }

            var jsString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.JsResourcePath))
            {
                var jsFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.JsResourcePath));
                jsString = new StreamReader((await jsFile.OpenReadAsync()).AsStream()).ReadToEnd();
            }

            var additionalPhoneCssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePathPhone))
            {
                var additionalCssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePathPhone));
                additionalPhoneCssString = new StreamReader((await additionalCssFile.OpenReadAsync()).AsStream()).ReadToEnd();
            }

            var additionalNotPhoneCssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePathNotPhone))
            {
                var additionalCssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePathNotPhone));
                additionalNotPhoneCssString = new StreamReader((await additionalCssFile.OpenReadAsync()).AsStream()).ReadToEnd();
            }

            _WebRoute.Root = this.WebUri;

            _WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                if (success)
                {
                    await _Interpreter.LoadFrameworkAsync(WebToHostMessageChannel.Default, this.WebUri.OriginalString);

                    //load JS
                    if (!string.IsNullOrWhiteSpace(jsString))
                    {
                        await _Interpreter.LoadAsync(jsString);
                    }

                    //load CSS
                    if (!string.IsNullOrWhiteSpace(cssString))
                    {
                        await _Interpreter.LoadCssAsync(cssString);
                    }

                    if (!string.IsNullOrWhiteSpace(additionalPhoneCssString))
                    {
                        await _Interpreter.LoadCssAsync(additionalPhoneCssString);
                    }

                    if (!string.IsNullOrWhiteSpace(additionalNotPhoneCssString))
                    {
                        await _Interpreter.LoadCssAsync(additionalNotPhoneCssString);
                    }

                    _HideNavigatingOverlay();
                }
                else
                {
                    _HideNavigatingOverlay();
                    _ShowOfflineOverlay();
                }
            });

            if (this.OpenOtherHostsExternal)
            {
                _WebRoute.MapOtherHosts(async uri =>
                {
                    _HideNavigatingOverlay();
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                });
            }

            WebView.ScriptNotify += async (s, args) =>
            {
                await _ProcessMessageAsync(args.Value);
            };

            _IsInitialised = true;
        }

        public void Navigate(Uri uri)
        {
            if(_WebRoute.CurrentUri == uri)
            {
                return;
            }

            _BrowserWrapper.Navigate(uri);
        }

        public void GoBack()
        {
            WebView.GoBack();
        }

        public void GoForward()
        {
            WebView.GoForward();
        }

        private void _OnMessageReceived(ScriptMessage scriptMessage)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, scriptMessage);
            }
        }

        private async Task _ProcessMessageAsync(string jsonString)
        {
            var msg = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ScriptMessage>(jsonString));

            //send it up to be handled
            _OnMessageReceived(msg);
        }

        #region Overlays

        private void _ShowNavigatingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            //Browser.Opacity = 0.1d;
            WebView.Visibility = Visibility.Visible;
        }

        private void _HideNavigatingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            //Browser.Opacity = 1;
            WebView.Visibility = Visibility.Visible;
        }

        private void _ShowOfflineOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            OfflineOverlay.Visibility = Visibility.Visible;

            WebView.Visibility = Visibility.Collapsed;//.Opacity = 0.1d;
        }

        /// <summary>
        /// Hides the offline overlay and transitions to the navigating overlay before refreshing the browser
        /// </summary>
        private void _HideOfflineOverlayAndRetry()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            _BrowserWrapper.Navigate(_WebRoute.CurrentUri);
        }

        #endregion

        private void WebView_Navigating(object sender, Microsoft.Phone.Controls.NavigatingEventArgs e)
        {
            if (this.NavigationStarting != null)
            {
                this.NavigationStarting(this, e.Uri);
            }

            _ShowNavigatingOverlay();
        }

        private void WebView_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.DOMContentLoaded != null)
            {
                this.DOMContentLoaded(this, e.Uri);
            }
        }
    }
}
