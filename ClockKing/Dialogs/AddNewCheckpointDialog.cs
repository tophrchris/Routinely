using System;
using MonoTouch.Dialog;
using Foundation;
using UIKit;
using ClockKing.Extensions;
using System.Linq;
using EmojiSharp;

namespace ClockKing
{
	public class AddNewCheckpointDialog:DialogViewController
	{
		CheckPointController Controller{ get; set; }

		private EntryElement nameElement { get; set; }
		private TimeElement targetElement { get; set; }
		private EntryElement emojiElement { get; set; }
		private BooleanElement nowElement { get; set; }
		private UIDatePicker picker { get; set; }
		private BooleanElement nowSwitch{ get; set; }

		public AddNewCheckpointDialog (CheckPointController controller, RootElement root, bool pushing) : base (root, pushing)
		{
			this.Controller = controller;
			this.Style = UITableViewStyle.Grouped;

			this.nameElement = new EntryElement ("Name", "Name your checkpoint", "");
			this.emojiElement = new EntryElement ("Abbreviation", "a short (2-letter) name","");
			this.nowElement = new BooleanElement ("Add Occurrence now?", false);
			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.Time };
			this.nowSwitch = new BooleanElement ("default to current time:", true);

			var instructions = new MultilineElement ("What time do you expect to complete this checkpoint, each day?");
			var pickerWrapper = new UIViewElement (string.Empty, picker, false);

			this.nameElement.NotifyChangedOnKeyStroke = true;

			this.nameElement.Changed += (s, ev) => {
				var words = this.nameElement.Value.Split(' ');

//				var emojiNames = Emoji.All.Where(e=>e.Value.AppleHasImage).Select(e=>e.Key.ToLower()).ToList();
//
//				var wordsToSearch = words.Where(w=>w.Length>2).Select(w=>w.ToLower());
//
//				if(wordsToSearch.Any(w=>emojiNames.Any(e=>e.ToLower().Contains(w)))){
//					var foundEmoji = emojiNames.Where(e=>wordsToSearch.Any(w=>e.ToLower().Contains(w)));
//					if(foundEmoji.Any())
//					{
//						var key = foundEmoji.First();
//						this.emojiElement.Value = Emoji.All.First(e=>e.Key.ToLower()==key).Value.AsShortcode();
//						//this.emojiElement.Value="wtf!";
//					}
//				}
//				else{
				if(words.Count()>1)
					this.emojiElement.Value = string.Join("", words.Where(w=>w.Length>0).Take(2).Select(w=>w.Substring(0,1)).ToArray());
				else
					if(words[0].Length>0)
						this.emojiElement.Value = words[0].Substring(0,this.nameElement.Value.Length>1?2:1); 
					else
						this.emojiElement.Value = string.Empty;
				//}
			};

			picker.ValueChanged += (s, e) => nowSwitch.Value=false;

			nowSwitch.ValueChanged += (s, e) => {
				if (nowSwitch.Value)
					this.picker.Date = DateTime.UtcNow.ToNSDate ();
			};

			this.Root.Add(new Section ("New Checkpoint:")
				{ 	
					nameElement,
					emojiElement,
					instructions,
					pickerWrapper,
					nowSwitch,
					nowElement 
				});
					
			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,(s,e)=>this.Save()),true);
		}

		public bool Save(){
			
			var newcp = this.Controller
					.AddNewCheckPoint (
						nameElement.Value,
					picker.Date.ToDateTime().ToLocalTime().TimeOfDay,
						emojiElement.Value);

				if (nowElement.Value)
					this.Controller.AddOccurrenceToCheckPoint(newcp,0);

				
				this.Controller.RespondToModelChanges ();
				CancelDialog ();

			return true;
		}
		public void CancelDialog()
		{
			this.Controller.NavigationController.PopViewController (true);
		}
	}
}

