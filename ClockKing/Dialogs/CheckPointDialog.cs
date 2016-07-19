using System;
using MonoTouch.Dialog;
using UIKit;

namespace ClockKing
{


	public class CheckPointDialog : DialogViewController
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
		public CheckPointDialog() : base(UIKit.UITableViewStyle.Grouped, null, true)
	{
	}

}
}