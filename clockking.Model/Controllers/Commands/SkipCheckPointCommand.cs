using System;
using ClockKing.Core;

namespace ClockKing
{
    public class SkipCheckPointCommand:EnabledCheckpointCommand
    {
        public SkipCheckPointCommand ():base("Red","Skip")
        {
            this.Category = "Right";
            this.LongName = "Skip this goal for today";
            this.EmojiName = EmojiSharp.Emoji.BLACK_RIGHT_POINTING_DOUBLE_TRIANGLE.ShortName;

        }
        public bool AllowForAllIncompleteGoals { get; set; } = false;

        public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
        {
            var o = controller.SkipCheckpoint (checkPoint);
            return o!=null;
        }

        public override bool ShouldDecorate (CheckPoint toDecorate)
        {
            if (AllowForAllIncompleteGoals)
                return toDecorate.IsActive && !(toDecorate.IsSkipped | toDecorate.CompletedToday);

            return toDecorate.IsActive &&  toDecorate.IsMissed && !toDecorate.IsSkipped;

        }
    }
}

