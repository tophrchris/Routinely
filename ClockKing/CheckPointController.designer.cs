// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace ClockKing
{
	[Register ("CheckPointTableViewController")]
	partial class CheckPointController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView CheckPointTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (CheckPointTableView != null) {
				CheckPointTableView.Dispose ();
				CheckPointTableView = null;
			}
		}
	}
}
