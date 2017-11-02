using System;
using Foundation;
using WatchConnectivity;
using ClockKing.Core;
using ClockKing.Extensions;
using UIKit;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ClockKing
{
	public sealed class WCSessionManager:NSObject, IWCSessionDelegate
	{

		private static readonly WCSessionManager shared = new WCSessionManager();

		public static WCSessionManager Instance{get{return shared;}}

		private static WCSession session = WCSession.IsSupported ? WCSession.DefaultSession : null;

		private WCSession validSession { get { return (session.Paired && session.WatchAppInstalled) ? session : null;}}

		private WCSession reachableSession {get { return session.Reachable ? validSession : null;}}

		private WCSessionDialog responder { get; set; } = new WCSessionDialog ();

		private WCSessionManager()
		{
			
		}

		private DataModel Data
		{
			get
			{
				var app = UIApplication.SharedApplication.Delegate as AppDelegate;
				return app.CheckPointData;
			}
		}

		public void StartSession()
		{
			if (session != null & session.ActivationState!=WCSessionActivationState.Activated)
			{
				session.Delegate = responder;
				session.ActivateSession();
			}
		}


		public bool UpdateSharedContext()
		{
			StartSession();

			if (validSession != null)
			{
				var updated = responder.SetSummaryContext(validSession);
				responder.SendGoalSummaries(validSession);

				return updated;
			}
			return false;
		}

		public void SendGoalSummaryFile ()
		{

			if (validSession != null) 
				responder.SendSummaryFile(validSession);	
		}
	}



	public class WCSessionDialog : WCSessionDelegate
	{
		private DataModel Data {
			get {
				var app = UIApplication.SharedApplication.Delegate as AppDelegate;
				return app.CheckPointData;
			}
		}

		public override void DidReceiveMessageData (WCSession session, NSData messageData, WCSessionReplyDataHandler replyHandler)
		{
			Debug.WriteLine("did recieve message");
			SendGoalSummaries(session);
		}


		public void SendSummaryFile(WCSession session)
		{
			var paths = new PathProvider(".json");
			var url = new NSUrl(@"file://" + paths.GetSummariesFileName());
			Debug.WriteLine(url.AbsoluteString);
			var transfer = session.TransferFile(url, null);
			Debug.WriteLine("WCSM: file transfer began");
		}

		public bool SetSummaryContext(WCSession session)
		{
			var context = GetSummaryContext();
			NSError err;
			return session.UpdateApplicationContext(context, out err);
		}

		public void SendGoalSummaries(WCSession session)
		{
			var context = GetSummaryContext();
			session.SendMessage(context, null, (NSError obj) => { });
		}


		public override void DidFinishFileTransfer(WCSession session, WCSessionFileTransfer fileTransfer, NSError error)
		{
			Debug.WriteLine(fileTransfer.File.FileUrl.AbsoluteString);
			Debug.WriteLine("WCSR: file transfer completed!");
		}


		public NSDictionary<NSString, NSObject> GetSummaryContext()
		{
			NSString key = new NSString("summaries");
			NSObject val = new NSString(GetGoalSummaryJson());
			var context = new NSDictionary<NSString, NSObject>(key, val);
			return context;
		}

		public string GetGoalSummaryJson()
		{
			var goals = GetGoalSummaries();
			return GetGoalSummaryJson(goals);
		}

		private string GetGoalSummaryJson(IEnumerable<GoalSummary> goals)
		{
			string json = JsonConvert.SerializeObject(goals, Formatting.Indented,
				new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

			return json;
		}

		private IEnumerable<GoalSummary> GetGoalSummaries()
		{
			return Data.checkPoints.Values.Select(cp => cp.AsSummary());
		}
	}
}
