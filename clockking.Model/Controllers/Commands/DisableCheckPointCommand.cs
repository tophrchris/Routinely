using System;
using ClockKing.Core;

namespace ClockKing
{
	public class DisableCheckPointCommand:EnabledCheckpointCommand
	{
		public DisableCheckPointCommand():base("DarkGray","Disable")
		{
			this.Category = "Left";
            this.EmojiName = EmojiSharp.Emoji.MEDIUM_WHITE_CIRCLE.ShortName;
		}

		public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
		{
			checkPoint.IsEnabled = false;
			return true;
		}
	}
}

