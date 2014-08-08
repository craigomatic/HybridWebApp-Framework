using HybridWebApp.Framework;
using HybridWebApp.Framework.Model;
using HybridWebApp.Toolkit;
using System;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;

namespace HybridWebApp.Toolkit.Controls
{
    public sealed partial class HybridWebView : UserControl
    {
        public event TypedEventHandler<HybridWebView, ScriptMessage> MessageReceived;

        public event TypedEventHandler<HybridWebView, Uri> NavigationStarting;

        public event TypedEventHandler<HybridWebView, Uri> DOMContentLoaded;

        /// <summary>
        /// Fires when the HybridWebView is ready for manipulation
        /// </summary>
        public event EventHandler Ready;
        
        public string LoadingBackgroundImage { get; set; }

        public string OfflineBackgroundImage { get; set; }

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

        private BrowserWrapper _BrowserWrapper;

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
            await _Initialise();

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
                cssString = await Windows.Storage.FileIO.ReadTextAsync(cssFile);
            }

            var jsString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.JsResourcePath))
            {
                var jsFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.JsResourcePath));
                jsString = await Windows.Storage.FileIO.ReadTextAsync(jsFile);
            }

            var additionalPhoneCssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePathPhone))
            {
                var additionalCssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePathPhone));
                additionalPhoneCssString = await Windows.Storage.FileIO.ReadTextAsync(additionalCssFile);
            }

            var additionalNotPhoneCssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePathNotPhone))
            {
                var additionalCssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePathNotPhone));
                additionalNotPhoneCssString = await Windows.Storage.FileIO.ReadTextAsync(additionalCssFile);
            }

            _WebRoute.Root = this.WebUri;

            _WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                if (success)
                {
                    await _Interpreter.LoadFrameworkAsync(WebToHostMessageChannel.IFrame, this.WebUri.OriginalString);

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
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                });
            }

            WebView.FrameNavigationStarting += async (s, args) =>
            {
                await _ProcessMessageAsync(args.Uri);

                //cancel navigation
                args.Cancel = true;
            };

            if (this.Ready != null)
            {
                this.Ready(this, EventArgs.Empty);
            }
        }

        public void Navigate(Uri uri)
        {
            if(_WebRoute.CurrentUri == uri)
            {
                return;
            }

            _BrowserWrapper.Navigate(uri);
        }

        private void _OnMessageReceived(ScriptMessage scriptMessage)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, scriptMessage);
            }
        }

        private async Task _ProcessMessageAsync(Uri uri)
        {
            if (!uri.ToString().Contains(string.Format("{0}/hwaf/", this.WebUri.OriginalString.TrimEnd('/'))) || uri.Segments.Length < 3)
            {
                return;
            }

            //process message
            var encodedMsg = uri.AbsolutePath.Replace("/hwaf/", string.Empty);
            var jsonString = System.Net.WebUtility.UrlDecode(encodedMsg).Replace("/\"", "\\\"");
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

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (this.NavigationStarting != null)
            {
                this.NavigationStarting(this, args.Uri);
            }

            _ShowNavigatingOverlay();
        }

        private void WebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (this.DOMContentLoaded != null)
            {
                this.DOMContentLoaded(this, args.Uri);
            }
        }
    }
}
