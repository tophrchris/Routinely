using System;
using UIKit;
using Foundation;

namespace ClockKing
{
	//TODO: is there a iDisposable component that makes using these easier?
	public class SharedDialogs
	{
		public SharedDialogs ()
		{
		}

		public static UIAlertController ConfirmationDialog(Action<UIAlertAction> handler,
			string Title="Are you sure?",
			string Message="Confirm Delete:",
			string yes="Yes",
			string no="Nevermind",
			bool YesIsDestructive=true)
		{
			var okCancelAlertController = UIAlertController.Create(Title,Message, UIAlertControllerStyle.ActionSheet);
			okCancelAlertController.AddAction(UIAlertAction.Create (yes,(YesIsDestructive)?UIAlertActionStyle.Destructive:UIAlertActionStyle.Default,handler));
			okCancelAlertController.AddAction(UIAlertAction.Create (no, UIAlertActionStyle.Cancel, null));
			return okCancelAlertController;

		}
	}
}

