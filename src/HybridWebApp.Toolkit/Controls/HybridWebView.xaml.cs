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
using Windows.UI.Xaml.Media;
using Windows.Web.Http;
using System.Collections.Generic;
using Windows.Storage.Streams;
using Windows.ApplicationModel.DataTransfer;

namespace HybridWebApp.Toolkit.Controls
{
    public sealed partial class HybridWebView : UserControl
    {
        /// <summary>
        /// Raised when a new Script Message is received from the web page
        /// </summary>
        public event TypedEventHandler<HybridWebView, ScriptMessage> MessageReceived;

        public event TypedEventHandler<HybridWebView, Uri> NavigationStarting;

        public event TypedEventHandler<HybridWebView, Uri> DOMContentLoaded;

        public event TypedEventHandler<HybridWebView, Uri> FrameContentLoading;

        public event TypedEventHandler<HybridWebView, Uri> FrameDOMContentLoaded;

        public event TypedEventHandler<HybridWebView, Tuple<Uri, bool, int>> FrameNavigationCompleted;

        public event TypedEventHandler<HybridWebView, Uri> FrameNavigationStarting;

        public event TypedEventHandler<HybridWebView, WebViewLongRunningScriptDetectedEventArgs> LongRunningScriptDetected;

        public event TypedEventHandler<HybridWebView, WebViewPermissionRequestedEventArgs> PermissionRequested;

        /// <summary>
        /// Fires when the HybridWebView is ready for manipulation
        /// </summary>
        public event EventHandler Ready;

        public Brush LoadingBackgroundBrush { get; set; }

        public string LoadingBackgroundImage { get; set; }

        public double LoadingBackgroundImageWidth { get; set; }

        public Brush OfflineForegroundBrush { get; set; }

        public Brush OfflineBackgroundBrush { get; set; }

        public string OfflineBackgroundImage { get; set; }

        public double OfflineBackgroundImageWidth { get; set; }

        public string OfflineTitle { get; set; }

        public string OfflineSubtitle { get; set; }

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

        /// <summary>
        /// Gets or sets the base website URI the control should render.
        /// </summary>
        public Uri WebUri { get; set; }

        /// <summary>
        /// When true, the WebUri is navigated to when the control finishes loading
        /// </summary>
        public bool NavigateOnLoad { get; set; }

        /// <summary>
        /// When true, the default loading overlay is displayed during navigation requests. 
        /// </summary>
        /// <remarks>For single-page apps this should almost always be set to false as they typically provide their own loading spinners.</remarks>
        public bool EnableLoadingOverlay { get; set; }

        /// <summary>
        /// When true, the default offline overlay is displayed when navigation attempts fail due to connectivity
        /// </summary>
        public bool EnableOfflineOverlay { get; set; }

        /// <summary>
        /// When true, all requests do a host/path combination that differs from the WebUri will be opened in an external IE window
        /// </summary>
        public bool OpenOtherHostsExternal { get; set; }

        /// <summary>
        /// Gets or sets the custom user agent that will be sent out with each request (doesn't update the client-side userAgent retrieved via Javascript)
        /// </summary>
        public string UserAgent
        {
            get { return _BrowserWrapper != null ? _BrowserWrapper.UserAgent : string.Empty; }
            set
            {
                if (_BrowserWrapper != null)
                {
                    _BrowserWrapper.UserAgent = value;
                }
            }
        }

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
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                return;
            }

            this.InitializeComponent();

            this.LoadingBackgroundImageWidth = 360;
            this.OfflineBackgroundImageWidth = 360;

            _BrowserWrapper = new BrowserWrapper(WebView);
            _Interpreter = new Interpreter(_BrowserWrapper);
            _WebRoute = new WebRoute(_Interpreter, _BrowserWrapper);

            this.OpenOtherHostsExternal = true;
            this.NavigateOnLoad = true;
            this.EnableLoadingOverlay = true;
            this.EnableOfflineOverlay = true;
            this.OfflineTitle = "Navigation Failed.";
            this.OfflineSubtitle = "Check your data connection then retry.";

            OfflineOverlay.RetryAction = () => { this.HideOfflineOverlayAndRetry(); };
        }

        async void HybridWebView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_IsInitialised)
            {
                await _Initialise();
            }

            if (this.Ready != null)
            {
                this.Ready(this, EventArgs.Empty);
            }

            if (this.NavigateOnLoad && _WebRoute.Root != null)
            {
                _BrowserWrapper.Navigate(_WebRoute.Root);
            }
        }

        private async Task _Initialise()
        {
            //cache the CSS and JS 
            var cssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePath))
            {
                try
                {
                    var cssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePath));
                    cssString = await Windows.Storage.FileIO.ReadTextAsync(cssFile);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to load CSS from the given path {0}.", this.CssResourcePath));
                }
            }

            var jsString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.JsResourcePath))
            {
                try
                {
                    var jsFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.JsResourcePath));
                    jsString = await Windows.Storage.FileIO.ReadTextAsync(jsFile);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to load JS from the given path {0}.", this.JsResourcePath));
                }
            }

            var additionalPhoneCssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePathPhone))
            {
                try
                {
                    var additionalCssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePathPhone));
                    additionalPhoneCssString = await Windows.Storage.FileIO.ReadTextAsync(additionalCssFile);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to load CSS from the given path {0}.", this.CssResourcePathPhone));
                }
            }

            var additionalNotPhoneCssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePathNotPhone))
            {
                try
                {
                    var additionalCssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePathNotPhone));
                    additionalNotPhoneCssString = await Windows.Storage.FileIO.ReadTextAsync(additionalCssFile);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to load CSS from the given path {0}.", this.CssResourcePathNotPhone));
                }
            }

            _WebRoute.Root = this.WebUri;

            _WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                if (success)
                {
                    await _Interpreter.LoadFrameworkAsync(WebToHostMessageChannel.IFrame);

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

        public void Navigate(Uri uri, HttpMethod httpMethod, IList<KeyValuePair<string, string>> httpHeaders, IHttpContent httpContent = null)
        {
            if (_WebRoute.CurrentUri == uri)
            {
                return;
            }

            _BrowserWrapper.Navigate(uri, httpMethod, httpHeaders, httpContent);
        }

        public void GoBack()
        {
            WebView.GoBack();
        }

        public void GoForward()
        {
            WebView.GoForward();
        }

        public IAsyncAction ClearTemporaryWebDataAsync()
        {
            return WebView.ClearTemporaryWebDataAsync();
        }

        public void AddWebAllowedObject(System.String name, System.Object pObject)
        {
            WebView.AddWebAllowedObject(name, pObject);
        }

        public IAsyncAction CapturePreviewToStreamAsync(IRandomAccessStream stream)
        {
            return WebView.CapturePreviewToStreamAsync(stream);
        }

        public IAsyncOperation<DataPackage> CaptureSelectedContentToDataPackageAsync()
        {
            return WebView.CaptureSelectedContentToDataPackageAsync();
        }

        public WebViewDeferredPermissionRequest DeferredPermissionRequestById(System.UInt32 id)
        {
            return WebView.DeferredPermissionRequestById(id);
        }

        private void _OnMessageReceived(ScriptMessage scriptMessage)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, scriptMessage);
            }
        }

        private async Task<bool> _ProcessMessageAsync(Uri uri)
        {
            if (!uri.OriginalString.Contains(FrameworkConstants.MessageProxyPath))
            {
                return false;
            }

            if (uri.OriginalString.EndsWith(FrameworkConstants.MessageProxyPath)) //no msg, just mark this as handled
            {
                return true;
            }

            //process message
            var encodedMsg = uri.AbsolutePath.Replace(FrameworkConstants.MessageProxyPath, string.Empty);
            var jsonString = System.Net.WebUtility.UrlDecode(encodedMsg).Replace("/\"", "\\\"");
            var msg = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ScriptMessage>(jsonString));

            //send it up to be handled
            _OnMessageReceived(msg);

            return true;
        }

        #region Overlays

        private void _ShowNavigatingOverlay()
        {
            if (!this.EnableLoadingOverlay)
            {
                return;
            }

            this.ShowNavigatingOverlay();
        }

        public void ShowNavigatingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            //Browser.Opacity = 0.1d;
            WebView.Visibility = Visibility.Visible;
        }

        private void _HideNavigatingOverlay()
        {
            if (!this.EnableLoadingOverlay)
            {
                return;
            }

            this.HideNavigatingOverlay();
        }

        public void HideNavigatingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            //Browser.Opacity = 1;
            WebView.Visibility = Visibility.Visible;
        }

        private void _ShowOfflineOverlay()
        {
            if (!this.EnableOfflineOverlay)
            {
                return;
            }

            this.ShowOfflineOverlay();
        }

        public void ShowOfflineOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            OfflineOverlay.Visibility = Visibility.Visible;

            WebView.Visibility = Visibility.Collapsed;//.Opacity = 0.1d;
        }

        private void _HideOfflineOverlayAndRetry()
        {
            if (this.EnableLoadingOverlay)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
            }

            if (this.EnableOfflineOverlay)
            {
                OfflineOverlay.Visibility = Visibility.Collapsed;
            }

            _BrowserWrapper.Navigate(_WebRoute.CurrentUri);
        }

        /// <summary>
        /// Hides the offline overlay and transitions to the navigating overlay before refreshing the browser
        /// </summary>
        public void HideOfflineOverlayAndRetry()
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

        private void WebView_FrameContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            if(this.FrameContentLoading != null)
            {
                this.FrameContentLoading(this, args.Uri);
            }
        }

        private void WebView_FrameDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (this.FrameDOMContentLoaded != null)
            {
                this.FrameDOMContentLoaded(this, args.Uri);
            }
        }

        private void WebView_FrameNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (this.FrameNavigationCompleted != null)
            {
                this.FrameNavigationCompleted(this, new Tuple<Uri, bool, int>(args.Uri, args.IsSuccess, (int)args.WebErrorStatus));
            }
        }

        private async void WebView_FrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var handled = await _ProcessMessageAsync(args.Uri);

            //cancel navigation
            if (handled)
            {
                args.Cancel = true;
                return;
            }

            if (this.FrameNavigationStarting != null)
            {
                this.FrameNavigationStarting(this, args.Uri);
            }
        }

        private void WebView_LongRunningScriptDetected(WebView sender, WebViewLongRunningScriptDetectedEventArgs args)
        {
            if (this.LongRunningScriptDetected != null)
            {
                this.LongRunningScriptDetected(this, args);
            }
        }

        private void WebView_PermissionRequested(WebView sender, WebViewPermissionRequestedEventArgs args)
        {
            if (this.PermissionRequested != null)
            {
                this.PermissionRequested(this, args);
            }
        }
    }
}
