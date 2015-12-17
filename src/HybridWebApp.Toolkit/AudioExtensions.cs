using HybridWebApp.Toolkit.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage.Streams;

namespace HybridWebApp.Toolkit
{
    public static class AudioExtensions
    {
        public static void UpdateFor(this SystemMediaTransportControls transportControls, AudioInfo audioInfo)
        {
            if (audioInfo == null)
            {
                transportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                transportControls.DisplayUpdater.MusicProperties.Title = string.Empty;
                transportControls.DisplayUpdater.Update();
                return;
            }

            transportControls.PlaybackStatus = _ResolveStatus(audioInfo.PlaybackStatus);
            transportControls.DisplayUpdater.Type = MediaPlaybackType.Music;

            transportControls.IsPlayEnabled = audioInfo.PlaybackStatus == AudioPlaybackStatus.Paused || audioInfo.PlaybackStatus == AudioPlaybackStatus.Stopped;
            transportControls.IsPauseEnabled = audioInfo.PlaybackStatus == AudioPlaybackStatus.Playing;
            transportControls.IsNextEnabled = audioInfo.IsNextEnabled;
            transportControls.IsPreviousEnabled = audioInfo.IsPreviousEnabled;

            if (!string.IsNullOrWhiteSpace(audioInfo.Artist))
            {
                transportControls.DisplayUpdater.MusicProperties.AlbumArtist = audioInfo.Artist;
                transportControls.DisplayUpdater.MusicProperties.Artist = audioInfo.Artist;
                transportControls.DisplayUpdater.MusicProperties.Title = audioInfo.Title;
            }

            if (!string.IsNullOrWhiteSpace(audioInfo.ImageUri))
            {
                try
                {
                    var streamRef = RandomAccessStreamReference.CreateFromUri(new Uri(audioInfo.ImageUri));
                    transportControls.DisplayUpdater.Thumbnail = streamRef;
                }
                catch { }
            }
            else
            {
                transportControls.DisplayUpdater.Thumbnail = null;
            }

            transportControls.DisplayUpdater.Update();
        }

        private static MediaPlaybackStatus _ResolveStatus(AudioPlaybackStatus playbackStatus)
        {
            switch (playbackStatus)
            {
                case AudioPlaybackStatus.Changing:
                    return MediaPlaybackStatus.Changing;
                case AudioPlaybackStatus.Closed:
                    return MediaPlaybackStatus.Closed;
                case AudioPlaybackStatus.Paused:
                    return MediaPlaybackStatus.Paused;
                case AudioPlaybackStatus.Playing:
                    return MediaPlaybackStatus.Playing;
                case AudioPlaybackStatus.Stopped:
                    return MediaPlaybackStatus.Stopped;
            }

            throw new NotImplementedException();
        }
    }
}
