using System;
using UIKit;
using Foundation;
using SidebarNavigation;
using MonoTouch.Dialog;

namespace ClockKing
{
	public class RootViewController:UIViewController
	{

		public SidebarController SideBar { get; set; }
		public NavigationController Navigation { get; set; }
		private UIStoryboard _storyboard;

		public override UIStoryboard Storyboard
		{
			get
			{
				if (_storyboard == null)
					_storyboard = UIStoryboard.FromName("Main", null);
				return _storyboard;
			}
		}

		public RootViewController() : base(null, null)
		{

			var menu = new Menu();
			var content = Storyboard.InstantiateInitialViewController();

			this.Navigation = content as NavigationController;
		
			this.SideBar = new SidebarController(this, content, menu);

			this.SideBar.MenuLocation = SidebarController.MenuLocations.Left;
			this.SideBar.StatusBarMoves = false;
		}
	}
}

