using HybridWebApp.Framework;
using HybridWebApp.Toolkit.Audio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BackgroundAudio
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BackgroundAudioService _BackgroundAudioService;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            _BackgroundAudioService = new BackgroundAudioService(LayoutRoot, Dispatcher);
            
            //setup the play/pause functions
            _BackgroundAudioService.PlayFunc = new Func<Task>(async () => await WebHost.Interpreter.EvalAsync("app.playTrack();"));
            _BackgroundAudioService.PauseFunc = new Func<Task>(async () => await  WebHost.Interpreter.EvalAsync("app.pauseTrack();"));
        }

        private async void HybridWebView_MessageReceived(HybridWebApp.Toolkit.Controls.HybridWebView sender, HybridWebApp.Framework.Model.ScriptMessage args)
        {
            switch(args.Type)
            {
                case AppMessageTypes.TrackInfo:
                    {
                        var audioInfo = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<AudioInfo>(args.Payload));
                        
                        //hardcode the pause/play/next/previous states
                        audioInfo.IsPauseEnabled = true;
                        audioInfo.IsPlayEnabled = false;
                        audioInfo.IsNextEnabled = false;
                        audioInfo.IsPreviousEnabled = false;
                        audioInfo.PlaybackStatus = HybridWebApp.Toolkit.AudioPlaybackStatus.Playing;

                        _BackgroundAudioService.UpdateNowPlaying(audioInfo);

                        break;
                    }
            }
        }

        private void HybridWebView_Ready(object sender, EventArgs e)
        {
            WebHost.WebRoute.Map("/", async(uri, success, errorCode) =>
            {
                if (success)
                {
                    await WebHost.Interpreter.EvalAsync("app.postTrackInfo();");
                }
            }, true, RouteTiming.Navigated);

            WebHost.Navigate(new Uri("http://hybridwebapp.azurewebsites.net/audio-demo/"));
        }
    }
}
