using System;
using WatchKit;
using Foundation;
using WatchConnectivity;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ClockKing.RoutinelyWatchExtension
{
	public partial class InterfaceController : WKInterfaceController
	{

		private static WCSession session = WCSession.IsSupported ? WCSession.DefaultSession : null;

		private WCSession reachableSession { get { return session.Reachable ? session : null; } }

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

		public void StartSession()
		{
			Debug.WriteLine("attempting to start session:");
			if (session != null && session.ActivationState!=WCSessionActivationState.Activated)
			{
				session.Delegate = new myDel(this);

				session.ActivateSession();
				Debug.WriteLine("session Activated!");
				if (session.ApplicationContext != null)
				{
					UpdateFromContext(session.ApplicationContext);
				}
			}
		}


		public override void WillActivate()
		{
			StartSession();
			// This method is called when the watch view controller is about to be visible to the user.
			Console.WriteLine("{0} will activate", this);
		}

		public override void DidDeactivate()
		{
			// This method is called when the watch view controller is no longer visible to the user.
			Console.WriteLine("{0} did deactivate", this);

		}

		public void UpdateFromContext(NSDictionary<NSString, NSObject> applicationContext)
		{

			Debug.WriteLine("UFC");
			var goalCount = applicationContext.Count;
			GoalTable.SetNumberOfRows((nint)goalCount, "default");
			int rowIndex = 0;
			foreach (var de in applicationContext)
			{
				var row = GoalTable.GetRowController(rowIndex) as RowController;
				var goalName = ((NSString)de.Key).ToString();
				var goal = JsonConvert.DeserializeObject<Core.CheckPoint>(de.Value.ToString());
				var emoji = goal.Emoji;
				row.render(emoji, goalName);
				rowIndex++;

			}
		}

	}
	public class myDel : WCSessionDelegate
	{
		private InterfaceController controller { get; set; }

		public myDel(InterfaceController Controller)
		{
			this.controller = Controller;
		}
		public override void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message)
		{
			Debug.WriteLine("drm");
			this.controller.UpdateFromContext(message);
		}
		public override void DidReceiveApplicationContext(WCSession session, NSDictionary<NSString, NSObject> applicationContext)
		{
			Debug.WriteLine("drac");
			this.controller.UpdateFromContext(applicationContext);
		}
	}
}
