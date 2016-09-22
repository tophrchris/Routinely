using System;
using System.Collections.Generic;
using UIKit;
namespace ClockKing
{
	public static class HapticsManager
	{

		private static UINotificationFeedbackGenerator Notification { get; set; } = new UINotificationFeedbackGenerator();
		private static UIImpactFeedbackGenerator Heavy { get; set; } = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Heavy);
		private static UIImpactFeedbackGenerator Medium { get; set; } = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
		private static UIImpactFeedbackGenerator Light { get; set; } = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
		private static UISelectionFeedbackGenerator Selection { get; set; } = new UISelectionFeedbackGenerator();

		private static Dictionary<UIImpactFeedbackStyle, UIImpactFeedbackGenerator> impacts { get; set; } = new Dictionary<UIImpactFeedbackStyle, UIImpactFeedbackGenerator>();


		static HapticsManager()
		{
			impacts.Add(UIImpactFeedbackStyle.Light, Light);
			impacts.Add(UIImpactFeedbackStyle.Medium, Medium);
			impacts.Add(UIImpactFeedbackStyle.Heavy, Heavy);
		}

		public static void Trigger(UIImpactFeedbackStyle style)
		{
			impacts[style].Prepare();
			impacts[style].ImpactOccurred();
		}
		public static void Trigger(UINotificationFeedbackType style = UINotificationFeedbackType.Success)
		{
			Notification.Prepare();
			Notification.NotificationOccurred(style);
		}
		public static void Tick()
		{
			Selection.Prepare();
			Selection.SelectionChanged();
		}

		public static void NavigationCompleted()
		{
			var gen = Medium;
			gen.Prepare();
			gen.ImpactOccurred();
		}

		public static void ChangeCompleted()
		{
			var gen = Heavy;
			gen.Prepare();
			gen.ImpactOccurred();
		}

		public static void Celebrate()
		{
			Notification.Prepare();
			Notification.NotificationOccurred(UINotificationFeedbackType.Success);
		}

		public static void Denounce()
		{
			Notification.Prepare();
			Notification.NotificationOccurred(UINotificationFeedbackType.Error);
		}

		public static void Warn()
		{
			Notification.Prepare();
			Notification.NotificationOccurred(UINotificationFeedbackType.Warning);
		}

	}
}
