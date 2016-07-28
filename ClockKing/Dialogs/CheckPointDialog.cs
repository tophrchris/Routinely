using System;
using MonoTouch.Dialog;
using UIKit;
using ClockKing.Core;
namespace ClockKing
{

	/// <summary>
	/// used to help create consistent dialogs.
	/// </summary>
	public class CheckPointDialog : DialogViewController,iNavigatableDialog
	{
		protected NavigationController ContentNavigation
		{
			get
			{
				return ((RootViewController)App.Window.RootViewController).Navigation;
			}
		}
		protected AppDelegate App
		{
			get
			{
				var app = UIApplication.SharedApplication;
				return app.Delegate as AppDelegate;
			}
		}
		protected CheckPointController Controller
		{
			get
			{
				return App.Controller;
			}
		}
		protected SidebarNavigation.SidebarController Sidebar
		{
			get
			{
				return App.Sidebar;
			}
		}

		protected CheckPointManager Manager
		{
			get
			{
				return Controller.CheckPoints;
			}
		}

		public CheckPointDialog() : base(UIKit.UITableViewStyle.Grouped, null, true)
		{
		}

		public void ResetNavigation(bool refreshData = false)
		{
			Controller.ResetNavigation(refreshData);
		}
	}
}