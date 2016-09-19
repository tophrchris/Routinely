using System;
using ClockKing.Core;
using Foundation;
using System.IO;
using System.Collections.Generic;

namespace ClockKing
{
	public class PathProvider:IPathProvider
	{
		protected string MyDocumentsPath{ get; set;}
		protected string CheckpointPath{ get; set;}
		protected string OccurrencesPath{ get; set; }
		protected virtual string checkpointFileName { get; set; } =  "checkpoints";
		protected virtual string occurrencesFileName  { get; set; } = "occurrences";

		public PathProvider(string extension)
		{
			try
			{
				this.MyDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				this.CheckpointPath = Path.Combine(this.MyDocumentsPath, checkpointFileName + extension);
				this.OccurrencesPath = Path.Combine(this.MyDocumentsPath, occurrencesFileName + extension);
			}
			catch { }
		}

		public string GetCheckpointFileName()
		{
			return this.CheckpointPath;
		}
		public string GetOccurrencesFileName()
		{
			return this.OccurrencesPath;
		}
		public bool Exists(string path)
		{
			return File.Exists (path);
		}
		public string[] ReadAllLines(string path)
		{
			return File.ReadAllLines (path);
		}
		public void WriteAllLines(string path,string[] lines)
		{
			File.WriteAllLines (path, lines);
		}
		public void Delete(string path)
		{
			File.Delete (path);
		}
		public void AppendAllLines(string path,string[] lines)
		{
			File.AppendAllLines (path, lines);
		}
		public string ReadAllText(string path)
		{
			return File.ReadAllText (path);
		}
		public void WriteAllText(string path,string contents)
		{
			File.WriteAllText(path,contents);
		}
	}
	public class AppGroupPathProvider : PathProvider
	{
		public static string SuiteName { get; set; } = "group.org.hollanders.routinely";
		public string AppGroupPath
		{
			get
			{
				var found = string.Empty;
				NSUrl furl;
							
				furl= NSFileManager.DefaultManager.GetContainerUrl(SuiteName);

				if (furl == null)
					found = @"/Users/chollander/Library/Developer/CoreSimulator/Devices/2F7C0634-6A70-4307-8890-BE9B51E3482D/data/Containers/Shared/AppGroup/F319239F-A6C4-44D5-863E-C317EC4B040A";
				  //found = @"/Users/chollander/Library/Developer/CoreSimulator/Devices/7BFEECA6-DB92-4AE0-BDB0-22874C6D9D2D/data/Containers/Shared/AppGroup/4FF57D16-9149-4612-A231-F4669D96B442/";
				else
					found = furl.Path;
				
				return found;
			}
		}
		public AppGroupPathProvider(string extension):base(extension)
		{
			

			this.MyDocumentsPath = this.AppGroupPath;
			this.CheckpointPath = Path.Combine(this.MyDocumentsPath, checkpointFileName + extension);
			this.OccurrencesPath = Path.Combine(this.MyDocumentsPath, occurrencesFileName + extension);


		}
	}
}

