using System;
using ClockKing.Core;
using Foundation;
using System.IO;


namespace ClockKing.RoutinelyExtension
{
	public class WatchPathProvider:IPathProvider
	{

		protected string MyDocumentsPath { get; set; }
		protected string CheckpointPath { get; set; }
		protected string OccurrencesPath { get; set; }
		protected virtual string checkpointFileName { get; set; } = "checkpoints";
		protected virtual string occurrencesFileName { get; set; } = "occurrences";

		public WatchPathProvider(string extension)
		{
			var fm = new NSFileManager();
			var agc = fm.GetContainerUrl("group.org.hollanders.routinely");
			var agcPath = agc.Path;

			this.MyDocumentsPath = agcPath; 
			this.CheckpointPath = Path.Combine(this.MyDocumentsPath, checkpointFileName + extension);
			this.OccurrencesPath = Path.Combine(this.MyDocumentsPath, occurrencesFileName + extension);
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
			return File.Exists(path);
		}
		public string[] ReadAllLines(string path)
		{
			return File.ReadAllLines(path);
		}
		public void WriteAllLines(string path, string[] lines)
		{
			File.WriteAllLines(path, lines);
		}
		public void Delete(string path)
		{
			File.Delete(path);
		}
		public void AppendAllLines(string path, string[] lines)
		{
			File.AppendAllLines(path, lines);
		}
		public string ReadAllText(string path)
		{
			return File.ReadAllText(path);
		}
		public void WriteAllText(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}
	}
}

