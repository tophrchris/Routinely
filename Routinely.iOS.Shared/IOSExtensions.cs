using System;
using Foundation;
using UIKit;
using System.Collections;
using System.Linq;

namespace ClockKing.Extensions
{
	public static class IOSExtensions
	{
		public static void setParallaxEffect(this UIView view, int effectSize = 5, bool clearExisting = true)
		{
			if (UIAccessibility.IsReduceMotionEnabled)
				return;

			var min = new NSObject();
			var max = new NSObject();
			min = NSNumber.FromInt32(effectSize * -1);
			max = NSNumber.FromInt32(effectSize);

			var h = new UIInterpolatingMotionEffect("center.x", UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis);
			var v = new UIInterpolatingMotionEffect("center.y", UIInterpolatingMotionEffectType.TiltAlongVerticalAxis);

			h.MinimumRelativeValue = v.MinimumRelativeValue = min;
			h.MaximumRelativeValue = v.MaximumRelativeValue = max;

			var g = new UIMotionEffectGroup();
			g.MotionEffects = new[] { h, v };

			if (clearExisting && view.MotionEffects.Any())
				foreach (var me in view.MotionEffects)
					view.RemoveMotionEffect(me);
			
			view.AddMotionEffect(g);
		}
	}
}
