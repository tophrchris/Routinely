using System;
using MonoTouch.Dialog;
using System.Collections.Generic;

namespace ClockKing
{
	public class ToggledStringElement:StringElement
	{
		private IEnumerator<string> generator;

		public ToggledStringElement (string caption):base(caption)
		{
			this.generator = GetNextLabel ().GetEnumerator ();

			this.Tapped += () => this.Toggle();
		}

		public void Toggle()
		{
			this.generator.MoveNext ();
			var next = this.generator.Current;
			this.Value = next;
		}

		public Func<string> PrimaryGenerator { get; set;}
		public Func<string> SecondaryGenerator {get;set;}

		public IEnumerable<string> GetNextLabel()
		{
			bool primary = false;

			while (true) 
			{
				primary = !primary;
				yield return primary ? PrimaryGenerator () : SecondaryGenerator ();
			}
		}
	}
}

