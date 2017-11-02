using Foundation;
using System;
using UIKit;

namespace ClockKing.RoutinelyWatchExtension
{
    public partial class RowController : NSObject
    {
        public RowController (IntPtr handle) : base (handle)
        {
			
        }
		public void render(string emoji, string name)
		{
			this.emojiLabel.SetText(emoji);
			this.GoalTitleLabel.SetText(name);
		}
    }
}