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

namespace ClockKing.RoutinelyWatchExtension
{
    [Register ("RowController")]
    partial class RowController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceLabel emojiLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceLabel GoalTitleLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (emojiLabel != null) {
                emojiLabel.Dispose ();
                emojiLabel = null;
            }

            if (GoalTitleLabel != null) {
                GoalTitleLabel.Dispose ();
                GoalTitleLabel = null;
            }
        }
    }
}