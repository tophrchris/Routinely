using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace ClockKing
{
	public partial class NavigationController : UINavigationController
	{
		public NavigationController(IntPtr id):base(id){}

		public bool isAwesome { get; set; }
	}

}
