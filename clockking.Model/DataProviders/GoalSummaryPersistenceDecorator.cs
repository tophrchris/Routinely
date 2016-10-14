using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace ClockKing.Core
{
    public class GoalSummaryPersistenceDecorator:ICheckPointDataProvider
    {
        private ICheckPointDataProvider Existing { get; set; }
        private IPathProvider Paths { get; set; }

        public GoalSummaryPersistenceDecorator (ICheckPointDataProvider existing,IPathProvider paths)
        {
            this.Existing = existing;
            this.Paths = paths;
        }

        public int LoadOccurrences (Dictionary<string, CheckPoint> checkPoints)
        {
            int loaded = Existing.LoadOccurrences (checkPoints);

            var summaries = checkPoints.Values.Select (cp => cp.AsSummary ());
            PersistGoalSummaries (summaries);

            return loaded;
        }

        public IEnumerable<CheckPoint> ReadCheckPoints ()
        {
            return Existing.ReadCheckPoints ();
        }

        public bool WriteAllOccurrences (IEnumerable<Occurrence> occurrences)
        {
            return Existing.WriteAllOccurrences (occurrences);
        }

        public bool WriteCheckPoints (IEnumerable<CheckPoint> CheckPoints)
        {
            var summaries = CheckPoints.Select (cp => cp.AsSummary ());
            PersistGoalSummaries (summaries);
            return Existing.WriteCheckPoints (CheckPoints);
        }

        public void WriteOccurrence (Occurrence toSave)
        {
            Existing.WriteOccurrence (toSave);
        }

        public void PersistGoalSummaries (IEnumerable<GoalSummary> summaries)
        {
            string json = JsonConvert.SerializeObject (summaries, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            var path = Paths.GetSummariesFileName ();

            Paths.WriteAllText (path, json);
        }
    }
}
