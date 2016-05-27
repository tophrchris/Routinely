using System;
using MonoTouch.Dialog;
using System.Linq;
using ClockKing.Extensions;
using UIKit;


namespace ClockKing
{
	public class NotificationReviewDialog:DialogViewController
	{

		public NotificationReviewDialog (CheckPointController controller,RootElement root,bool pushing=true):base(root,pushing)
		{
			var msgs = controller.Notifier
				.ScheduledNotifications
				.Select (n=>new MessageElement () {
					Body=n.AlertBody,
					Subject=n.Category,
					Date=n.FireDate.ToDateTime().ToLocalTime(),
					Sender=n.AlertTitle,
					MessageCount=1,
					NewFlag=false
			});

			var msglist = new Section ("notifications");

			msglist.AddAll (msgs);
			root.Add (msglist);

			this.NavigationItem.SetRightBarButtonItem (
				new UIBarButtonItem (UIBarButtonSystemItem.Cancel,(s,e)=>
					controller.NavigationController.PopViewController(true)
				),true);


		}

	}
}

