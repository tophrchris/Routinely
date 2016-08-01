using System;
using System.Collections.Generic;
using System.Linq;
using ClockKing.Core;
using MonoTouch.Dialog;
using ClockKing.Extensions;

namespace ClockKing
{
	public class SampleGoalBrowsingDialog:CheckPointDialog
	{
		private List<CheckPoint> samples { get; set; }		
		public SampleGoalBrowsingDialog()
		{
			this.Style = UIKit.UITableViewStyle.Grouped;
			var sampleDataModel = new DataModel(new JSONDataProvider(new BundlePathProvider()), false); ;
			this.samples = sampleDataModel.checkPoints.Values.ToList();

			this.Root = new RootElement("Add a Goal");
			this.Root.Add(
				new Section("Custom Goal:", "Create a custom goal with a name, target time, abbreviation, and more.")
					{
				new StyledStringElement("Write your own...",()=>ShowCustomAddDialog()){Accessory=UIKit.UITableViewCellAccessory.DisclosureIndicator}
			});

			var categoryEmoji = new Dictionary<string, string>()
			{
				{"nutrition",EmojiSharp.Emoji.FORK_AND_KNIFE.Unified},
				{"wellness",EmojiSharp.Emoji.PERSON_WITH_FOLDED_HANDS.Unified},
				{"chores",EmojiSharp.Emoji.TOILET.Unified}
			};

			var byCategory = from goal in samples
							 where !string.IsNullOrEmpty(goal.Category)
			                 orderby goal.TargetTime
							 group goal by goal.Category into bycat
							 select bycat;

			var categorySection = new Section("Browse Examples:");

			Func<CheckPoint,StringElement> goalToElement = (g) => 
			{
				var label = string.Format("{1} {0}", g.Name, g.Emoji);
				var e = new StringElement(label,g.TargetTime.ToAMPMString());
				e.Tapped += () => ShowCustomAddDialog(g);
				return e;
			};

			foreach (var category in byCategory)
			{
				var key = category.Key.ToLower();
				var categoryLabel = categoryEmoji.ContainsKey(key) ?
												 categoryEmoji[key] + " " + category.Key :
												 category.Key;
				var subRoot = new RootElement(categoryLabel);
				var catsec = new Section("example goals for " + category.Key);

				catsec.AddAll(category.Select(g =>goalToElement(g)));
				subRoot.Add(catsec);
				categorySection.Add(subRoot);
			}
			this.Root.Add(categorySection);

			var optionsSection = new Section("Option:","Go straight to the custom goal page, instead of showing examples.");
			optionsSection.Add(
				new BooleanElement("Next time, Skip this screen.", false) );

			this.Root.Add(optionsSection);
		}
		private void ShowCustomAddDialog(CheckPoint existing = null)
		{
			var d = new CheckPointEditingDialog(this.Manager, new RootElement("Add..."), true);

			if (existing != null)
			{
				existing.UniqueIdentifier = Guid.NewGuid();
				existing.CreatedOn = DateTime.Now;
				d.RenderForCheckPoint(existing);
			}

			Manager.NavigateToDialog(d);
		}
	}
}

