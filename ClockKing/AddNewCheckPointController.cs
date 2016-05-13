using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using MonoTouch.Dialog;
using PickerCells;
using System.Collections.Generic;
using PickerCells.Data;

namespace ClockKing
{
	partial class AddNewCheckPointController : DialogViewController
	{
		public CheckPointTableViewController Parent { get; set; }

		public AddNewCheckPointController (IntPtr handle) : base (handle)
		{
			this.Pushing = true;

		}
		public override void ViewDidLoad ()
		{

			//maybe i don't need this controler at all?!?!


			base.ViewDidLoad ();
			//this.TableView.Source = new DateTableViewSource ();
			var section = new Section ("New Checkpoint:");

			var nameElement = new EntryElement ("name", "Name your checkpoint", "");
			var instructions = new MultilineElement ("specify the time that you expect to complete this checkpoint, each day:");

			var targetElement = new TimeElement ("target", DateTime.Now);

			var aCell = new DatePickerCell(UIDatePickerMode.Time,DateTime.Now)
			{
				Key = "1234",
			};
			aCell.TextLabel.Text = "Date";
			aCell.RightLabelTextAlignment = UITextAlignment.Right;
			aCell.OnItemChanged += (object sender, PickerCellArgs e) => {

				var result = e.Items;
			};



			var saveButton = new StringElement ("save", 
				() =>{ 
					this.Parent.AddNewCheckPoint(nameElement.Value,targetElement.DateValue.TimeOfDay);
					this.DismissModalViewController(true);
				}){Alignment=UITextAlignment.Right};

			var cancelButton = new StringElement ("cancel", 
				() =>{ 
					this.DismissModalViewController(true);
				}){Alignment=UITextAlignment.Right};


			section.AddAll (new Element[]{ nameElement, instructions, targetElement, saveButton,cancelButton });
			this.Root.Add (section);
		}

		public class TextEntryCell:UITableViewCell
		{
			UILabel label;
			UITextField text;
			public TextEntryCell (NSString cellId):base(UITableViewCellStyle.Default,cellId)
			{
				this.label=new UILabel();
				this.text=new UITextField();
				this.text.BackgroundColor=UIColor.Blue;
				this.text.Placeholder="enter a name";
				ContentView.AddSubviews(new UIView[]{this.label,this.text});
			}

			public string Label{set{ this.label.Text=value; }}
			public string Value{get{ return this.text.Text; }}

			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				var fullHeight = ContentView.Bounds.Height;
				var halfWidth = ContentView.Bounds.Width / 2;
				label.Frame = new CoreGraphics.CGRect (5, 5, halfWidth, fullHeight);
				text.Frame = new CoreGraphics.CGRect (halfWidth, 5, halfWidth,fullHeight);
			}

			public override bool BecomeFirstResponder ()
			{
				 base.BecomeFirstResponder ();
				return this.text.BecomeFirstResponder ();
			}
		}

		public class DateTableViewSource : UITableViewSource
		{
			private float DefaultRowHeight = 44.0f;
			private List<UITableViewCell> formCells;

			public DateTableViewSource()
			{
				formCells = new List<UITableViewCell>();

				var aCell = new DatePickerCell(UIDatePickerMode.Time,DateTime.Now)
				{
					Key = "1234",
				};
				aCell.TextLabel.Text = "Date";
				aCell.RightLabelTextAlignment = UITextAlignment.Right;
				aCell.OnItemChanged += (object sender, PickerCellArgs e) => {

					var result = e.Items;
				};


				var nameCell = new TextEntryCell(new NSString("name")){Label="Name"};
				formCells.Add(nameCell);

				formCells.Add(aCell);






			}
				
			public override nint RowsInSection(UITableView tableview, nint section)
			{
				// TODO: return the actual number of items in the section
				return formCells.Count;
			}

			public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
			{

				var aCell = GetCell(tableView,indexPath);

				if (aCell is BasePickerCell)
				{
					var datePickerTableViewCell = aCell as BasePickerCell;

					return datePickerTableViewCell.PickerHeight;

				}

				return DefaultRowHeight;

			}
			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				return formCells[indexPath.Row];
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{

				var aCell = GetCell(tableView,indexPath);

				if (aCell is TextEntryCell)
					aCell.BecomeFirstResponder ();

				if (aCell is BasePickerCell)
				{
					var datePickerTableViewCell = aCell as BasePickerCell;

					datePickerTableViewCell.SelectedInTableView(tableView);

					tableView.DeselectRow(indexPath, true);
				}
			}
		}
	}
}



/*

	
					this.Parent.AddNewCheckPoint(nameElement.Value,targetElement.DateValue.TimeOfDay);
					this.DismissModalViewController(true);
				
					(so,e)=>this.PerformSegue("Return",this)), true);



Next steps

Setup/getting started
Familiar places
The importance of patterns
Gotchas*/