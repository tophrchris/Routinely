using System;
using UIKit;
using Foundation;
using CoreGraphics;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;
using SWTableViewCells;


namespace ClockKing
{
	public class CheckPointTableCell:SWTableViewCell
	{
		public UILabel TitleLabel { get; private set; }

		public CheckPoint CheckPoint { get; set;}

		public Dictionary<string,Tuple<string,UILabel>> autoLabels;

		public static readonly NSString Key = new NSString("cptc");

		public CheckPointTableCell ():base (UITableViewCellStyle.Default, Key)
		{
			CreateSubViews();
			this.Accessory = UITableViewCellAccessory.DisclosureIndicator;
		}

		public void CreateSubViews()
		{
			TitleLabel = new UILabel (new CGRect (70, 6, 260, 22));

			this.CreateAutoLabel ("TargetTime", "Target: {0:t}");
			this.CreateAutoLabel ("Occurrences", "Occurances: {0}");
			this.CreateAutoLabel ("avgTime", "Average: {0}");
			this.CreateAutoLabel ("MostRecent", "Last: {0}");

			LayoutLabels (new CGPoint (73f, 33f), new CGSize (260f / 2f, 16f), 1f);
		
			var container = this.TextLabel.Superview;

			container.AddSubview(TitleLabel);

			container.AddSubviews (autoLabels.Select (kv => kv.Value.Item2).ToArray ());

			this.ApplyStyles ();
		}

		private void ApplyStyles ()
		{
			ApplyStyleToLabels (l =>{ 
				l.BackgroundColor = UIColor.Clear;
				l.Font=UIFont.FromName ("HelveticaNeue", 12);
				l.TextColor=UIColor.FromRGB (151, 157, 155);
				l.HighlightedTextColor=UIColor.White;
			});

			TitleLabel.BackgroundColor = UIColor.Clear;
			TitleLabel.Font = UIFont.FromName ("HelveticaNeue-Bold", 18);
		}

		public void RenderCheckpoint(CheckPoint checkpoint)
		{
			this.CheckPoint = checkpoint;

			this.TextLabel.Text = "☀";
			this.TextLabel.AdjustsFontSizeToFitWidth = true;
			this.TitleLabel.Text = checkpoint.Name;
			setLabel("TargetTime", checkpoint.TargetTime.ToString("t"));
			setLabel ("avgTime", checkpoint.averageObservedTime.ToString ("t"));
			setLabel ("Occurrences", 
				checkpoint.Occurrences.Count ().ToString ());
			if(checkpoint.Occurrences.Any())
				setLabel ("MostRecent",
					checkpoint.Occurrences
					.OrderByDescending (o => o.timeStamp)
					.First()
					.timeStamp
					.ToString("g"));
		}

		public override void SetSelected (bool selected, bool animated)
		{
			base.SetSelected (selected, animated);

			TitleLabel.TextColor = UIColor.FromRGBA (0.29f, 0.29f, 0.29f, 1);

			var yOffset = selected ? 0 : 1;

			var offset = new CGSize (0, yOffset);

			ApplyStyleToLabels (l => l.ShadowOffset = offset);
		}

		private void ApplyStyleToLabels(Action<UILabel> Decorator)
		{
			autoLabels.ToList().ForEach (kv => Decorator (kv.Value.Item2));
		}

		public void CreateAutoLabel(string key, string formatString)
		{
			if(this.autoLabels==null)
				this.autoLabels = new Dictionary<string, Tuple<string, UILabel>> ();
			
			var label = new UILabel ();
			label.Text = string.Format(formatString,string.Empty);
			this.autoLabels.Add (key, Tuple.Create (formatString, label));
		}

		private void setLabel(string key, string value)
		{
			var f = this.autoLabels [key];
			f.Item2.Text = string.Format (f.Item1, value);
		}

		private void LayoutLabels(CGPoint corner, CGSize size, float padding)
		{
			autoLabels ["TargetTime"].Item2.Frame = 
			new CGRect (corner, size);
			autoLabels ["Occurrences"].Item2.Frame = 
			new CGRect (new CGPoint (corner.X, corner.Y + size.Height + padding), size);
			autoLabels ["avgTime"].Item2.Frame = 
			new CGRect (new CGPoint (corner.X + size.Width + padding, corner.Y), size);
			autoLabels ["MostRecent"].Item2.Frame = 
			new CGRect (new CGPoint (corner.X + size.Width + padding, corner.Y + size.Height + padding), size);
		}
	}
}

