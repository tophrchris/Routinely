using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace ClockKing
{
	partial class NavigationController : UINavigationController
	{
		public NavigationController (IntPtr handle) : base (handle)
		{
			Console.WriteLine ("hi");
		}


		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);
		}

	}

}
