using System;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClockKing
{
	public class ToggledStringElement:StringElement
	{
		public Func<string> PrimaryValueGenerator { get; set;}
		public Func<string> SecondaryValueGenerator {get;set;}

		public Func<string> PrimaryCaptionGenerator { get; set; }
		public Func<string> SecondaryCaptionGenerator { get; set; }
		public Func<string> DefaultCaptionGenerator{ get; set; }


		private IEnumerator<string> valueEnumerator;
		private IEnumerator<string> captionEnumerator;

		private Func<string> currentValueGenerator;
		private Func<string> currentCaptionGenerator;

		public ToggledStringElement (string caption):base(caption)
		{
			this.DefaultCaptionGenerator = () => caption;

			this.valueEnumerator = GetNextValue ().GetEnumerator ();
			this.captionEnumerator = GetNextCaption ().GetEnumerator ();
			this.Tapped += () => this.Toggle();
		}

		public void Toggle()
		{
			this.valueEnumerator.MoveNext ();
			var nextValue = this.valueEnumerator.Current;
			this.Value = nextValue;

			this.captionEnumerator.MoveNext ();
			var nextCaption = this.captionEnumerator.Current;
			this.Caption = nextCaption;
		}

		private IEnumerable<string> GetNextValue()
		{
			bool primary = false;

			while (true) 
			{
				primary = !primary;
				currentValueGenerator = primary ? PrimaryValueGenerator : SecondaryValueGenerator;
				yield return currentValueGenerator ();
			}
		}

		private IEnumerable<string> GetNextCaption()
		{
			bool primary = false;

			while (true) 
			{
				primary = !primary;
				currentCaptionGenerator = primary ? PrimaryCaptionGenerator : SecondaryCaptionGenerator;

				if (currentCaptionGenerator == null)
					currentCaptionGenerator = DefaultCaptionGenerator;
				
				yield return currentCaptionGenerator ();
			}
		}
	}
}

