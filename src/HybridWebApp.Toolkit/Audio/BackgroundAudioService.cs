using System;
using System.Threading.Tasks;
using Windows.Media;
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

        private SystemMediaTransportControls _TransportControls;
        private MediaElement _MediaElement;
        private AudioInfo _NowPlaying;
        
        private Panel _MediaElementHost;
        private CoreDispatcher _Dispatcher;

        private bool _WasPlaying;

        public BackgroundAudioService(Panel mediaElementHost, CoreDispatcher dispatcher)
        {
            _MediaElementHost = mediaElementHost;
            _Dispatcher = dispatcher;

            _MediaElement = new MediaElement();
            _MediaElement.IsLooping = true;
            _MediaElement.AudioCategory = AudioCategory.BackgroundCapableMedia;
            _MediaElement.AutoPlay = false;
            _MediaElement.Source = new Uri("ms-appx:///HybridWebApp.Toolkit/Audio/empty.mp3");
            _MediaElement.Volume = 1;
            _MediaElementHost.Children.Add(_MediaElement);

            _TransportControls = SystemMediaTransportControls.GetForCurrentView();
            _TransportControls.IsEnabled = false;
            _TransportControls.ButtonPressed += _TransportControls_ButtonPressed;
            _TransportControls.PropertyChanged += _TransportControls_PropertyChanged;
            _TransportControls.IsPlayEnabled = true;
            _TransportControls.IsPauseEnabled = true;
            _TransportControls.IsStopEnabled = true;
            _TransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            _TransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;
        }

        public void UpdateNowPlaying(AudioInfo audioInfo)
        {
            if (audioInfo.Equals(_NowPlaying))
            {
                return;
            }

            _NowPlaying = audioInfo;

            //clear all seems to be required as otherwise the tracks don't update
            _TransportControls.DisplayUpdater.ClearAll();
            _TransportControls.IsEnabled = true;

            _TransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;

            _TransportControls.IsPlayEnabled = _NowPlaying.IsPlayEnabled;
            _TransportControls.IsPauseEnabled = _NowPlaying.IsPauseEnabled;
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

            switch (_NowPlaying.PlaybackStatus)
            {
                case AudioPlaybackStatus.Playing:
                    {
                        _MediaElement.Play();
                        break;
                    }
                case AudioPlaybackStatus.Paused:
                    {
                        _MediaElement.Pause();
                        break;
                    }
                case AudioPlaybackStatus.Stopped:
                    {
                        _MediaElement.Stop();
                        break;
                    }
            }

            _TransportControls.DisplayUpdater.Update();
        }

        #region Transport Control Event Handlers

        async void _TransportControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
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

                                await this.PauseFunc();

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

                                    await this.PlayFunc();
                                }

                                break;
                            }
                    }
                }
                catch { }
            }
        }

        async void _TransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    {
                        await _Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            _TransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                            _TransportControls.IsPauseEnabled = true;
                            await this.PlayFunc();
                        });

                        break;
                    }
                case SystemMediaTransportControlsButton.Pause:
                    {
                        await _Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            _TransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                            _TransportControls.IsPlayEnabled = true;
                            await this.PauseFunc();
                        });

                        break;
                    }
                case SystemMediaTransportControlsButton.Next:
                    {
                        await _Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await this.NextFunc();
                        });

                        break;
                    }
                case SystemMediaTransportControlsButton.Previous:
                    {
                        await _Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await this.PreviousFunc();
                        });

                        break;
                    }
            }
        }

        #endregion

        public void Dispose()
        {
            _TransportControls.ButtonPressed -= _TransportControls_ButtonPressed;
            _TransportControls.PropertyChanged -= _TransportControls_PropertyChanged;
            _MediaElementHost.Children.Remove(_MediaElement);
        }
    }
}
