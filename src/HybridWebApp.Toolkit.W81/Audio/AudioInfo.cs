using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Toolkit.Audio
{
    public class AudioInfo
    {
        public string Album { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string ImageUri { get; set; }

        public bool IsPauseEnabled { get; set; }

        public bool IsPlayEnabled { get; set; }

        public bool IsNextEnabled { get; set; }

        public bool IsPreviousEnabled { get; set; }

        public AudioPlaybackStatus PlaybackStatus { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = (AudioInfo)obj;

            return this.PlaybackStatus == other.PlaybackStatus &&
                this.Artist == other.Artist &&
                this.Album == other.Album &&
                this.ImageUri == other.ImageUri &&
                this.Title == other.Title;
        }
    }
}
