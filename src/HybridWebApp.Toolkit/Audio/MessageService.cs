using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace HybridWebApp.Toolkit.Audio
{
    public static class MessageService
    {
        // The underlying BMP methods can pass a ValueSet. MessageService
        // relies on this to pass a type and body payload.
        const string MessageType = "MessageType";
        const string MessageBody = "MessageBody";

        public static void SendMessageToForeground<T>(T message)
        {
            var payload = new ValueSet();
            payload.Add(MessageService.MessageType, typeof(T).FullName);
            payload.Add(MessageService.MessageBody, JsonConvert.SerializeObject(message));
            BackgroundMediaPlayer.SendMessageToForeground(payload);
        }

        public static void SendMessageToBackground<T>(T message)
        {
            var payload = new ValueSet();
            payload.Add(MessageService.MessageType, typeof(T).FullName);
            payload.Add(MessageService.MessageBody, JsonConvert.SerializeObject(message));
            BackgroundMediaPlayer.SendMessageToBackground(payload);
        }

        public static bool TryParseMessage<T>(ValueSet valueSet, out T message)
        {
            object messageTypeValue;
            object messageBodyValue;

            message = default(T);

            // Get message payload
            if (valueSet.TryGetValue(MessageService.MessageType, out messageTypeValue)
                && valueSet.TryGetValue(MessageService.MessageBody, out messageBodyValue))
            {
                // Validate type
                if ((string)messageTypeValue != typeof(T).FullName)
                {
                    Debug.WriteLine("Message type was {0} but expected type was {1}", (string)messageTypeValue, typeof(T).FullName);
                    return false;
                }

                message = JsonConvert.DeserializeObject<T>(messageBodyValue.ToString());
                return true;
            }

            return false;
        }
    }
}
