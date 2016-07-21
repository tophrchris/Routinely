using System;
using UIKit;
using Foundation;
using CoreGraphics;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using SWTableViewCells;
//using BarChart;
using Humanizer;
using System.Threading.Tasks;


namespace ClockKing
{
	public class CheckPointTableCell:SWTableViewCell
	{
		public CheckPoint CheckPoint { get; set;}

		public UILabel TitleLabel { get; protected set; }
		public UILabel ProgressLabel { get; protected set;}
		public UILabel TargetLabel { get; protected set;}
		public UILabel AverageLabel { get; protected set;}
		public UILabel MostRecentDay { get; protected set;}
		public UILabel MostRecentLabel { get; protected set;}
		public UILabel EmojiLabel { get; protected set;}
		public UILabel AdditionalDetail { get; protected set;}
//		public BarChartView Chart{ get; protected set;}
		public bool ShowBarChartInLandscape{ get; set;}

		public static readonly NSString Key = new NSString("cptc");
		public static readonly nfloat Height = 120f;

		protected static float padding = 5f;
		protected static float EmojiSize = 50f;
		protected static float AccessoryPadding = 40f;
		protected static float BaseFontSize = 20f;

		protected UIDeviceOrientation RenderedOrientation { get; set;}

		protected CGRect DetailRect { get; set;}
		protected CGRect TitleRect { get; set;}
		protected UIStackView TitleStack { get; set;}
		protected UIStackView DetailStack{ get; set;}

		public CheckPointTableCell(string key):base(UITableViewCellStyle.Default,key)	
		{
			CreateSubViews(this.TextLabel.Superview);
			//this.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			this.Frame = new CGRect (new CGPoint (0, 0), new CGSize (UIApplication.SharedApplication.KeyWindow.Frame.Width, Height));
			this.ShowBarChartInLandscape = false;
		}

		public CheckPointTableCell ():this(Key){ }

		protected void CalculateSizes(float titleHeightAdjustment = 20f)
		{
			var windowFrame = UIApplication.SharedApplication.KeyWindow.Frame;
			var titleWidth = windowFrame.Width - EmojiSize - AccessoryPadding;
			var titleCorner = new CGPoint ((padding * 2.0f) + EmojiSize, padding);
			var titleSize = new CGSize (titleWidth, 22f+titleHeightAdjustment);
			this.TitleRect = new CGRect (titleCorner, titleSize);

			var subLayoutCorner = new CGPoint (titleCorner.X, titleSize.Height + padding);
			var subLayoutSize = new CGSize (titleSize.Width, (Height+titleHeightAdjustment) - (subLayoutCorner.Y+50f));
			this.DetailRect = new CGRect (subLayoutCorner,subLayoutSize);
		}

		protected virtual void CreateSubViews(UIView container)
		{
			this.RenderedOrientation = UIDevice.CurrentDevice.Orientation;
			this.CalculateSizes ();

			this.TitleLabel = new UILabel ();
			this.ProgressLabel=new UILabel();
			this.EmojiLabel = new UILabel (new CGRect(padding*2f,padding*2f,EmojiSize,EmojiSize));

			this.TitleLabel.BackgroundColor = UIColor.Clear;
			this.TitleLabel.Font = UIFont.FromName ("AvenirNext-Regular", BaseFontSize);

			this.ProgressLabel.TextColor=UIColor.FromRGB (.5f, .5f, .5f);
			this.ProgressLabel.TextAlignment = UITextAlignment.Right;
			this.ProgressLabel.Font=UIFont.FromName ("AvenirNext-Regular", BaseFontSize*.6f);

			this.EmojiLabel.Layer.MasksToBounds = true;
			this.EmojiLabel.Layer.CornerRadius=2;
			this.EmojiLabel.Font=UIFont.FromName("HelveticaNeue-Bold", BaseFontSize*2f);

			this.TitleStack = new UIStackView (this.TitleRect);
			TitleStack.Axis = UILayoutConstraintAxis.Horizontal;
			TitleStack.Alignment = UIStackViewAlignment.Fill;
			TitleStack.Distribution = UIStackViewDistribution.FillProportionally;
			TitleStack.Spacing = padding*2f;

			TitleStack.AddArrangedSubview (TitleLabel);
			TitleStack.AddArrangedSubview (ProgressLabel);

			container.AddSubviews (new UIView[]{this.EmojiLabel,TitleStack});

			this.CreateDetailViews (container);

			container.LayoutIfNeeded ();

		}

		protected virtual void CreateDetailViews(UIView container)
		{
			this.TargetLabel = new UILabel();
			this.AverageLabel = new UILabel ();
			this.MostRecentLabel = new UILabel ();
			this.MostRecentDay = new UILabel ();
			this.AdditionalDetail = new UILabel ();

			this.DetailStack = new UIStackView (this.DetailRect);
			DetailStack.Axis = UILayoutConstraintAxis.Horizontal;
			DetailStack.Alignment = UIStackViewAlignment.Fill;
			DetailStack.Distribution = UIStackViewDistribution.FillProportionally;
			DetailStack.Spacing = padding*2f;

			var targetStack = new UIStackView ();
			targetStack.Axis = UILayoutConstraintAxis.Vertical;
			targetStack.Alignment = UIStackViewAlignment.Fill;
			targetStack.Distribution = UIStackViewDistribution.Fill;
			targetStack.AddArrangedSubview (new UILabel (){ Text = "Target",
				Font=UIFont.FromName("AvenirNext-Regular",BaseFontSize*.6f) });
			targetStack.AddArrangedSubview (TargetLabel);

			var avgstack = new UIStackView ();
			avgstack.Axis = targetStack.Axis;
			avgstack.Alignment = targetStack.Alignment;
			avgstack.Distribution = targetStack.Distribution;
			avgstack.AddArrangedSubview(new UILabel(){Text="Average",
				Font=UIFont.FromName("AvenirNext-Regular",BaseFontSize*.6f) });
			avgstack.AddArrangedSubview (AverageLabel);


			var mrstack = new UIStackView ();
			mrstack.Axis = targetStack.Axis;
			mrstack.Alignment = targetStack.Alignment;
			mrstack.Distribution = targetStack.Distribution;
			mrstack.AddArrangedSubview (MostRecentDay);
			mrstack.AddArrangedSubview (MostRecentLabel);


			DetailStack.AddArrangedSubview (targetStack);
			DetailStack.AddArrangedSubview (avgstack);
			DetailStack.AddArrangedSubview (mrstack);

/*			this.Chart = new BarChartView ()
			{AutoLevelsEnabled=false,
				Frame=new CGRect(DetailRect.Location,
					new CGSize(DetailRect.Width*0.3f,DetailRect.Height)),
				BarWidth=15f,
				BarOffset=5f,
				MaximumValue=60f,
				MinimumValue=-60f,
				GridHidden=false,
				LegendHidden=false,
				LevelsHidden=true,
			};
			for(int m=-60;m<=60;m+=30)
				this.Chart.AddLevelIndicator(m);
*/
			container.AddSubview (DetailStack);

			MostRecentDay.Font=UIFont.FromName ("AvenirNext-Regular", BaseFontSize*.6f);
			AdditionalDetail.Font = MostRecentDay.Font;


			foreach (var l in new UILabel[]{this.TargetLabel,this.AverageLabel,this.MostRecentLabel}) {
				l.Font = UIFont.FromName ("AvenirNextCondensed-UltraLight", BaseFontSize*1.1f);
				l.TextColor=UIColor.FromRGB (.5f, .5f, .5f);
			}

			UIView.Animate (.25d, () => DetailStack.LayoutIfNeeded ());
		}
			
		public virtual void RenderCheckpoint(CheckPoint checkpoint)
		{

			this.CheckPoint = checkpoint;
			this.EmojiLabel.Text = string.IsNullOrEmpty(this.CheckPoint.Emoji)?"☀":this.CheckPoint.Emoji;
			this.TitleLabel.Text = checkpoint.Name;

			this.TargetLabel.Text = checkpoint.TargetTimeToday.ToString ("t");
			this.AverageLabel.Text = (DateTime.Now.Date + checkpoint.averageObservedTime).ToString ("t");
			this.MostRecentDay.Text = checkpoint.MostRecentOccurrenceTimeStamp ().ToString ("d");
			this.MostRecentLabel.Text = checkpoint.MostRecentOccurrenceTimeStamp().ToString("t");
			this.ProgressLabel.Text = checkpoint.GetProgress();

			float red = .3f, green = .3f, blue = .3f;

			if (checkpoint.IsMissed)
				red = .9f;
			else
				if (checkpoint.IsSoon())
					green = .9f;

			this.ProgressLabel.TextColor = UIColor.FromRGB(red,green,blue);



			var target = checkpoint.TargetTime.TotalMinutes;
/*
			var data = checkpoint.Occurrences
				.OrderByDescending (o => o.TimeStamp)
				.Select (o => new {delta=o.Time.TotalMinutes-target,occurance=o})
				.Select (o => new BarModel () {Value = (float)o.delta,
				ValueCaptionHidden = true,
					Legend=o.occurance.TimeStamp.Date.Day.ToString(),
				Color = (o.delta > 0) ? UIColor.Red : UIColor.Green
				})
				.Take(5)
				.Reverse();

			this.Chart.ItemsSource = data.ToList ();
*/


			if (this.RenderedOrientation != UIDevice.CurrentDevice.Orientation) 
			{
				this.RenderedOrientation = UIDevice.CurrentDevice.Orientation;

				if (this.isPortrait) {
//					this.Chart.RemoveFromSuperview ();
					this.AdditionalDetail.RemoveFromSuperview ();
					DetailStack.Distribution = UIStackViewDistribution.FillProportionally;
				} else if (ShowBarChartInLandscape)
				{
//					DetailStack.AddArrangedSubview (this.Chart);
					DetailStack.AddArrangedSubview (this.AdditionalDetail);
					DetailStack.Distribution = UIStackViewDistribution.Fill;
					this.updateAdditionalDetail ();
				}

				CalculateSizes ();

				this.TitleStack.Frame = this.TitleRect;
				this.DetailStack.Frame = this.DetailRect;

				UIView.Animate (.25d, () => this.LayoutIfNeeded ());
			}
		}

		public void updateAdditionalDetail()
		{
			var checkpoint = this.CheckPoint;
			this.AdditionalDetail.Text = checkpoint.UntilNextTargetTime.Humanize ();
		}

		protected bool isPortrait
		{
			get 
			{
				return 	this.RenderedOrientation == UIDeviceOrientation.Portrait ||
				this.RenderedOrientation == UIDeviceOrientation.PortraitUpsideDown;
			}	
		}
		public void RenderCheckpointForDetail(CheckPoint checkpoint)
		{
			CalculateSizes (10f);
			this.Accessory = UITableViewCellAccessory.None;
			this.ShowBarChartInLandscape = false;
			this.RenderCheckpoint (checkpoint);
			this.DetailStack.Distribution = UIStackViewDistribution.FillEqually;
		}
	}
}

