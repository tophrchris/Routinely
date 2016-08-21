using Foundation;
using UIKit;
using Xamarin.Themes;
using Xamarin.Themes.Core;
using System;
using Xamarin.Themes.TrackBeam;
using System.Collections.Generic;

namespace ClockKing
{
	public class ClockKingOptions
	{
		public static Themes Theme 
		{ get { return (Themes)GetIntPreference(themeKey); } set { SetIntPreference(themeKey, (int)value); } } 
		public static GroupingChoices GroupingChoice 
		{ get { return (GroupingChoices)GetIntPreference(groupingChoiceKey); } set { SetIntPreference(groupingChoiceKey, (int)value); } } 
		public static bool TracingEnabled
		{ get { return GetBoolPreference(tracingKey);} set { SetBoolPreference(tracingKey, value); } }
		public static bool ShowInactiveGoals 
		{ get { return GetBoolPreference(inactiveKey); } set { SetBoolPreference(inactiveKey, value); } }
		public static bool ShowExampleBrowser
		{ get { return GetBoolPreference(exampleBrowserKey); } set { SetBoolPreference(exampleBrowserKey, value); } }
		public static bool EnableDebugOptions
		{ get { return GetBoolPreference(debugKey); } set { SetBoolPreference(debugKey, value); } }
		public static bool OnlySkipMissed
		{ get { return GetBoolPreference(skipKey); } set { SetBoolPreference(skipKey, value); } }

		private static int GetIntPreference(string key)
		{
			var f = (int)NSUserDefaults.StandardUserDefaults.IntForKey(key);
			return f;
		}
		private static bool GetBoolPreference(string key)
		{
			var f = NSUserDefaults.StandardUserDefaults.BoolForKey(key);
			return f;
		}
		private static void SetIntPreference(string key, int val)
		{
			NSUserDefaults.StandardUserDefaults.SetInt(val, key);
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}
		private static void SetBoolPreference(string key, bool val)
		{
			NSUserDefaults.StandardUserDefaults.SetBool(val, key);
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}

		const string themeKey = "themeKey";
		const string groupingChoiceKey = "groupingKey";
		const string tracingKey = "tracingKey";
		const string inactiveKey = "inactiveKey";
		const string exampleBrowserKey = "exampleKey";
		const string debugKey = "debugKey";
		const string skipKey = "OnlySkipMissed";

		public static void ApplyTheme()
		{
			var themer = new Dictionary<Themes,Action> ();
			themer.Add (Themes.FitPulse,()=> FitpulseTheme.Apply ());
			themer.Add (Themes.TrackBeam,()=> ThemeManager.Register<TrackBeamTheme>().Apply());
			//themer.Add (Themes.Foody,()=> FoodyTheme.Apply ());
			//themer.Add (Themes.Prolific, () => ProlificTheme.Apply ());
			//themer.Add (Themes.Gridlocked, () => GridlockTheme.Apply ());
			//themer.Add (Themes.Industrial, () => IndustrialTheme.Apply ());

			if (themer.ContainsKey (ClockKingOptions.Theme))
				themer [ClockKingOptions.Theme].Invoke ();
		}

		public static void LoadDefaultValues()
		{
			var defs = new NSDictionary(themeKey, 0,
									groupingChoiceKey, 0,
									tracingKey, false,
									inactiveKey, true,
									exampleBrowserKey, true,
			                        skipKey,true,
			                        debugKey,false);

			NSUserDefaults.StandardUserDefaults.RegisterDefaults(defs);
		}
	}



	public enum Themes
	{
		FitPulse=0,
		TrackBeam,
		Gridlocked,
		Prolific,
		Foody,
		Industrial
	}
	public enum GroupingChoices
	{
		ByStatus=0,
		ByTimeOfDay,
		ByCategory
	}
}

