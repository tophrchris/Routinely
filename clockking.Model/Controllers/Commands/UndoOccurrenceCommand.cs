using System;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace ClockKing
{
    public class UndoOccurrenceCommand : EnabledCheckpointCommand
    {
        public UndoOccurrenceCommand ():base("Red","Undo")
        {
            this.LongName = "Undo completion for today";
            this.Category = "Right";
            this.EmojiName = EmojiSharp.Emoji.LEFTWARDS_ARROW_WITH_HOOK.ShortName;
        }
        public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
        {
            var undone = false;
            controller.PresentConfirmationDialog (() => 
            {
                undone = checkPoint.RemoveOccurrences (DateTime.Today);
                if (undone)
                    controller.RewriteOccurrences ();
            }
            , "Are you sure you would like to undo completion of this goal?", "You can complete it again at any time"); 
            return undone;
        }
        public override bool ShouldDecorate (CheckPoint toDecorate)
        {
            return toDecorate.CompletedToday && toDecorate.SinceLastOccurrence.TotalMinutes < 6;
        }
    }
}

