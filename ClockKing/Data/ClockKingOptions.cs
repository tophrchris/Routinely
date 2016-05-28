using Foundation;
using UIKit;
using Xamarin.Themes;
using Xamarin.Themes.Core;
using System;
using Xamarin.Themes.Core;
using Xamarin.Themes.TrackBeam;
using System.Collections.Generic;

namespace ClockKing
{
	public class ClockKingOptions
	{
		public Themes Theme{ get; set; }
		public GroupingChoices GroupingChoice { get; set; }
	
		public ClockKingOptions()
		{
			this.Theme = Themes.Foody;
			this.GroupingChoice = GroupingChoices.ByStatus;
		}

		public void ApplyTheme()
		{
			var themer = new Dictionary<Themes,Action> ();
			themer.Add (Themes.FitPulse,()=> FitpulseTheme.Apply ());
			themer.Add (Themes.TrackBeam,()=> ThemeManager.Register<TrackBeamTheme>().Apply());
			themer.Add (Themes.GunMetal,()=> GunmetalTheme.Apply ());
			themer.Add (Themes.CashFlow,()=> CashflowTheme.Apply ());
			themer.Add (Themes.Foody,()=> FoodyTheme.Apply ());
			themer.Add (Themes.Mapper,()=> MapperTheme.Apply ());

			if (themer.ContainsKey (this.Theme))
				themer [this.Theme].Invoke ();
		}
	}



	public enum Themes
	{
		FitPulse,
		TrackBeam,
		GunMetal,
		CashFlow,
		Foody,
		Mapper
	}
	public enum GroupingChoices
	{
		ByStatus,
		ByTimeOfDay
	}
}

