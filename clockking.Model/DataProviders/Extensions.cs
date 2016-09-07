using System;
using ClockKing.Core;


namespace ClockKing.Extensions
{
    public static class TimeSpanExtensions 
    {
        public static string ToAMPMString(this TimeSpan ts)
        {
            return (DateTime.Today.Date + ts).ToString ("t");
        }
    }
    public static class CheckPointExtensions
    {
        public static string AsWriteable (this Occurrence toSave)
        {
            return string.Format ("{0}|{1}|{2}", toSave.checkpointLabel, toSave.TimeStamp, toSave.IsSkipped);
        }
    }
}
