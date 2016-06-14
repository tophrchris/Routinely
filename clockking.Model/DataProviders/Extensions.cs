using System;


namespace ClockKing.Extensions
{
    public static class TimeSpanExtensions 
    {
        public static string ToAMPMString(this TimeSpan ts)
        {
            return (DateTime.Today.Date + ts).ToString ("t");
        }
    }
}
