using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using MonoTouch.Dialog;

namespace ClockKing
{
	partial class AddNewCheckPointController : DialogViewController
	{
		public CheckPointTableViewController Parent { get; set; }

		public AddNewCheckPointController (IntPtr handle) : base (handle)
		{

		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

						var h = new Section ("header");
						var n = new EntryElement ("name","placeholder","?");
						var t = new TimeElement ("target", DateTime.Now);
						var s = new StringElement ("save", 
								() =>{ 
					this.Parent.AddNewCheckPoint(n.Value,t.DateValue.TimeOfDay);
									this.DismissModalViewController(true);
								})
								{Alignment=UITextAlignment.Right};
			
						h.AddAll( new Element[]{ n, t,s});

			this.Root.Add (h);



			this.NavigationItem.SetLeftBarButtonItem (
				new UIBarButtonItem ("cancel", UIBarButtonItemStyle.Done, 
					(so,e)=>this.PerformSegue("Return",this)), true);
			
				
			this.NavigationItem.SetRightBarButtonItem (
				new UIBarButtonItem ("save",UIBarButtonItemStyle.Plain,null), true);
		}
	}
}
