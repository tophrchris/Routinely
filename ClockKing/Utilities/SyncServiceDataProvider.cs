using System;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;
using UIKit;

namespace ClockKing
{
	public class SyncServiceDataProvider:ICheckPointDataProvider
	{
		iCloudDocumentDataProvider provider; 

		public SyncServiceDataProvider(iCloudDocumentDataProvider existing)
		{
			this.provider = existing;
		}

		public IEnumerable<CheckPoint> ReadCheckPoints()
		{
			if(!this.provider.HasUbiquity)
				yield break;

			var existingEnumerator = this.provider.ReadCheckPoints();

			if (existingEnumerator.Any())
			{
				var app = UIApplication.SharedApplication.Delegate as AppDelegate;
				var con = app.Controller;
				var o = SharedDialogs.ConfirmationDialog((obj) =>
				{
					ClockKingOptions.SynchronizeDataViaICloud = true;
				},
                     "Enable Synchronization?",
                     "Routinely has detected existing data saved in your iCloud account.  Would you like to synchronize that data to this device? Any changes you make to your data from any synchronized device will be visisble on any other synchronized device.",
				                                        "Yes, enable!","No",true);

				con.InvokeOnMainThread(() =>
				{
					if (!ClockKingOptions.SynchronizeDataViaICloud)
						con.PresentModalViewController(o, true);
				});
				if (ClockKingOptions.SynchronizeDataViaICloud)
					foreach (var f in existingEnumerator)
						yield return f;
				
				yield break;
			}
		}


		public int LoadOccurrences(Dictionary<string, CheckPoint> checkPoints)
		{
			return LoadOccurrences(checkPoints);
		}

		public bool WriteAllOccurrences(IEnumerable<Occurrence> occurrences)
		{
			return provider.WriteAllOccurrences(occurrences);
		}

		public bool WriteCheckPoints(IEnumerable<CheckPoint> CheckPoints)
		{
			return provider.WriteCheckPoints(CheckPoints);
		}

		public void WriteOccurrence(Occurrence toSave)
		{
			provider.WriteOccurrence(toSave);
		}
	}
}

