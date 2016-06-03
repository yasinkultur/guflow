﻿using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow
{
    internal abstract class WorkflowItem
    {
        private readonly IWorkflowItems _workflowItems;
        private readonly HashSet<WorkflowItem> _parentItems = new HashSet<WorkflowItem>();
        protected readonly Identity Identity;
        protected Func<TimerCancelledEvent, WorkflowAction> OnTimerCancelledAction; 
        protected WorkflowItem(Identity identity, IWorkflowItems workflowItems)
        {
            Identity = identity;
            _workflowItems = workflowItems;
            OnTimerCancelledAction = c=>WorkflowAction.CancelWorkflow("TIMER_CANCELLED");
        }

        public IEnumerable<IActivityItem> ParentActivities { get { return _parentItems.OfType<IActivityItem>(); } }

        public IEnumerable<ITimerItem> ParentTimers { get { return _parentItems.OfType<ITimerItem>(); } }

        public string Name
        {
            get { return Identity.Name; }
        }

        internal bool HasNoParents()
        {
            return _parentItems.Count == 0;
        }
        internal bool IsChildOf(WorkflowItem workflowItem)
        {
            return _parentItems.Contains(workflowItem);
        }

        internal IEnumerable<WorkflowItem> GetChildlern()
        {
            return _workflowItems.GetChildernOf(this);
        }

        internal abstract WorkflowDecision GetScheduleDecision();
        internal WorkflowDecision GetRescheduleDecision(TimeSpan afterTimeout)
        {
            return new ScheduleTimerDecision(Identity,afterTimeout,true);
        }
        internal abstract WorkflowDecision GetCancelDecision();

        internal bool Has(AwsIdentity identity)
        {
            return Identity.Id == identity;
        }
        internal bool Has(Identity identity)
        {
            return Identity.Equals(identity);
        }
        internal abstract WorkflowAction TimerFired(TimerFiredEvent timerFiredEvent);
        internal WorkflowAction TimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            return OnTimerCancelledAction(timerCancelledEvent);
        }
        public override bool Equals(object other)
        {
            var otherItem = other as WorkflowItem;
            if (otherItem == null)
                return false;
            return Identity.Equals(otherItem.Identity);
        }
        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        internal bool AllParentsAreProcessed()
        {
            return _parentItems.All(p => p.IsProcessed());
        }

        protected abstract bool IsProcessed();

        protected void AddParent(Identity identity)
        {
            var parentItem = _workflowItems.Find(identity);
            if (parentItem == null)
                throw new ParentItemMissingException(string.Format(Resources.Schedulable_item_missing, identity));
            _parentItems.Add(parentItem);
        }

        protected IWorkflowHistoryEvents WorkflowHistoryEvents
        {
            get { return _workflowItems.CurrentHistoryEvents; }
        }
      
    }
}
