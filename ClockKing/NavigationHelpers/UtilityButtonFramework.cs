using System;
using Foundation;
using UIKit;
using SWTableViewCells;
using System.Collections.Generic;
using ClockKing.Model;
using System.Linq;

namespace ClockKing
{

	public class UtilityButton:UIButton
	{
		private string title;

		public UtilityButton(UIColor Color,string Label):base(UIButtonType.Custom)
		{
			this.title = Label;

			this.BackgroundColor = Color;
			this.SetTitle (Label, UIControlState.Normal);
			this.SetTitleColor (UIColor.White, UIControlState.Normal);
			this.TitleLabel.AdjustsFontSizeToFitWidth = true;
		}

		/// <summary>
		/// Executes for.
		/// </summary>
		/// <returns><c>true</c>, if execution resulted in a change to the model, <c>false</c> otherwise.</returns>
		/// <param name="controller">Controller.</param>
		/// <param name="checkPointPair">Check point pair.</param>
		public virtual bool ExecuteFor(CheckPointController controller, CheckPoint checkPoint)
		{
			var buttonName = this.title;


			Console.WriteLine ("{0} button was pressed.", buttonName);

			new UIAlertView (
				string.Format("Utility Button for {0}",checkPoint.Name), 
				string.Format ("{0} button was pressed.", buttonName), null, "OK", null).Show ();

			return false;
		}

		protected void MsgBox(string title,string message)
		{
			new UIAlertView (title, message,null, "OK", null).Show ();
		}
	}
		
		
		
	public class UtilityButtonDelegate : SWTableViewCellDelegate
	{
		private CheckPointController Controller{ get; set; }
		private Dictionary<string,UtilityButton> Utilities { get; set;}

		public UtilityButtonDelegate(CheckPointController controller)
		{
			this.Controller = controller;
			this.Utilities = new Dictionary<string, UtilityButton> ();
		}

		public override void DidTriggerLeftUtilityButton (SWTableViewCell cell, nint index)
		{
			var cpp = ((CheckPointTableCell)cell).CheckPoint;
			if(this.ExecuteUtilityCommandForCheckpoint (cpp, cell.LeftUtilityButtons, (int)index))
				this.Controller.TableView.ReloadData();
		}

		public override void DidTriggerRightUtilityButton (SWTableViewCell cell, nint index)
		{
			var cpp = ((CheckPointTableCell)cell).CheckPoint;
			if(this.ExecuteUtilityCommandForCheckpoint (cpp, cell.RightUtilityButtons, (int)index))
				this.Controller.TableView.ReloadData();
		}

		private bool ExecuteUtilityCommandForCheckpoint(CheckPoint checkPoint, IEnumerable<UIButton> utilities, int utilityIndex)
		{
			var foundUtility = utilities.ElementAt (utilityIndex);
			var titleName = foundUtility.CurrentTitle;

			return  Utilities[titleName].ExecuteFor (this.Controller, checkPoint);
		}

		public bool AttachUtilityButtonsToCell(CheckPointTableCell cell)
		{

			List<UtilityButton> leftButtons;
			List<UtilityButton> rightButtons;

			if (cell.CheckPoint.Enabled) {
				leftButtons = new List<UtilityButton> () {
					new DisableCheckPointButton ()
				};
					
				rightButtons = new List<UtilityButton> () {
					new AddOccurrenceButton (),
					new AddHistoricOccurrenceButton ()
				};

			} else {
				leftButtons = new List<UtilityButton> () {
					new DeleteCheckPointButton()
				};

				rightButtons = new List<UtilityButton> () {
					new EnableCheckPointButton()
				};
			}

			leftButtons.Concat(rightButtons).ToList().ForEach(b=>
				{
					if(!Utilities.ContainsKey(b.CurrentTitle))
							Utilities.Add(b.CurrentTitle,b);
				});


			cell.LeftUtilityButtons = leftButtons.ToArray();
			cell.RightUtilityButtons = rightButtons.ToArray();

			return true;
		}
	}
}

