using System;

using WatchKit;
using Foundation;
using ClockKing.Core;
using System.Linq;

namespace ClockKing.RoutinelyExtension
{
	public partial class InterfaceController : WKInterfaceController
	{
		
		protected InterfaceController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void Awake(NSObject context)
		{
			base.Awake(context);

			// Configure interface objects here.
			Console.WriteLine("{0} awake with context", this);
		}

		public override void WillActivate()
		{
			// This method is called when the watch view controller is about to be visible to the user.
			Console.WriteLine("{0} will activate", this);

			var p = new WatchPathProvider(".json");
			var j = new JSONDataProvider(p);
			var d = new DataModel(j, false);


			this.GoalTable.SetNumberOfRows(d.checkPoints.Count, "GoalTableRow");
			for (nint i = 0; i < 5; i++)
			{
				((GoalTableRow)this.GoalTable.GetRowController(i)).Label = d.checkPoints.Keys.ElementAt((int)i);

			}
		}

		public override void DidDeactivate()
		{
			// This method is called when the watch view controller is no longer visible to the user.
			Console.WriteLine("{0} did deactivate", this);
		}
	}
}
