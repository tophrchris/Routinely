using System;
using UIKit;
using CoreGraphics;


namespace ClockKing
{
	public class TimeView:UIStackView
	{
		private DateTime value;
		public DateTime Value { 
		get
			{
				return this.value;
			}
		set 
			{
				this.value = value;
				this.HourLabel.Text = (value.Hour-(value.Hour>12?12:0)).ToString() + ":";
				this.MinuteLabel.Text =  value.Minute.ToString("D2");
				this.AMPMLabel.Text = value.Hour >= 12 ? "PM" : "AM";
			}
		}
		public UIFont Font { 
		get
			{
				return this.HourLabel.Font;
			}
		
		set
			{
				this.HourLabel.Font = value;
				this.MinuteLabel.Font = UIFont.FromName (value.Name, value.PointSize*.75f);
				this.AMPMLabel.Font = UIFont.FromName (value.Name, value.PointSize * .50f);
			}
		}
		public UIColor TextColor{
			get{
				return this.HourLabel.TextColor;
			}
			set
			{
				this.HourLabel.TextColor = this.MinuteLabel.TextColor = this.AMPMLabel.TextColor = TextColor;
			}
		}

		protected UILabel HourLabel { get; set; }
		protected UILabel MinuteLabel { get; set; }
		protected UILabel AMPMLabel { get; set; }

		public TimeView ()
		{
			this.HourLabel = new UILabel ();
			this.MinuteLabel = new UILabel ();
			this.AMPMLabel = new UILabel ();

			var v = new UIStackView ();

			this.Axis = UILayoutConstraintAxis.Horizontal;
			v.Axis = UILayoutConstraintAxis.Vertical;
			this.Alignment = UIStackViewAlignment.Fill;
			v.Alignment = UIStackViewAlignment.Fill;
			this.Distribution = UIStackViewDistribution.EqualCentering;
			v.Distribution = UIStackViewDistribution.Fill;
		
			this.AddArrangedSubview (HourLabel);
			this.AddArrangedSubview (v);
			v.AddArrangedSubview (MinuteLabel);
			v.AddArrangedSubview (AMPMLabel);

			this.LayoutIfNeeded ();
		}
	}
}

