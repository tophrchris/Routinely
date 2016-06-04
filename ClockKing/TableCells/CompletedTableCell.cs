using System;
using Foundation;
using UIKit;
using CoreGraphics;
using BarChart;
using System.Linq;

namespace ClockKing
{
	public class CompletedTableCell:CheckPointTableCell
	{

		public static new readonly NSString Key = new NSString("comcptc");
		protected BarChartView chart { get; set;}


		public CompletedTableCell ():base(Key)
		{

		}


		protected override void CreateDetailViews (UIView container)
		{
			this.chart = new BarChartView {
				Frame= new 
					CGRect(this.DetailRect.Location,
						new CGSize(this.DetailRect.Width*.75f,this.DetailRect.Height)),
				GridHidden=false,LevelsHidden=true,LegendHidden=true,BarWidth=8f,
				BarOffset=2f
			};
			this.chart.AddLevelIndicator (-60);
			this.chart.AddLevelIndicator (-30);
			this.chart.AddLevelIndicator(0);
			this.chart.AddLevelIndicator (30);
			this.chart.AddLevelIndicator (60);


			container.AddSubview(this.chart);
		}



		public override void RenderCheckpoint (ClockKing.Core.CheckPoint checkpoint)
		{

			var targetTime = (float)checkpoint.TargetTime.TotalMinutes ;

			var data = checkpoint.Occurrences
				.OrderByDescending (o => o.timeStamp)
				.Select (o => (float)o.Time.TotalMinutes)
				.Select (m => targetTime - m)
				.Take (10)
				.Reverse();


			chart.ItemsSource = data.Select(v => 
				new BarModel { Value = v,
					Color=(v>0)?UIColor.Red:UIColor.Green,
					ValueCaptionHidden = true }).ToList();

			this.CheckPoint = checkpoint;
			this.EmojiLabel.Text = checkpoint.Emoji;
			this.TitleLabel.Text = checkpoint.Name;

			UIView.Animate (.25d, () => chart.LayoutIfNeeded ());

		}
	}
}

