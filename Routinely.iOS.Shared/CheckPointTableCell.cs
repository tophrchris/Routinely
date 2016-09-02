using System;
using UIKit;
using Foundation;
using CoreGraphics;
using ClockKing.Core;
using SWTableViewCells;
using ClockKing.Core.Shared;

namespace ClockKing
{
	public class CheckPointTableCell:SWTableViewCell
	{
		public enum DisplayModes
		{
			Normal,
			Detail,
			Widget
		}

		public DisplayModes DisplayMode { get; set; } = DisplayModes.Normal;

		public CheckPoint CheckPoint { get; set;}

		public UILabel TitleLabel { get; protected set; }
		public UILabel ProgressLabel { get; protected set;}
		public UILabel TargetLabel { get; protected set;}
		public UILabel AverageLabel { get; protected set;}
		public UILabel MostRecentDay { get; protected set;}
		public UILabel MostRecentLabel { get; protected set;}
		public UILabel EmojiLabel { get; protected set;}
		public UILabel AdditionalDetail { get; protected set;}
		public bool ShowBarChartInLandscape{ get; set;}
		public bool EnqueuedForRefresh { get; set; } = false;

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
			
		}

		public CheckPointTableCell ():this(Key)
		{
			init((float)UIApplication.SharedApplication.KeyWindow.Frame.Width);
		}

		public CheckPointTableCell(float Width,DisplayModes mode) : this(Key)
		{
			this.DisplayMode = mode;
			init(Width);
		}

		private void init(float width)
		{
			try
			{
				CreateSubViews(this.TextLabel.Superview);
				this.Frame = new CGRect(new CGPoint(0, 0), new CGSize(width, Height));
				this.ShowBarChartInLandscape = false;
			}
			catch (Exception e) { }
		}

		protected void CalculateSizes(float titleHeightAdjustment = 20f)
		{
			if (DisplayMode == DisplayModes.Widget)
				titleHeightAdjustment = 10f;
			
			var windowFrame = this.Frame;
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

			if (this.DisplayMode != DisplayModes.Widget)
				this.CreateDetailViews(container);
			
			

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

			container.AddSubview (DetailStack);

			MostRecentDay.Font=UIFont.FromName ("AvenirNext-Regular", BaseFontSize*.6f);
			AdditionalDetail.Font = MostRecentDay.Font;


			foreach (var l in new UILabel[]{this.TargetLabel,this.AverageLabel,this.MostRecentLabel}) {
				l.Font = UIFont.FromName ("AvenirNextCondensed-UltraLight", BaseFontSize*1.1f);
				l.TextColor=UIColor.FromRGB (.5f, .5f, .5f);
			}

			//UIView.Animate (.25d, () => 
			DetailStack.LayoutIfNeeded();
			               //);
		}
			
		public virtual void RenderCheckpoint(CheckPoint checkpoint)
		{
			this.CheckPoint = checkpoint;
			this.EmojiLabel.Text = string.IsNullOrEmpty(this.CheckPoint.Emoji)?"☀":this.CheckPoint.Emoji;
			this.TitleLabel.Text = checkpoint.Name;
			this.ProgressLabel.Text = checkpoint.GetProgress(DisplayMode==DisplayModes.Widget);

			if (this.DisplayMode != DisplayModes.Widget)
			{
				this.TargetLabel.Text = checkpoint.TargetTimeToday.ToString("t");
				this.AverageLabel.Text = (DateTime.Now.Date + checkpoint.AverageCompletionTime).ToString("t");
				this.MostRecentDay.Text = checkpoint.MostRecentOccurrenceTimeStamp().ToString("d");
				this.MostRecentLabel.Text = checkpoint.MostRecentOccurrenceTimeStamp().ToString("t");
			}
			else {
				
			}

			float red = .3f, green = .3f, blue = .3f;

			if (DisplayMode == DisplayModes.Widget)
				red = green = blue = .6f;

			if (checkpoint.IsActive & checkpoint.IsEnabled)
			{
				if (checkpoint.IsMissed)
					red = .9f;
				else
					if (checkpoint.IsSoon())
					green = .9f;
			}

			this.ProgressLabel.TextColor = UIColor.FromRGB(red,green,blue);




			if (this.RenderedOrientation != UIDevice.CurrentDevice.Orientation) 
			{
				this.RenderedOrientation = UIDevice.CurrentDevice.Orientation;

				//if (this.isPortrait) {
				//	this.AdditionalDetail.RemoveFromSuperview ();
				//	DetailStack.Distribution = UIStackViewDistribution.FillProportionally;
				//} else if (ShowBarChartInLandscape)
				//{
				//	DetailStack.AddArrangedSubview (this.AdditionalDetail);
				//	DetailStack.Distribution = UIStackViewDistribution.Fill;
				//	this.updateAdditionalDetail ();
				//}

				CalculateSizes ();

				this.TitleStack.Frame = this.TitleRect;
				this.DetailStack.Frame = this.DetailRect;

				//UIView.Animate (.25d, () => 
				this.LayoutIfNeeded();
				               //);
			}
		}

		//public void updateAdditionalDetail()
		//{
		//	var checkpoint = this.CheckPoint;
		//	this.AdditionalDetail.Text = checkpoint.UntilNextTargetTime.Humanize ();
		//}

		//protected bool isPortrait
		//{
		//	get 
		//	{
		//		return 	this.RenderedOrientation == UIDeviceOrientation.Portrait ||
		//		this.RenderedOrientation == UIDeviceOrientation.PortraitUpsideDown;
		//	}	
		//}

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

