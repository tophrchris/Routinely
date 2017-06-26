using System;
using WatchKit;
using Foundation;
using WatchConnectivity;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Generic;
using ClockKing.Core;
using ClockKing.Extensions;
using System.Linq;

namespace ClockKing.RoutinelyWatchExtension
{
	public partial class InterfaceController : WKInterfaceController
	{

		private static WCSession session = WCSession.IsSupported ? WCSession.DefaultSession : null;

		private WCSession reachableSession { get { return session.Reachable ? session : null; } }

		private RoutinelyHostResponder responder { get; set; } 

		protected InterfaceController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}


		public override void Awake(NSObject context)
		{
			this.responder = new RoutinelyHostResponder(this);

			base.Awake(context);
			// Configure interface objects here.
			Console.WriteLine("{0} awake with context", this);
			try
			{
				var goals = ReadGoals();
				UpdateModel(goals);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		public void StartSession()
		{
			Debug.WriteLine("attempting to start session:");


			if (session != null && session.ActivationState!=WCSessionActivationState.Activated)
			{
				session.Delegate = this.responder;
				session.ActivateSession();
				Debug.WriteLine("session Activated!");
				this.responder.RequestSummaries(reachableSession);

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

		public void UpdateModel(List<GoalSummary> goals)
		{
			Debug.WriteLine("UFC");
			var display = goals.Where(g =>g.Active&&g.Enabled).ToList();
			var goalCount = display.Count;
			GoalTable.SetNumberOfRows((nint)goalCount, "default");
			int rowIndex = 0;
			foreach (var goal in display)
			{
				var row = GoalTable.GetRowController(rowIndex) as RowController;
				var goalName = goal.Name;
				var emoji = goal.Emoji;
				row.render(emoji, goalName);
				rowIndex++;
			}
		}

		public void SaveModel(List<GoalSummary> goals)
		{
			var json = JsonConvert.SerializeObject(goals);
			var path = responder.GetSummariesPath();
			System.IO.File.WriteAllText(path, json);
		}

		public List<GoalSummary> ReadGoals()
		{
			var found = new List<GoalSummary>();

			var path = responder.GetSummariesPath();
			if (System.IO.File.Exists(path))
			{
				try
				{
					var json = System.IO.File.ReadAllText(path);
					found = responder.GetGoalsFromJson(json);
					Debug.WriteLine($"read {found.Count} goals from disk");
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.Message);
				}
			}
			return found;
		}
	}


	public class RoutinelyHostResponder : WCSessionDelegate
	{
		private InterfaceController controller { get; set; }

		public RoutinelyHostResponder(InterfaceController Controller)
		{
			this.controller = Controller;
		}

		public void RequestSummaries(WCSession session)
		{
			if (session == null)
				return;
			var msg = new NSDictionary<NSString, NSObject>(new NSString("update"), null);
			session.SendMessage(msg, null, null);
		}

		public override void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message)
		{
			Debug.WriteLine("drm");
			var json = message["summaries"];
			var goals = GetGoalsFromJson(json.ToString());
			this.controller.UpdateModel(goals);
			this.controller.SaveModel(goals);
		}
	
		public override void DidReceiveApplicationContext(WCSession session, NSDictionary<NSString, NSObject> applicationContext)
		{
			Debug.WriteLine("drac");
			var json = applicationContext["summaries"];
			var goals = GetGoalsFromJson(json.ToString());
			this.controller.UpdateModel(goals);
			this.controller.SaveModel(goals);
		}

		public override void DidReceiveFile(WCSession session, WCSessionFile file)
		{
			try
			{
				var dest = GetSummariesPath();
				System.IO.File.Copy(file.FileUrl.AbsoluteString, dest);
				Debug.WriteLine(file.FileUrl.AbsoluteString);
				Debug.WriteLine(dest);
				Debug.WriteLine("WEdrf:file recieved");
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		public List<GoalSummary> GetGoalsFromJson(string json)
		{

			var found = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GoalSummary>>(json);
			return found.OrderBy(f => f.NextTargetTime).ToList();
		}

		public string GetSummariesPath()
		{
			var md = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var path = System.IO.Path.Combine(md, "summaries.json");
			return path;
		}

	}
}
