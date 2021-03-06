﻿using System;
using System.IO;
using System.Collections.Generic;
using ClockKing.Core;
using System.Threading.Tasks;
using Foundation;
using CloudKit;
using UIKit;
using Newtonsoft.Json;
using ClockKing.Extensions;
using System.Linq;
using System.Diagnostics;

namespace ClockKing
{
	public class iCloudDocumentDataProvider : ICheckPointDataProvider
	{
		private Task<NSUrl> ubiquityResolver { get; set; }
		private StringDocument checkpointDocument { get; set; }
		private StringDocument occurrenceDocument { get; set; }

		protected NSUrl UbiquityContainerUrl
		{
			get
			{
				return ubiquityResolver.Result;
			}
		}
		protected string DocumentsUrl
		{
			get
			{
				return Path.Combine(this.UbiquityContainerUrl.Path, "Documents");
			}
		}

		public bool HasUbiquity
		{
			get { return this.UbiquityContainerUrl != null; }
		}

		public iCloudDocumentDataProvider()
		{

			this.ubiquityResolver = new Task<NSUrl>(() =>
				NSFileManager.DefaultManager.GetUrlForUbiquityContainer(null));
			this.ubiquityResolver.Start();
			if (HasUbiquity)
			{
				var checkpointPath = Path.Combine(DocumentsUrl, "Checkpoints.json");
				var occurrencesPath = Path.Combine(DocumentsUrl, "Occurrences.csv");
				this.checkpointDocument = new StringDocument(new NSUrl(checkpointPath, false));
				this.occurrenceDocument = new StringDocument(new NSUrl(occurrencesPath, false));
			}
		}

		public int LoadOccurrences(Dictionary<string, CheckPoint> checkPoints)
		{
			int added = 0;
			if (!HasUbiquity)
				return added;
			try
			{
				var read = this.occurrenceDocument.documentData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				if (read.Any())
				{
					foreach (var line in read)
					{
						//TODO: extract this into core?
						bool Skipped = false;
						var parts = line.Split('|');
						var name = parts[0];
						var timeStamp = DateTime.Parse(parts[1]);
						if (parts.Length == 3)
							bool.TryParse(parts[2], out Skipped);
						if (checkPoints.ContainsKey(name))
						{
							var newOccurrence = checkPoints[name].CreateOccurrence(timeStamp);
							newOccurrence.IsSkipped = Skipped;
							checkPoints[name].AddOccurrence(newOccurrence);
							added++;
						}
					}
				}
			}
			catch { }

			return added;
		}

		public IEnumerable<CheckPoint> ReadCheckPoints()
		{
			if (!HasUbiquity)
				yield break;

			string json = string.Empty;

			using (new docHandler(this.checkpointDocument))
			{
				try
				{
					if (this.checkpointDocument.documentData != null &&
					   this.checkpointDocument.documentData != string.Empty)
						json = this.checkpointDocument.documentData;
					else
						yield break;
				}
				catch { yield break; }
			}

			var cv = JsonConvert.DeserializeObject<List<CheckPoint>>(json) as IEnumerable<CheckPoint>;

			foreach (var cp in cv)
			{
				if (cp.UniqueIdentifier == Guid.Empty)
					cp.UniqueIdentifier = Guid.NewGuid();

				yield return cp;
			}


			yield break;

		}

		public bool WriteAllOccurrences(IEnumerable<Occurrence> occurrences)
		{
			if (!HasUbiquity)
				return false;
			var lines = string.Join(Environment.NewLine, occurrences.Select(o => o.AsWriteable()).ToArray());

			using (new docHandler(this.occurrenceDocument))
			{
				this.occurrenceDocument.documentData = lines;
				this.occurrenceDocument.UpdateChangeCount(UIDocumentChangeKind.Done);
			}
			return true;
		}

		public bool WriteCheckPoints(IEnumerable<CheckPoint> CheckPoints)
		{
			if (!HasUbiquity)
				return false;
			var json = JsonConvert.SerializeObject(CheckPoints, Formatting.Indented,
				new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

			using (new docHandler(this.checkpointDocument))
			{
				this.checkpointDocument.documentData = json;
				this.checkpointDocument.UpdateChangeCount(UIDocumentChangeKind.Done);
			}
			return true;
		}

		public void WriteOccurrence(Occurrence toSave)
		{
			if (!HasUbiquity)
				return;
			var line = toSave.AsWriteable();
			using (new docHandler(this.occurrenceDocument))
			{
				this.occurrenceDocument.documentData += Environment.NewLine + line;
				this.occurrenceDocument.UpdateChangeCount(UIDocumentChangeKind.Done);
			}
		}

	}
	public class docHandler:IDisposable
	{
		private StringDocument held { get; set; }
		public docHandler(StringDocument toHold)
		{
			this.held = toHold;
			UIApplication.SharedApplication.InvokeOnMainThread(() =>
				{
					this.held.Initialize();
					var t = this.held.OpenAsync();
					var success = t.Wait(TimeSpan.FromSeconds(5));
					successMessenger(success, "open");
				});
		}

		void successMessenger(bool success,string operation)
		{
			Debug.WriteLine("{0}:{1}:{2}", this.held.FileUrl, operation, success);
		}

		public void Dispose()
		{
			UIApplication.SharedApplication.InvokeOnMainThread(() =>
				{
					this.held.SavePresentedItemChanges((obj) => successMessenger(obj==null,"save"));
					var t = this.held.CloseAsync();
					var success = t.Wait(TimeSpan.FromSeconds(5));
					successMessenger(success, "close");
				});
		}
	}

	public class StringDocument : UIDocument
	{
		
		private NSString data;

		public string documentData
		{
			get
			{
				return data.ToString();
			}
			set
			{
				this.data = new NSString(value);
			}
		}


		public StringDocument(NSUrl url) : base(url)
		{
			this.data = new NSString(string.Empty);
		}

		public void Initialize()
		{
			if (!File.Exists(this.FileUrl.Path))
			{
				this.Save(this.FileUrl, UIDocumentSaveOperation.ForCreating,
						  (success) =>
						  {
							  if (success)
								  Console.WriteLine("created");
							  else
								  Console.WriteLine("not created");
						});
			}
		}
		public override bool LoadFromContents(NSObject contents, string typeName, out NSError outError)
		{
			outError = null;


			if (contents != null)
				this.data = NSString.FromData((NSData)contents, NSStringEncoding.UTF8);

			//NSNotificationCenter.DefaultCenter.PostNotificationName("StringDataLoaded", this);
			return true;
		}

		public override NSObject ContentsForType(string typeName, out NSError outError)
		{
			outError = null;
			NSData docData = this.data.Encode(NSStringEncoding.UTF8);
			return docData;
		}
	}
}

