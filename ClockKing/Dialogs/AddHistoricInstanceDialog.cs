using System;
using UIKit;

namespace ClockKing
{
	public class AddHistoricInstanceDialog:UIViewController
	{
		public AddHistoricInstanceDialog ()
		{
			
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();



			var Rows = new UIStackView ();
			Rows.Axis = UILayoutConstraintAxis.Vertical;
			Rows.Alignment = UIStackViewAlignment.Center;
			Rows.Distribution = UIStackViewDistribution.EqualSpacing;
			Rows.Spacing = 30;

			var label = new UITextField ();
			label.Text = "o hai!";
			var picker = new UIDatePicker (){ Mode = UIDatePickerMode.DateAndTime };


			Rows.Frame = View.Bounds;



			Rows.AddArrangedSubview (label);
			Rows.AddArrangedSubview (picker);

			View.AddSubview (Rows);
			Rows.LayoutIfNeeded ();




			//UIView.Animate (.25, () => Rows.LayoutIfNeeded ());


		}

	}
}

