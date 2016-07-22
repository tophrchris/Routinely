using System;
using System.Linq;
using ClockKing.Core;

namespace ClockKing
{
	public class EditCheckPointCommand:Command
	{
		public EditCheckPointCommand(string Color, string Label):base(Color,Label){}

		public EditCheckPointCommand ():base("Magenta","Edit")
		{
			this.Category = "Left";
			this.LongName = "Edit Goal";

            this.EmojiName = EmojiSharp.Emoji.PENCIL.ShortName;
		}

		public override bool ExecuteFor (iCheckpointCommandController controller, ClockKing.Core.CheckPoint checkPoint)
		{
			controller.PresentEditDialogFor (checkPoint);
			return false;
		}
		public override bool ShouldDecorate (ClockKing.Core.CheckPoint toDecorate)
		{
			return true;
		}
	}

	public class InPlaceEditCheckPointCommand:Command,IDialogBoundCommand
	{
		public iNavigatableDialog ExistingDialog{ get; set;}

		public InPlaceEditCheckPointCommand(iNavigatableDialog existing):this()
		{
			this.ExistingDialog = existing;
		}

		public InPlaceEditCheckPointCommand ():base("Magenta","In Place Edit")
		{
			this.Category = "In Place";
			this.LongName = "Edit Goal...";
		}


		public override bool ExecuteFor (iCheckpointCommandController controller, ClockKing.Core.CheckPoint checkPoint)
		{
			controller.InjectEditDialogIntoExistingDialogFor (checkPoint, this.ExistingDialog);
			return false;
		}
		public override bool ShouldDecorate (ClockKing.Core.CheckPoint toDecorate)
		{
			return true;
		}
	}
}

