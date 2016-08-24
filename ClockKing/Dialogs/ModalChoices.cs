using System;
using ClockKing.Core;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace ClockKing
{
	
	public class ModalChoices:iModalChoices
	{
		public string Title { get; set; }
		public string Instructions { get; set; }
		public IEnumerable<ModalChoice> Choices{ get; set; }
		public UIViewController Controller { get; set; }

		#region iModalChoices implementation
		public void Display ()
		{
			var ac = UIAlertController.Create(Title,Instructions,UIAlertControllerStyle.ActionSheet);
			foreach(var choice in Choices)
				ac.AddAction(UIAlertAction.Create(choice.Label,
					(choice.Destructive) ? UIAlertActionStyle.Destructive :
					(choice.Cancel) ? UIAlertActionStyle.Cancel :
					UIAlertActionStyle.Default,
                  (a) =>
				  	{
					  if (choice.Handler!=null)
							choice.Handler();
					}));
					
			Controller.PresentModalViewController (ac, true);
		}
		#endregion
	}

	public class SharedDialogs
	{
		public SharedDialogs ()
		{
		}

		public static UIAlertController ErrorDialog(string Title, string Message)
		{
			var d = UIAlertController.Create(Title, Message, UIAlertControllerStyle.Alert);
			d.AddAction(UIAlertAction.Create("ok", UIAlertActionStyle.Cancel, null));
			return d;
		}

		public static UIAlertController ConfirmationDialog(Action<UIAlertAction> handler,
			string Title="Are you sure?",
			string Message="Confirm Delete:",
			string yes="Yes",
			string no="Nevermind",
			bool YesIsDestructive=true)
		{


			var okCancelAlertController = UIAlertController.Create(Title,Message, UIAlertControllerStyle.Alert);
			okCancelAlertController.AddAction(UIAlertAction.Create (yes,(YesIsDestructive)?UIAlertActionStyle.Destructive:UIAlertActionStyle.Default,handler));
			okCancelAlertController.AddAction(UIAlertAction.Create (no, UIAlertActionStyle.Cancel, null));
			return okCancelAlertController;

		}
	}
}

