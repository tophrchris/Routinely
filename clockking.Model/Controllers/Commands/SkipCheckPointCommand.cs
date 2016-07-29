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

        }

        public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
        {
            var o = controller.SkipCheckpoint (checkPoint);
            return o!=null;
        }

        public override bool ShouldDecorate (CheckPoint toDecorate)
        {

            return toDecorate.Active &&  toDecorate.IsMissed && !toDecorate.IsSkipped;
            //return base.ShouldDecorate (toDecorate);
        }
    }
}

