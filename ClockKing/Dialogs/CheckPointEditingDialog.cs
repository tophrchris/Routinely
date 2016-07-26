using System;
using MonoTouch.Dialog;
using UIKit;
using ClockKing.Extensions;
using System.Linq;
using EmojiSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClockKing.Core;


namespace ClockKing
{
	public class CheckPointEditingDialog:DialogViewController,iNavigatableDialog
	{
		iCheckpointCommandController CheckPoints{ get; set; }

		private EntryElement nameElement { get; set; }
		private TimeElement targetElement { get; set; }
		private EntryElement emojiElement { get; set; }
		private EntryElement categoryElement { get; set; }
		private BooleanElement nowElement { get; set; }
		private UIDatePicker picker { get; set; }
		private UIViewElement pickerWrapper { get; set; }
		private BooleanElement nowSwitch{ get; set; }
		private List<string> emojiNames { get; set; }
		private BooleanElement SuggestEmoji{ get; set; }
		private BooleanElement enabledSwitch{ get; set; }
		private TimeElement targetTimeElement{ get; set; }
		private bool SuggestAbbreviations = true;

		public CheckPointEditingDialog (iCheckpointCommandController checkpoints, RootElement root, bool pushing) : base (root, pushing)
		{
			this.CheckPoints = checkpoints;
			this.Style = UITableViewStyle.Grouped;

			this.emojiNames = Emoji.All.Where (kv => kv.Value.AppleHasImage).Select (kv => kv.Key.ToLower ()).ToList ();
			this.nameElement = new EntryElement ("Name", "Name your goal", "");
			this.emojiElement = new EntryElement ("Abbreviation", "a short (2-letter) name","");
			this.categoryElement = new EntryElement("Category", "You can specify a category for your goal", "");
			this.nowElement = new BooleanElement ("Add Occurrence now?", false);
			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.Time };
			this.SuggestEmoji = new BooleanElement ("suggest emoji for abbreviation?", true);
			this.nowSwitch = new BooleanElement ("default to current time:", true);

			var instructions = new MultilineElement ("What time do you expect to complete this goal, each day?");
			this.pickerWrapper = new UIViewElement (string.Empty, picker, false);

			var checkPointForm = new Section ("Goal:") { 	
				nameElement,
				categoryElement,
				SuggestEmoji,
				emojiElement,
				instructions,
				pickerWrapper,
				nowSwitch,
				nowElement 
			};

			this.SuggestEmoji.ValueChanged += (s, e) => this.SuggestAbbreviations = SuggestEmoji.Value;

			this.nameElement.NotifyChangedOnKeyStroke = true;

			this.nameElement.Changed += (s, ev) => AutoSetEmoji();

			picker.ValueChanged += (s, e) => nowSwitch.Value=false;

			nowSwitch.ValueChanged += (s, e) => {
				if (nowSwitch.Value)
					this.picker.Date = DateTime.UtcNow.ToNSDate ();
			};

			this.Root.Add(checkPointForm);
					
			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,
				(s,e)=>this.Save()),true);
		}

		#region iNavigatableDialog implementation

		public void ResetNavigation (bool refreshData=false)
		{
			((iNavigatableDialog)this.CheckPoints).ResetNavigation ();
		}

		#endregion

		public void RenderForCheckPoint(CheckPoint toEdit)
		{
			var knownEmoji = Emoji.All.Where (e => e.Value.AppleHasImage).Select (e => e.Value.Unified).ToList ();

			this.nameElement.Value = toEdit.Name;
			this.categoryElement.Value = toEdit.Category;
			this.emojiElement.Value = toEdit.Emoji;
			var section = this.Root.First();
			section.Remove (this.pickerWrapper.IndexPath.Row);
			section.Remove (this.nowElement.IndexPath.Row);
			section.Remove (this.nowSwitch.IndexPath.Row);
			this.targetTimeElement = new TimeElement ("Target time", toEdit.TargetTimeToday);
			this.enabledSwitch = new BooleanElement ("Enabled?", toEdit.Enabled);
			section.AddAll(new Element[]{targetTimeElement, enabledSwitch});

			if (knownEmoji.Contains (toEdit.Emoji))
				SuggestAbbreviations = false;

			SuggestEmoji.Value = SuggestAbbreviations;


			Task.Factory.StartNew (() => {	
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();	
				var detailSections = new Section[]
				{
					new CheckPointCellSection(toEdit),
					new CheckPointStatsSection(toEdit,()=>this.ReloadData())
				};
				this.InvokeOnMainThread(()=>UIView.Animate(.25d,()=>this.Root.Add(detailSections)));
			});

			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,
				(s,e)=>this.Save(toEdit)),true);
		}

		public bool ShowError(string Message)
		{
			this.CheckPoints.PresentChoices("Goal",Message,new[]{new ModalChoice(){Label="Ok"}});

			return false;
		}

		public bool Save(CheckPoint toEdit)
		{
			var nameChanged = toEdit.Name!=this.nameElement.Value;

			if(nameChanged && this.CheckPoints.CheckPointExists(this.nameElement.Value))
			{
				ShowError ("A goal already exists with the new name you've chosen.  Please choose a different name!");
				return false;
			}

			toEdit.Category = this.categoryElement.Value;
			toEdit.Emoji=this.emojiElement.Value;
			toEdit.TargetTime=targetTimeElement.DateValue.ToLocalTime().TimeOfDay;
			toEdit.Enabled= enabledSwitch.Value;
			if(nameChanged)
				toEdit.Name=this.nameElement.Value;

			this.CheckPoints.ResaveCheckpoints();

			if(nameChanged)
				this.CheckPoints.RewriteOccurrences();
				
			ResetNavigation ();
			return true;
		}

		public bool Save()
		{
			if (this.CheckPoints.CheckPointExists (nameElement.Value))
				return ShowError ("A goal already exists with that name.  Please choose a different name!");	
			
			var newcp = this.CheckPoints
					.AddNewCheckPoint (
						nameElement.Value,
					picker.Date.ToDateTime().ToLocalTime().TimeOfDay,
						emojiElement.Value,categoryElement.Value);

				if (nowElement.Value)
					this.CheckPoints.AddOccurrenceToCheckPoint(newcp,0);

				ResetNavigation ();

			return true;
		}

		//TODO: move this to EmojiExtensions
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