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
}
