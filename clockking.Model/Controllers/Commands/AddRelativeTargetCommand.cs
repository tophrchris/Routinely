using System;
using System.Collections.Generic;
using System.Linq;
using ClockKing.Core;

namespace ClockKing
{
    public class AddRelativeTargetCommand : EnabledCheckpointCommand, IDialogBoundCommand
    {
        public iNavigatableDialog ExistingDialog { get; set; }

        public AddRelativeTargetCommand () : base ("Blue", "Rel Target")
        {
            this.Category = "In Place";
            this.LongName = "Add Relative Target";
            this.EmojiName = EmojiSharp.Emoji.LINK_SYMBOL.ShortName;
        }

        public AddRelativeTargetCommand (iNavigatableDialog existing) : this ()
        {
            this.ExistingDialog = existing;
        }

        public override bool ExecuteFor (iCheckpointCommandController controller, ClockKing.Core.CheckPoint checkPoint)
        {
            controller.PresentRelativeTargetDialogForCheckpoint (checkPoint, ExistingDialog);
            return false;
        }

        public override bool ShouldDecorate (CheckPoint toDecorate)
        {
            if (toDecorate.RelativeTarget != null)
                return false;
            
            return base.ShouldDecorate (toDecorate);
        }
    }
}

