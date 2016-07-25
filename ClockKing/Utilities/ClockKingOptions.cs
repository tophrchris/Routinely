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
		public Themes Theme{ get; set; }
		public GroupingChoices GroupingChoice { get; set; }
		public bool TracingEnabled { get; set; }=false;
		public bool ShowInactiveGoals { get; set; } = true;

		public ClockKingOptions()
		{
			this.Theme = Themes.TrackBeam;
			this.GroupingChoice = GroupingChoices.ByStatus;
		}

		public void ApplyTheme()
		{
			var themer = new Dictionary<Themes,Action> ();
			//themer.Add (Themes.FitPulse,()=> FitpulseTheme.Apply ());
			themer.Add (Themes.TrackBeam,()=> ThemeManager.Register<TrackBeamTheme>().Apply());
			//themer.Add (Themes.Foody,()=> FoodyTheme.Apply ());
			//themer.Add (Themes.Prolific, () => ProlificTheme.Apply ());
			//themer.Add (Themes.Gridlocked, () => GridlockTheme.Apply ());
			//themer.Add (Themes.Industrial, () => IndustrialTheme.Apply ());

			if (themer.ContainsKey (this.Theme))
				themer [this.Theme].Invoke ();
		}
	}



	public enum Themes
	{
		FitPulse,
		TrackBeam,
		Gridlocked,
		Prolific,
		Foody,
		Industrial
	}
	public enum GroupingChoices
	{
		ByStatus,
		ByTimeOfDay
	}
}

