﻿using System;
using MonoTouch.Dialog;
using UIKit;
using ClockKing.Extensions;
using System.Linq;
using EmojiSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClockKing.Core;
using Humanizer;


namespace ClockKing
{
	public class CheckPointEditingDialog:CheckPointDialog
	{
		iCheckpointCommandController CheckPoints{ get; set; }

		private EntryElement nameElement { get; set; }
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
		private BooleanElement categorySwitch { get; set; }
		private MultilineElement instructionsElement { get; set; }
		private bool SuggestAbbreviations = true;
		private CheckPoint existingCheckPoint { get; set; }
		private Section altTargetsSection { get; set; }
		private bool AltTargetsSectionVisible { get; set; } = false;

		public CheckPointEditingDialog (iCheckpointCommandController checkpoints, RootElement root, bool pushing) :base()
		{
			this.Root = root;
			this.CheckPoints = checkpoints;
			this.Style = UITableViewStyle.Grouped;

			this.emojiNames = Emoji.All.Where (kv => kv.Value.AppleHasImage).Select (kv => kv.Key.ToLower ()).ToList ();
			this.nameElement = new EntryElement ("Name", "Name your goal", "");
			this.emojiElement = new EntryElement ("Abbreviation", "short name or emoji","");
			this.categorySwitch = new BooleanElement("Specify category?", false);
			this.categoryElement = new EntryElement("Category", "You can specify a category for your goal", "");
			this.nowElement = new BooleanElement ("Complete right now?", false);
			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.Time };
			this.SuggestEmoji = new BooleanElement ("suggest emoji for abbreviation?", true);
			this.nowSwitch = new BooleanElement ("default to current time:", true);

			this.instructionsElement = new MultilineElement ("What time do you expect to complete this goal, each day?");
			this.pickerWrapper = new UIViewElement (string.Empty, picker, false);

			var altTargetOption = new BooleanElement("Enable Alternate Targets?", false);
			var altTargetDescription = new MultilineElement("Alternative targets allow you to designate different target times by day of week.");
			var AddAltTargetButton = new StringElement("Add Alternative Target");
			AddAltTargetButton.Alignment = UITextAlignment.Center;


			var checkPointForm = new Section ("Goal:") { 	
				nameElement,
				SuggestEmoji,
				emojiElement,
				instructionsElement,
				pickerWrapper,
				nowSwitch,
				nowElement,
				categorySwitch,
				altTargetOption
			};

			altTargetOption.ValueChanged += (sender, e) => 
			{
				if (altTargetOption.Value)
				{
					checkPointForm.Insert(altTargetOption.IndexPath.Row + 1, UITableViewRowAnimation.Top, altTargetDescription);

					checkPointForm.Insert(altTargetOption.IndexPath.Row + 2, UITableViewRowAnimation.Top, AddAltTargetButton);
				}
				else {
					checkPointForm.Remove(altTargetDescription);
					checkPointForm.Remove(AddAltTargetButton);
				}

			};




			this.SuggestEmoji.ValueChanged += (s, e) => this.SuggestAbbreviations = SuggestEmoji.Value;

			this.categorySwitch.ValueChanged += (sender, e) => 
			{
				if (this.categorySwitch.Value)
					checkPointForm.Insert(this.categorySwitch.IndexPath.Row + 1, UITableViewRowAnimation.Top, this.categoryElement);
				else
					checkPointForm.Remove(this.categoryElement);
			};

			AddAltTargetButton.Tapped += () =>
			{
				var cmd = Controller.Commands.Commands["Alt Target"] as AddScheduledTargetCommand;
				cmd.ExistingDialog = this;
				if (this.nameElement.Value != string.Empty)
				{
					if (this.existingCheckPoint == null)
						this.existingCheckPoint = this.CreateCheckpoint();
					else
						UpdateCheckpoint(this.existingCheckPoint);
					
					cmd.ExecuteFor(this.Controller.CheckPoints, this.existingCheckPoint);
				}
			};


			this.nameElement.NotifyChangedOnKeyStroke = true;

			this.nameElement.Changed += (s, ev) => AutoSetEmoji();

			picker.MinuteInterval = ClockKingOptions.MinimumInterval;
			picker.ValueChanged += (s, e) => nowSwitch.Value=false;

			nowSwitch.ValueChanged += (s, e) => {
				if (nowSwitch.Value)
					this.picker.Date = DateTime.UtcNow.ToNSDate ();
			};

			this.Root.Add(checkPointForm);
					
			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,
				(s,e)=>this.Save()),true);
		}

		public override void ViewDidAppear(bool animated)
		{
			this.App.Track("Edit Goal");
			if (AltTargetsSectionVisible)
				this.Root.Remove(altTargetsSection);
			
			if (this.existingCheckPoint != null)
			{
				if (this.existingCheckPoint.ScheduledTargets.Any())
				{
					this.altTargetsSection = new AlternativeTargetsSection(existingCheckPoint, this.Controller.CheckPoints, this);
					this.Root.Add(altTargetsSection);
					AltTargetsSectionVisible = true;
				}

			}
			base.ViewDidAppear(animated);

		}

		#region iNavigatableDialog implementation

		public override void ResetNavigation (bool refreshData=false)
		{
			((iNavigatableDialog)this.CheckPoints).ResetNavigation ();
		}

		#endregion

		public void RenderForCheckPoint(CheckPoint toEdit)
		{
			var knownEmoji = Emoji.All.Where (e => e.Value.AppleHasImage).Select (e => e.Value.Unified).ToList ();
			this.existingCheckPoint = toEdit;
			this.nameElement.Value = toEdit.Name;
			this.categoryElement.Value = toEdit.Category;
			this.emojiElement.Value = toEdit.Emoji;
			var section = this.Root.First();
			section.Remove (this.pickerWrapper.IndexPath.Row);
			section.Remove (this.nowElement.IndexPath.Row);
			section.Remove (this.nowSwitch.IndexPath.Row);
			this.targetTimeElement = new TimeElement("Target time", DateTime.Today + toEdit.TargetTime);


			if (toEdit.RelativeTarget != null)
			{
				var msg = string.Format("The target time for this goal is dynamically set to {0} after the completion of {1}{2}.",
				                        toEdit.RelativeTarget.Offset.Humanize(),
			                toEdit.RelativeTarget.RelatedCheckPoint.Emoji, 
			                toEdit.RelativeTarget.RelatedCheckPoint.Name);
				var originalIInstructions = instructionsElement.Caption;
				this.instructionsElement.Caption = msg;
				this.instructionsElement.Tapped += () => 
				{

					this.CheckPoints.PresentConfirmationDialog(() =>
					{
						toEdit.RelativeTarget = null;
						this.CheckPoints.ResaveCheckpoints();

						section.Insert(instructionsElement.IndexPath.Row + 1, new Element[] { targetTimeElement });
						this.instructionsElement.Caption = originalIInstructions;
					},
					Message: "Would you like to remove the relative target from this goal?"
					);	
				};
				
			}
			else 
			{
				section.Insert(instructionsElement.IndexPath.Row + 1, new Element[] { targetTimeElement });
			}
			this.enabledSwitch = new BooleanElement("Enabled?", toEdit.IsEnabled);
			section.Add(enabledSwitch);

			if (knownEmoji.Contains (toEdit.Emoji))
				SuggestAbbreviations = false;

			if (!string.IsNullOrEmpty(toEdit.Category))
				this.categorySwitch.Value = true;

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
			var isNew = !CheckPoints.CheckPointExists(toEdit.UniqueIdentifier);
			var nameChanged = toEdit.Name != this.nameElement.Value;

			var updated = UpdateCheckpoint(toEdit);
			if (!updated)
				return false;



			if (isNew)
			{	
				this.CheckPoints.AddNewCheckPoint(toEdit);
			}
			else 
			{
				this.CheckPoints.UpdateCheckPoint(toEdit);	
				this.CheckPoints.ResaveCheckpoints();
			}
			if(nameChanged & !isNew)
				this.CheckPoints.RewriteOccurrences();
				
			ResetNavigation ();
			return true;
		}



		public bool Save()
		{
			if (this.existingCheckPoint != null)
				return this.Save(this.existingCheckPoint);
			
			if (this.CheckPoints.CheckPointExists(nameElement.Value))
				return ShowError("A goal already exists with that name.  Please choose a different name!");
			CheckPoint newcp = CreateCheckpoint();

			ResetNavigation();

			return newcp != null;
		}

		private CheckPoint CreateCheckpoint()
		{
			var newcp = this.CheckPoints
								.AddNewCheckPoint(
									nameElement.Value,
								picker.Date.ToDateTime().ToLocalTime().TimeOfDay,
									emojiElement.Value, categoryElement.Value);

			if (nowElement.Value)
				this.CheckPoints.AddOccurrenceToCheckPoint(newcp, 0);
			return newcp;
		}
		private bool UpdateCheckpoint(CheckPoint toEdit)
		{
			var isNew = !CheckPoints.CheckPointExists(toEdit.UniqueIdentifier);
			var nameChanged = toEdit.Name != this.nameElement.Value;

			if ((nameChanged | isNew) && this.CheckPoints.CheckPointExists(this.nameElement.Value))
			{
				ShowError("A goal already exists with the new name you've chosen.  Please choose a different name!");
				return false;
			}
			TimeSpan targetTime;

			if (this.targetTimeElement != null)
				targetTime = targetTimeElement.DateValue.ToLocalTime().TimeOfDay;
			else
				targetTime = picker.Date.ToDateTime().ToLocalTime().TimeOfDay;
			
			toEdit.Category = this.categoryElement.Value;
			toEdit.Emoji = this.emojiElement.Value;
			toEdit.TargetTime = targetTime;
			if (enabledSwitch != null)
				toEdit.IsEnabled = enabledSwitch.Value;
			else
				toEdit.IsEnabled = true;
			if (nameChanged)
				toEdit.Name = this.nameElement.Value;

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