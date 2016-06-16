using System;

namespace ClockKing.Core
{

	public interface IPathProvider
	{
       
		string GetCheckpointFileName();

		string GetOccurrencesFileName();

		bool Exists (string path);

		string[] ReadAllLines (string path);

		void WriteAllLines (string path, string[] lines);

		void Delete (string path);

		void AppendAllLines (string path, string[] lines);

		string ReadAllText(string path);

		void WriteAllText(string path,string contents);
	}
}

