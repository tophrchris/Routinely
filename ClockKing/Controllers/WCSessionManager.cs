using System;
using Foundation;
using WatchConnectivity;
using ClockKing.Core;
using ClockKing.Extensions;
using UIKit;
using System.Runtime.CompilerServices;

namespace ClockKing
{
	public sealed class WCSessionManager:NSObject, IWCSessionDelegate
	{

		private static readonly WCSessionManager shared = new WCSessionManager();

		public static WCSessionManager Instance{get{return shared;}}

		private static WCSession session = WCSession.IsSupported ? WCSession.DefaultSession : null;

		private WCSession validSession { get { return (session.Paired && session.WatchAppInstalled) ? session : null;}}

		private WCSession reachableSession {get { return session.Reachable ? validSession : null;}}

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
				session.Delegate = this;
				session.ActivateSession();
			}
		}


		public bool UpdateSharedContext()
		{
			StartSession();

			if (validSession != null)
			{
				var context = DictionaryExtensions<string, CheckPoint>.ToContextDictionary(Data.checkPoints);
				NSError err;
				var updated =  validSession.UpdateApplicationContext(context, out err);



				validSession.SendMessage(context, null, (obj) => {});
				return updated;
			}
			else
				return false;
		}




		[Export("session:didReceiveMessage:replyHandler:")]
		public void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message, 
		                               WCSessionReplyHandler replyHandler)
		{
			System.Diagnostics.Debug.WriteLine("did recieve message");
			var context = DictionaryExtensions<string, CheckPoint>.ToContextDictionary(Data.checkPoints);
			session.SendMessage(context, null, (NSError obj) => {});
		}

	}
}
