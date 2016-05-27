using Foundation;
using UIKit;
using Xamarin.Themes;
using Xamarin.Themes.Core;
using System;
using Xamarin.Themes.Core;
using Xamarin.Themes.TrackBeam;

namespace ClockKing
{
	public class ClockKingOptions
	{
		public Themes Theme{ get; set; }
		public GroupingChoices GroupingChoice { get; set; }
	
		public void ApplyTheme()
		{
			if (this.Theme == Themes.FitPulse)
				FitpulseTheme.Apply ();
			if(this.Theme==Themes.TrackBeam)
				ThemeManager.Register<TrackBeamTheme> ().Apply ();
			if(this.Theme==Themes.GunMetal)
				GunmetalTheme.Apply();
			if(this.Theme==Themes.CashFlow)
				GunmetalTheme.Apply();
		}
	}



	public enum Themes
	{
		FitPulse,
		TrackBeam,
		GunMetal,
		CashFlow
	}
	public enum GroupingChoices
	{
		ByStatus,
		ByTimeOfDay
	}
}

