using System;
namespace ClockKing.Core
{
    public class GoalSummary
    {
        public string Emoji { get; set; }
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public DateTime NextTargetTime { get; set; }
        public DateTime MostRecentOccurrence { get; set; }
        public TimeSpan AverageCompletionTime { get; set; }
        public string Summary { get; set; }
        public bool Enabled { get; set; }
        public bool Active { get; set; }
        public CompletionStatus Status { get; set; }
    }
    public enum CompletionStatus
    {
        Incomplete=0,
        Completed,
        Missed,
        Skippped
    }
    public static class GoalSummaryConverter
    {
        public static GoalSummary AsSummary (this CheckPoint existing)
        {
            var ns = new GoalSummary ();
            var ev = new CheckPointEvaluator (existing);
            ns.Guid = existing.UniqueIdentifier;
            ns.Name = existing.Name;
            ns.Emoji = existing.Emoji;
            ns.Active = existing.IsActive;
            ns.Enabled = existing.IsEnabled;
            ns.Summary = ev.Evaluation;
            ns.MostRecentOccurrence = existing.MostRecentOccurrenceTimeStamp ();
            ns.AverageCompletionTime = existing.AverageCompletionTime;

            //this should really be alt. target aware, but fine for now
            ns.NextTargetTime = existing.TargetTimeToday;

            ns.Status = CompletionStatus.Incomplete;
            if (existing.CompletedToday)
                ns.Status = CompletionStatus.Completed;
            else
                if(existing.TargetTimeToday < DateTime.Now)
                    ns.Status = CompletionStatus.Missed;

            if (existing.IsSkipped)
                ns.Status = CompletionStatus.Skippped;
          

            return ns;
        }
    }
}
