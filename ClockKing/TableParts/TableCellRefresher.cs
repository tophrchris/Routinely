using System;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Humanizer;


namespace ClockKing
{

	public class TableCellRefresher
	{
		public enum RefreshRate
		{
			None,
			Instant,
			Fast,
			Standard,
			Slow
		}

		private class RefreshQueue : Queue<CheckPointTableCell>
		{
			public RefreshRate Rate { get; set; }
			public TimeSpan Delay { get; set; }
		}

		private Dictionary<RefreshRate, RefreshQueue> RefreshQueues;


		private List<Task> Refreshers { get; set; } = new List<Task>();
		private Action<Action> executor { get; set; }

		private CancellationTokenSource Cancel { get; set; } = new CancellationTokenSource();

		public TableCellRefresher(Action<Action> invoker)
		{
			this.executor = invoker;
			this.RefreshQueues = new Dictionary<RefreshRate, RefreshQueue>()
			{
				{RefreshRate.Instant,
					new RefreshQueue() { Rate = RefreshRate.Instant, Delay = TimeSpan.FromSeconds(1) }},
				{RefreshRate.Fast,
					new RefreshQueue() { Rate = RefreshRate.Fast, Delay = TimeSpan.FromSeconds(15) }},
				{RefreshRate.Standard,
					new RefreshQueue() { Rate = RefreshRate.Standard, Delay = TimeSpan.FromMinutes(1) }},
				{RefreshRate.Slow,
					new RefreshQueue() { Rate = RefreshRate.Slow, Delay = TimeSpan.FromMinutes(15) }}
			};

		}

		public void EnqueueCell(CheckPointTableCell watch)
		{ 
			var desired = watch.CheckPoint.GetDesiredRefreshRate();

			if (RefreshQueues.ContainsKey(desired))
			{
				//Debug.WriteLine("{0} queued as {1}".FormatWith(watch.CheckPoint.Name, RefreshQueues[desired].Rate.ToString()));

				RefreshQueues[desired].Enqueue(watch);
			}
		}

		public void Restart()
		{
			Cancel.Cancel();
			Task.WaitAll(Refreshers.ToArray());
				
			StartRefresherTasks();
			Cancel = new CancellationTokenSource();
		}
	
		private void StartRefresherTasks()
		{
			this.Refreshers = this.RefreshQueues.Values
				.Select(q => Task.Factory.StartNew(() =>
					   ProcessRefreshQueue(q))).ToList();
		}

		private void ProcessRefreshQueue(RefreshQueue queue)
		{
			try
			{
				var requeue = new Queue<CheckPointTableCell>();
				while (true & !this.Cancel.IsCancellationRequested)
				{
					Task.Delay(queue.Delay, this.Cancel.Token).Wait();
					//Debug.Print("after {0} {1} has {2}"
					 //           .FormatWith(queue.Delay.Humanize(),queue.Rate.ToString(),queue.Count));
					while (queue.Any())
					{

						var toRefresh = queue.Dequeue();
						if (toRefresh != null)
						{
							this.executor(() => toRefresh.RenderCheckpoint(toRefresh.CheckPoint));
							requeue.Enqueue(toRefresh);
						}
					}
					while (requeue.Any())
					{
						var torequeue = requeue.Dequeue();
						EnqueueCell(torequeue);
					}
				}
				Debug.WriteLine("{0} cancelled".FormatWith(queue.Rate.ToString()));
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}
	}
}