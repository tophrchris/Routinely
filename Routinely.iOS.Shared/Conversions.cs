﻿using System;
using Foundation;
using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace ClockKing.Extensions
{
	public static class ConversionExtensions {

		#region Data

		/// <summary>The NSDate from Xamarin takes a reference point form January 1, 2001, at 12:00</summary>
		/// <remarks>
		/// It also has calls for NIX reference point 1970 but appears to be problematic
		/// </remarks>
		private static DateTime nsRef = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // last zero is milliseconds

		#endregion

		/// <summary>Returns the seconds interval for a DateTime from NSDate reference data of January 1, 2001</summary>
		/// <param name="dt">The DateTime to evaluate</param>
		/// <returns>The seconds since NSDate reference date</returns>
		public static double SecondsSinceNSRefenceDate(this DateTime dt) {
			return (dt - nsRef).TotalSeconds;
		}


		/// <summary>Convert a DateTime to NSDate</summary>
		/// <param name="dt">The DateTime to convert</param>
		/// <returns>An NSDate</returns>
		public static NSDate ToNSDate(this DateTime dt) {
			return NSDate.FromTimeIntervalSinceReferenceDate(dt.SecondsSinceNSRefenceDate());
		}


		/// <summary>Convert an NSDate to DateTime</summary>
		/// <param name="nsDate">The NSDate to convert</param>
		/// <returns>A DateTime</returns>
		public static DateTime ToDateTime(this NSDate nsDate) {
			// We loose granularity below millisecond range but that is probably ok
			return nsRef.AddSeconds(nsDate.SecondsSinceReferenceDate);
		}

		public static string AsSentence(this string passage)
		{
			var endings = new[] { ".", "!", "?", ":" };
			if (!endings.Any(e => passage.EndsWith(e, StringComparison.CurrentCulture)))
				passage += ".";
			return passage.ApplyCase(LetterCasing.Sentence);
		}
	}
}
