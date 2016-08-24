using System;
using System.Collections.Generic;

namespace ClockKing.Core
{

    public interface iCheckpointCommandController
    {
        event EventHandler<CheckPointDataChangedEventArgs> CheckPointDataChanged;

        bool CheckPointExists (string checkPointName);
        bool CheckPointExists (Guid checkPointGuid);
        CheckPoint AddNewCheckPoint(string title, TimeSpan target,string emoji,string category="");
        CheckPoint AddNewCheckPoint (CheckPoint toAdd);
        bool RemoveCheckpoint(CheckPoint checkPoint);
        Occurrence SkipCheckpoint (CheckPoint checkPoint);
        Occurrence AddOccurrenceToCheckPoint(string checkPointName,int mins);
        Occurrence AddOccurrenceToCheckPoint(CheckPoint checkPoint,int mins);
        Occurrence AddOccurrenceToCheckPoint(CheckPoint checkPoint,DateTime when);
        void ResaveCheckpoints();
        void RewriteOccurrences();
      

        //this could be its own IDialogNavigation interface?
        void PresentChoices(string Title, string Instructions, IEnumerable<ModalChoice> choices);

        void PresentConfirmationDialog(
            Action handler,
            string Title="Are you sure?",
            string Message="Confirm Delete:",
            string yes="Yes",
            string no="Nevermind",
            bool YesIsDestructive=true);

        void NavigateToDialog(iNavigatableDialog dialog);
        void PresentHistoricOccurrenceDialogFor(CheckPoint checkpoint);
        void PresentEditDialogFor(CheckPoint checkpoint);
        void InjectEditDialogIntoExistingDialogFor(CheckPoint checkpoint,iNavigatableDialog existing);
        void PresentRelativeTargetDialogForCheckpoint (CheckPoint checkpoint, iNavigatableDialog existing);
        void PresentScheduledTargetDialogForCheckpoint(CheckPoint checkpoint,iNavigatableDialog dialog);
        void PresentCheckPointActionsFor(CheckPoint checkpoint, iNavigatableDialog existing);

    }

    public interface iNavigatableDialog
    {
        void ResetNavigation(bool refreshData=false);
    }

    public interface iModalChoices
    {
        string Title { get; set; }
        string Instructions { get; set; }
        IEnumerable<ModalChoice> Choices{ get; set; }
        void Display();
    }

    public class ModalChoice
    {
        public string Label {get;set;}
        public Action Handler { get; set; }
        public bool Destructive {get;set;}
        public bool Cancel {get;set;}
    }

    public class CheckPointDataChangedEventArgs
    {
        public string Entity{get;set;}
        public ActionType ActionOccurred{get;set;}
        public ResultType Result{ get; set;}=ResultType.Success;
        public bool RespondToModelChanges{get;set;}=true;
        public bool ConditionallyRefreshData{get;set;}=false;
    }
    public enum ActionType
    {
        Added,
        Updated,
        Deleted,
        Written
    }
    public enum ResultType
    {
        Success,
        Failure
    }
}

