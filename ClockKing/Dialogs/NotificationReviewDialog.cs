using System;
using MonoTouch.Dialog;
using System.Linq;
using ClockKing.Extensions;
using UIKit;


namespace ClockKing
{
	public class NotificationReviewDialog:CheckPointDialog
	{

		public NotificationReviewDialog ():base()
		{
			this.Root = new RootElement("Notifications");
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();


			var msgs = Controller.Notifier
				.ScheduledNotifications
				.Select(n => new MessageElement()
				{
					Body = n.AlertBody,
					Subject = n.Category,
					Date = n.FireDate.ToDateTime().ToLocalTime(),
					Sender = n.AlertTitle,
					MessageCount = 1,
					NewFlag = false
				});

			var msglist = new Section("current");

			msglist.AddAll(msgs);
			Root.Add(msglist);
		}
	}
}

