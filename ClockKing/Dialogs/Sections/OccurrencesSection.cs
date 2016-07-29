using System;
using MonoTouch.Dialog;
using ClockKing.Core;
using System.Linq;
using UIKit;
using Humanizer;

namespace ClockKing
{
	public class OccurrencesSection:Section
	{
		public OccurrencesSection (CheckPoint checkpoint,iCheckpointCommandController Controller,CheckPointDetailDialog dialog)
		{

			this.Caption= "Occurrences:";
			this.Footer = "Tap to remove.";

			var occurenceElements = 
				checkpoint
					.AllOccurrences
					.OrderByDescending (o => o.TimeStamp)
					.Select (o => new StringElement (o.Date.ToString ("d")  + (o.IsSkipped?" (Skipped)":""),
						()=>{
							var c = SharedDialogs.ConfirmationDialog(
								(a)=>
								{
									checkpoint.RemoveOccurrence(o);
									Controller.RewriteOccurrences();
									dialog.Render();
								},Message:"Deleting this occurrence will affect averages and streaks.");
							dialog.PresentModalViewController(c,true);
						})
						{
							Value= o.TimeStamp.ToString ("t")
						})
					.ToList();
			this.AddAll (occurenceElements.Take(5));

			if (occurenceElements.Count > 5)
				this.Add (new StringElement ("All Occurrences",
					()=>{
						var r = new RootElement(checkpoint.Name);
						var s = new Section("All Occurrences");
						r.Add(s);
						s.AddAll(
							occurenceElements.Select(o=>
								{
									o.Tapped+=()=>
									{r.Reload(s,UITableViewRowAnimation.Automatic);};
									return o;
								}
							));
						dialog.moreDialog = new DialogViewController(r,true);
						dialog.NavigationController.PushViewController(dialog.moreDialog,true);
					}
				){ Alignment = UITextAlignment.Center });
		}
	}
}

