using System;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace HybridWebApp.Toolkit.Audio
{
    public class BackgroundAudioService : IDisposable
    {
        public Func<Task> PlayFunc { get; set; }

        public Func<Task> PauseFunc { get; set; }

        public Func<Task> NextFunc { get; set; }

        public Func<Task> PreviousFunc { get; set; }

        internal MediaPlayer MediaPlayer 
        {
            get
            {
                MediaPlayer instance = null;

                try
                {
                    instance = BackgroundMediaPlayer.Current;
                }
                catch (Exception e)
                {
                    if (e.HResult == TaskExceptions.RPC_S_SERVER_UNAVAILABLE)
                    {
                        BackgroundMediaPlayer.Shutdown();
                    }
                }

                if (instance == null)
                {
                    //TODO: handle
                }

                return instance;
            }
        }

        private AudioInfo _NowPlaying;
        
        private CoreDispatcher _Dispatcher;

        private bool _WasPlaying;

        public BackgroundAudioService(CoreDispatcher dispatcher)
        {
            _Dispatcher = dispatcher;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception e)
            {
                if (e.HResult == TaskExceptions.RPC_S_SERVER_UNAVAILABLE)
                {
                    //TODO: Handle
                }
            }

            this.MediaPlayer.IsLoopingEnabled = true;
            //this.MediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            this.MediaPlayer.AutoPlay = false;
            this.MediaPlayer.SetUriSource(new Uri("ms-appx:///HybridWebApp.Toolkit/Audio/empty.mp3"));
            this.MediaPlayer.Volume = 1;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            
        }

        public void UpdateNowPlaying(AudioInfo audioInfo)
        {
            if (audioInfo.Equals(_NowPlaying))
            {
                return;
            }

            _NowPlaying = audioInfo;

            switch (_NowPlaying.PlaybackStatus)
            {
                case AudioPlaybackStatus.Playing:
                    {
                        this.MediaPlayer.Play();
                        break;
                    }
                case AudioPlaybackStatus.Paused:
                    {
                        this.MediaPlayer.Pause();
                        break;
                    }
                case AudioPlaybackStatus.Stopped:
                    {
                        //TODO: Stop method is gone, what should happen instead?
                        //this.MediaPlayer.Stop();
                        break;
                    }
            }

            MessageService.SendMessageToBackground(audioInfo);
        }

        public void Dispose()
        {
            BackgroundMediaPlayer.Shutdown();
        }
    }
}
