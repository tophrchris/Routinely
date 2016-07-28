using System;
using Foundation;
using System.IO;



namespace ClockKing
{
	public class BundlePathProvider:PathProvider
	{
		public BundlePathProvider():base(string.Empty)
		{
			var bundle = NSBundle.MainBundle;

			var fileName = "examples.json";

			this.MyDocumentsPath = bundle.ResourcePath;
			this.CheckpointPath = Path.Combine(this.MyDocumentsPath, fileName);
			this.OccurrencesPath = string.Empty;
		}
	}
}

