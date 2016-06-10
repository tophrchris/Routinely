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
		public OccurrencesSection (CheckPoint checkpoint,CheckPointController Controller,CheckPointDetailDialog dialog)
		{

			this.Caption= "Occurrence History:";

			var occurenceElements = 
				checkpoint
					.Occurrences
					.OrderByDescending (o => o.TimeStamp)
					.Select (o => new StringElement (o.Date.ToString ("d"),
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
							Value= "{0} - {1}".FormatWith(o.TimeStamp.ToString ("t"),o.MinutesFromTarget)
						})
					.ToList();
			this.AddAll (occurenceElements.Take(5));

			if (occurenceElements.Count > 5)
				this.Add (new StringElement ("More",
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
						dialog.moreDialog = new DialogViewController(r);
						dialog.moreDialog.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(UIBarButtonSystemItem.Cancel,
							(so,e)=>dialog.NavigationController.PopViewController(true)),true);
						dialog.NavigationController.PushViewController(dialog.moreDialog,true);
					}
				){ Alignment = UITextAlignment.Center });
		}
	}
}

