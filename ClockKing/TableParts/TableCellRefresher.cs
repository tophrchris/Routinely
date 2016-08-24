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
			public TimeSpan interval { get; set; }
			public DateTime LastProcessed { get; set; } = DateTime.Now;
		}

		private Dictionary<RefreshRate, RefreshQueue> RefreshQueues { get; set; }=new Dictionary<RefreshRate, RefreshQueue>();


		private Task Refresher { get; set; }
		private Action<Action> executor { get; set; }

		private CancellationTokenSource Cancel { get; set; } = new CancellationTokenSource();
		private bool logging { get; set; } = false;

		public TableCellRefresher(Action<Action> invoker)
		{
			this.executor = invoker;

		}

		public void EnqueueCell(CheckPointTableCell watch)
		{ 
			var desired = watch.CheckPoint.GetDesiredRefreshRate();

			if (RefreshQueues.ContainsKey(desired))
				RefreshQueues[desired].Enqueue(watch);
		}

		void ConstructQueues()
		{
			this.RefreshQueues = new Dictionary<RefreshRate, RefreshQueue>()
			{
				{RefreshRate.Instant,
					new RefreshQueue() { Rate = RefreshRate.Instant, interval = TimeSpan.FromSeconds(1) }},
				{RefreshRate.Fast,
					new RefreshQueue() { Rate = RefreshRate.Fast, interval = TimeSpan.FromSeconds(15) }},
				{RefreshRate.Standard,
					new RefreshQueue() { Rate = RefreshRate.Standard, interval = TimeSpan.FromMinutes(1) }},
				{RefreshRate.Slow,
					new RefreshQueue() { Rate = RefreshRate.Slow, interval = TimeSpan.FromMinutes(15) }}
			};
		}

		public void Restart()
		{
			log("triggering cancellation");	
			Cancel.Cancel();

			if (Refresher != null && !Refresher.IsCompleted)
			{
				log("awaiting existing task completion");
				Refresher.Wait();
			}

			log("starting task");
			StartRefresherTask();
			Cancel = new CancellationTokenSource();
		}
	
		private void StartRefresherTask()
		{
			this.ConstructQueues();
			this.Refresher = Task.Factory.StartNew(()=>ProcessRefreshQueues());
		}

		void ProcessRefreshQueues()
		{
			try
			{
				while (!this.Cancel.IsCancellationRequested)
				{
					Task.Delay(TimeSpan.FromSeconds(1), this.Cancel.Token).Wait();
					//log("sampling");
					foreach (var kv in this.RefreshQueues)
					{
						var q = kv.Value;
						var reque = new Queue<CheckPointTableCell>();
						var since = DateTime.Now - q.LastProcessed;
						//log(string.Format("{0} since {1} was proccessed", since, kv.Key));
						if (since > q.interval)
						{
							q.LastProcessed = DateTime.Now;
							if (q.Any())
							{
								log(string.Format("proccessing {0} items on {1} queue", q.Count, kv.Key));
								while (q.Any())
								{
									var toRefresh = q.Dequeue();
									if (toRefresh != null)
									{
										log(string.Format("refreshing {0}", toRefresh.CheckPoint.Name));
										this.executor(() => toRefresh.RenderCheckpoint(toRefresh.CheckPoint));
										reque.Enqueue(toRefresh);
									}
								}
								while (reque.Any())
									EnqueueCell(reque.Dequeue());
							}
						}
					}
				}
				log("queue proccessing cancelled");
			}
			catch (Exception e)
			{
				log("exception in queue processor");
				log(e.Message);
			}
		}
		void log(string msg)
		{
			if (logging)
				Debug.WriteLine(msg);
		}
	}
}