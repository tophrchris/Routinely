using System;
using System.Collections.Generic;
using Humanizer;



namespace EmojiSharp
{
   

    public partial class Emoji
    {
        public static Emoji NowClock 
        {
            get {
                return Emoji.ClockFor (DateTime.Now.ToLocalTime ());
            }

        }

        public static Emoji ClockFor (DateTime at) 
        {
            var now = at;
            var hour = now.Hour;
            if (hour > 12)
                hour -= 12;
            if (hour == 0) 
                hour = 12;
            var past30 = (now.Minute >= 30) ? "30" : "";
            var emojiString = "clock{0}{1}".FormatWith (hour, past30);
            return Emoji.All [emojiString];
        }
    }
}

