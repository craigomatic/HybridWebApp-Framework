using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Toolkit
{
    public enum AudioPlaybackStatus
    {
        // Summary:
        //     The media is closd.
        Closed = 0,
        //
        // Summary:
        //     The media is changing.
        Changing = 1,
        //
        // Summary:
        //     The media is stopped.
        Stopped = 2,
        //
        // Summary:
        //     The media is playing.
        Playing = 3,
        //
        // Summary:
        //     The media is paused.
        Paused = 4,
    }
}
