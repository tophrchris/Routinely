using System;
using MonoTouch.Dialog;
using Foundation;
using UIKit;
using ClockKing.Extensions;
using System.Linq;
using EmojiSharp;
using System.Text;
using System.Unicode;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClockKing.Core;


namespace ClockKing
{
	public class CheckPointEditingDialog:DialogViewController
	{
		CheckPointController Controller{ get; set; }

		private EntryElement nameElement { get; set; }
		private TimeElement targetElement { get; set; }
		private EntryElement emojiElement { get; set; }
		private BooleanElement nowElement { get; set; }
		private UIDatePicker picker { get; set; }
		private BooleanElement nowSwitch{ get; set; }
		private List<string> emojiNames { get; set; }
		private BooleanElement SuggestEmoji{ get; set; }
		private bool SuggestAbbreviations = true;

		public CheckPointEditingDialog (CheckPointController controller, RootElement root, bool pushing) : base (root, pushing)
		{
			this.Controller = controller;
			this.Style = UITableViewStyle.Grouped;

			this.emojiNames = Emoji.All.Where (kv => kv.Value.AppleHasImage).Select (kv => kv.Key.ToLower ()).ToList ();
			this.nameElement = new EntryElement ("Name", "Name your checkpoint", "");
			this.emojiElement = new EntryElement ("Abbreviation", "a short (2-letter) name","");
			this.nowElement = new BooleanElement ("Add Occurrence now?", false);
			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.Time };
			this.SuggestEmoji = new BooleanElement ("suggest emoji for abbreviation?", true);
			this.nowSwitch = new BooleanElement ("default to current time:", true);

			var instructions = new MultilineElement ("What time do you expect to complete this checkpoint, each day?");
			var pickerWrapper = new UIViewElement (string.Empty, picker, false);

			var checkPointForm = new Section ("Goal:") { 	
				nameElement,
				SuggestEmoji,
				emojiElement,
				instructions,
				pickerWrapper,
				nowSwitch,
				nowElement 
			};

			this.nameElement.NotifyChangedOnKeyStroke = true;

			this.nameElement.Changed += (s, ev) => AutoSetEmoji();

			picker.ValueChanged += (s, e) => nowSwitch.Value=false;

			nowSwitch.ValueChanged += (s, e) => {
				if (nowSwitch.Value)
					this.picker.Date = DateTime.UtcNow.ToNSDate ();
			};

			this.Root.Add(checkPointForm);
					
			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,(s,e)=>this.Save()),true);
		}

		public void RenderForCheckPoint(ClockKing.Core.CheckPoint toEdit)
		{
			var knownEmoji = Emoji.All.Where (e => e.Value.AppleHasImage).Select (e => e.Value.Unified).ToList ();

			this.nameElement.Value = toEdit.Name;
			this.emojiElement.Value = toEdit.Emoji;
			var section = this.Root.First();
			section.Remove (4);
			section.Remove (this.nowElement.IndexPath.Row);
			section.Remove (this.nowSwitch.IndexPath.Row);
			var targetTimeElement = new TimeElement ("Target time", toEdit.TargetTimeToday);
			var enabledSwitch = new BooleanElement ("Enabled?", toEdit.Enabled);
			section.AddAll(new Element[]{targetTimeElement, enabledSwitch});

			if (knownEmoji.Contains (toEdit.Emoji))
				SuggestAbbreviations = false;

			SuggestEmoji.Value = SuggestAbbreviations;


			var t = Task.Factory.StartNew (() => {	
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();	
				var detailSections = CheckPointDetailDialog.GetDetailSections (toEdit,Controller);
				this.InvokeOnMainThread(()=>UIView.Animate(.25d,()=>this.Root.Add(detailSections)));
			});

			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,(s,e)=>
				{
					var nameChanged = toEdit.Name!=this.nameElement.Value;

					if(nameChanged && this.Controller.CheckPointExists(this.nameElement.Value))
					{
							ShowError ("A goal already exists with the new name you've chosen.  Please choose a different name!");
							return;
					}

					toEdit.Emoji=this.emojiElement.Value;
					toEdit.TargetTime=targetTimeElement.DateValue.ToLocalTime().TimeOfDay;
					toEdit.Enabled= enabledSwitch.Value;
					if(nameChanged)
						toEdit.Name=this.nameElement.Value;

					this.Controller.ResaveCheckpoints();

					if(nameChanged)
						this.Controller.RewriteOccurrences();
					
					this.Controller.ResetNavigation();
				}),true);
		}

		public bool ShowError(string Message)
		{
			var acs = UIAlertController.Create ("Goal",Message, UIAlertControllerStyle.Alert);
			acs.AddAction (UIAlertAction.Create ("Ok", UIAlertActionStyle.Default, null));
			this.Controller.PresentViewController (acs, true, null);
			return false;
		}

		public bool Save()
		{
			if (this.Controller.CheckPointExists (nameElement.Value))
				return ShowError ("A goal already exists with that name.  Please choose a different name!");	
			
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
		public void CancelDialog(bool animated=true)
		{
			this.Controller.NavigationController.PopViewController (animated);
		}

		public void AutoSetEmoji()
		{
			if (!SuggestAbbreviations)
				return;
			
			var wordsFoundInName = this.nameElement.Value.Split (' ');

			var searchableWords = wordsFoundInName.Where (w => w.Length >= 2).Select (w => w.ToLower ()).ToList();

			var foundEmojis = searchableWords.SelectMany (w => emojiNames.Where (e => e.Contains (w)));

			if (foundEmojis.Any () && SuggestEmoji.Value) 
			{
				var sortedEmoji = foundEmojis
					.OrderBy (e => e.Length - searchableWords.OrderByDescending(w=>w.Length).FirstOrDefault (w => e.Contains (w)).Length);

				var bestEmojiName = sortedEmoji.First ();

				var foundEmoji = Emoji.All [bestEmojiName].Unified;

				if (foundEmoji.Length < 3)
					this.emojiElement.Value = foundEmoji;
			} 
			else
			{
				if (wordsFoundInName.Where(w=>w.Length>1).Count () > 1)
					this.emojiElement.Value = string.Join ("", wordsFoundInName.Where (w => w.Length > 0).Take (2).Select (w => w.Substring (0, 1)).ToArray ());
				else if (wordsFoundInName [0].Length > 0)
					this.emojiElement.Value = wordsFoundInName [0].Substring (0, this.nameElement.Value.Length > 1 ? 2 : 1);
				else
					this.emojiElement.Value = string.Empty;
			}
		}
	}
}