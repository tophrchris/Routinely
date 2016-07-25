using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;

namespace ClockKing.Core
{
	public  abstract class Command
	{
		public string Name;
		public bool IsDestructive{ get; set; }
		public string Category{ get; set; }
		public string LongName{ get; set; }
		public bool ChangesCheckpoint { get; set;}
		public string ColorName{ get; set; }
        public virtual string EmojiName { get; set; }
        public virtual string EmojiUnified {
            get {
                if (!string.IsNullOrEmpty (EmojiName))
                    return EmojiSharp.Emoji.All [EmojiName].Unified;
                else
                    throw new ArgumentException ();
            }
            set {
                throw new NotImplementedException ();
            }
        }

		public Command(string Color,string Label)
		{
			this.ChangesCheckpoint = true;
			this.Name = Label;
			this.IsDestructive = false;
			this.Category = string.Empty;
			this.ColorName = Color;
		}

		public virtual bool ShouldDecorate(CheckPoint toDecorate)
		{
			return true;
		
		}
        public string NameWithEmoji 
        {
            get {
                if (!string.IsNullOrEmpty (this.EmojiName))
                    return this.EmojiUnified + " " + this.Name;
                else
                    return this.Name;
            }
        }

        public string LongNameWithEmoji 
        {
            get {

                var name = string.IsNullOrEmpty (this.LongName) ? this.Name : this.LongName;

                if (!string.IsNullOrEmpty (this.EmojiName))
                    return this.EmojiUnified + " " + name;
                else
                    return name;
            }
        }
			
		public virtual bool ExecuteFor(iCheckpointCommandController controller, CheckPoint checkPoint)
		{
			throw new NotImplementedException ();
		}
	}

	public interface IDialogBoundCommand
	{
		iNavigatableDialog ExistingDialog{get;set;}

	}

	public abstract class EnabledCheckpointCommand:Command
	{
		public EnabledCheckpointCommand(string color,string title):base(color,title){}
		public override bool ShouldDecorate (CheckPoint toDecorate)
		{
			return toDecorate.Enabled;
		}
	}
	public abstract class DisabledCheckpointCommand:EnabledCheckpointCommand
	{
		public DisabledCheckpointCommand(string color,string title):base(color,title){}
		public override bool ShouldDecorate (CheckPoint toDecorate)
		{
			return !base.ShouldDecorate(toDecorate);
		}
	}
}

