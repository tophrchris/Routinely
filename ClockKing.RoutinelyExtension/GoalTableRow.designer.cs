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

namespace ClockKing.RoutinelyExtension
{
    [Register ("GoalTableRow")]
    partial class GoalTableRow
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
         WatchKit.WKInterfaceLabel EmojiLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
         WatchKit.WKInterfaceLabel GoalLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (EmojiLabel != null) {
                EmojiLabel.Dispose ();
                EmojiLabel = null;
            }

            if (GoalLabel != null) {
                GoalLabel.Dispose ();
                GoalLabel = null;
            }
        }
    }
}