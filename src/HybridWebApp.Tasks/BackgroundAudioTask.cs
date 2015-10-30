using HybridWebApp.Toolkit;
using HybridWebApp.Toolkit.Audio;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Playback;
using System;
using Windows.Storage.Streams;

namespace HybridWebApp.Tasks
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private SystemMediaTransportControls _TransportControls;
        private BackgroundTaskDeferral _BackgroundTaskDeferral;
        private AppState _AppState;
        private ManualResetEvent _ResetEvent;
        private AudioInfo _NowPlaying;
        private bool _WasPlaying;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background Audio Task starting...");

            _AppState = AppState.Unknown;

            _TransportControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            _TransportControls.ButtonPressed += _SystemMediaTransportControls_ButtonPressed;
            _TransportControls.PropertyChanged += _SystemMediaTransportControls_PropertyChanged;
            _TransportControls.IsEnabled = true;
            _TransportControls.IsPauseEnabled = true;
            _TransportControls.IsPlayEnabled = true;
            _TransportControls.IsNextEnabled = true;
            _TransportControls.IsPreviousEnabled = true;

            //TODO: get state of foreground app, should be active/suspended. When suspended this task is responsible for audio management

            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            //TODO: notify foreground app that the bg task has been started (if it's not suspended)

            _BackgroundTaskDeferral = taskInstance.GetDeferral();
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            AudioInfo audioInfo = null;

            if (MessageService.TryParseMessage<AudioInfo>(e.Data, out audioInfo))
            {
                _UpdateNowPlaying(audioInfo);
            }
        }

        private void _UpdateNowPlaying(AudioInfo audioInfo)
        {
            //clear all seems to be required as otherwise the tracks don't update
            _TransportControls.DisplayUpdater.ClearAll();
            _TransportControls.IsEnabled = true;

            _TransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;

            _TransportControls.IsPlayEnabled = _NowPlaying.PlaybackStatus == AudioPlaybackStatus.Paused || _NowPlaying.PlaybackStatus == AudioPlaybackStatus.Stopped;
            _TransportControls.IsPauseEnabled = _NowPlaying.PlaybackStatus == AudioPlaybackStatus.Playing;
            _TransportControls.IsNextEnabled = _NowPlaying.IsNextEnabled;
            _TransportControls.IsPreviousEnabled = _NowPlaying.IsPreviousEnabled;

            if (!string.IsNullOrWhiteSpace(_NowPlaying.Artist))
            {
                _TransportControls.DisplayUpdater.MusicProperties.AlbumArtist = _NowPlaying.Artist;
                _TransportControls.DisplayUpdater.MusicProperties.Artist = _NowPlaying.Artist;
                _TransportControls.DisplayUpdater.MusicProperties.Title = _NowPlaying.Title;
            }

            _TransportControls.PlaybackStatus = (MediaPlaybackStatus)_NowPlaying.PlaybackStatus;

            if (!string.IsNullOrWhiteSpace(_NowPlaying.ImageUri))
            {
                try
                {
                    var streamRef = RandomAccessStreamReference.CreateFromUri(new Uri(_NowPlaying.ImageUri));
                    _TransportControls.DisplayUpdater.Thumbnail = streamRef;
                }
                catch { }
            }

            _TransportControls.DisplayUpdater.Update();
        }

        private void _SystemMediaTransportControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            if (_NowPlaying == null)
            {
                return;
            }

            //this handles the scenario where the app goes into the background and another app is playing audio in the foreground, in addition to the scenario where it returns to the foreground
            if (args.Property == SystemMediaTransportControlsProperty.SoundLevel)
            {
                try
                {
                    switch (_TransportControls.SoundLevel)
                    {
                        case SoundLevel.Muted:
                            {
                                _WasPlaying = _NowPlaying.PlaybackStatus == AudioPlaybackStatus.Playing ? true : false;

                                //TODO: Post pause message to foreground app?
                                //await this.PauseFunc();

                                break;
                            }
                        case SoundLevel.Low:
                            {

                                break;
                            }
                        case SoundLevel.Full:
                            {
                                if (_WasPlaying)
                                {
                                    _WasPlaying = false;

                                    //TODO: Post play message to foreground app
                                    //await this.PlayFunc();
                                }

                                break;
                            }
                    }
                }
                catch { }
            }
        }

        private void _SystemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    {
                        _TransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                        _TransportControls.IsPauseEnabled = true;

                        //TODO: Post play message to foreground app
                        //await _Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        //{
                        //    await this.PlayFunc();
                        //});

                        break;
                    }
                case SystemMediaTransportControlsButton.Pause:
                    {
                        _TransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                        _TransportControls.IsPlayEnabled = true;

                        //TODO: Post pause message to foreground app
                        //await _Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        //{
                        //    await this.PauseFunc();
                        //});

                        break;
                    }
                case SystemMediaTransportControlsButton.Next:
                    {
                        //TODO: Post next message to foreground app
                        //await _Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        //{
                        //    await this.NextFunc();
                        //});

                        break;
                    }
                case SystemMediaTransportControlsButton.Previous:
                    {
                        //TODO: Post previous message to foreground app
                        //await _Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        //{
                        //    await this.PreviousFunc();
                        //});

                        break;
                    }
            }
        }
    }
}
