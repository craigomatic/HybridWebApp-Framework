using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework
{
    public class KnownMessageTypes
    {
        public const string Log = "log";

        public const string Notice = "notice";

        public const string Error = "error";

        public const string Menu = "menu";

        public const string Pin = "pin";

        public const string Gesture = "gesture";

        public const string WindowOpen = "window.open";
    }

    public class GestureTypes
    {
        public const string Rotate = "rotate";
        public const string Scale = "scale";
        public const string Swipe = "swipe";
    }

    public class GestureDirections
    {
        public const string Left = "left";
        public const string Right = "right";
        public const string Up = "up";
        public const string Down = "down";
        public const string Stretch = "stretch";
        public const string Pinch = "pinch";
    }

    public enum CustomRouteAction
    {
        /// <summary>
        /// No action, process as usual
        /// </summary>
        None,

        /// <summary>
        /// Cancel navigation
        /// </summary>
        Cancel,

        /// <summary>
        /// Redirect to the specified route
        /// </summary>
        Redirect,

        /// <summary>
        /// For routes that otherwise would have loaded in the browser, force them to open internal to the app
        /// </summary>
        ForceInternal
    }

    public enum WebToHostMessageChannel
    {
        /// <summary>
        /// The default messaging channel, ie: ScriptNotify event
        /// </summary>
        Default,

        /// <summary>
        /// Uses an iFrame's src atttribute as the messaging channel
        /// </summary>
        IFrame
    }

    public enum RouteTiming
    {
        /// <summary>
        /// Evaluate when the load completed event has fired
        /// </summary>
        [Obsolete("Use Navigate instead on W8.x/WP8.x or newer")]
        LoadCompleted,

        /// <summary>
        /// Evaulate when the navigated event has fired
        /// </summary>
        Navigated,

        /// <summary>
        /// Evaluate when the FrameContentLoading event has fired (useful for Single-Page Apps)
        /// </summary>
        FrameContentLoading
    }
  
    public class FrameworkConstants
    {
        public const string MessageProxyPath = "/_hwaf_/";
    }
}
