// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public sealed class ScheduleWorkflowItemAction : WorkflowAction
    {
        private readonly WorkflowItem _workflowItem;
        private WorkflowAction _scheduleWorkflowAction;
        internal ScheduleWorkflowItemAction(WorkflowItem workflowItem)
        {
            _workflowItem = workflowItem;
            _scheduleWorkflowAction = Custom(workflowItem.GetScheduleDecisions().ToArray());
        }
        /// <summary>
        /// Cause the item to reschedule after a given timeout.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public ScheduleWorkflowItemAction After(TimeSpan timeout)
        {
           _scheduleWorkflowAction = Custom(_workflowItem.GetRescheduleDecisions(timeout).ToArray());
            return this;
        }
        /// <summary>
        /// Limit the rescheduling. Once the limit is reached, Guflow returns the default WorkflowAction for event.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public WorkflowAction UpTo(Limit limit)
        {
            if (limit.IsExceeded(_workflowItem))
                _scheduleWorkflowAction = _workflowItem.DefaultActionOnLastEvent();
            return this;
        }
        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            return _scheduleWorkflowAction.GetDecisions();
        }
        private bool Equals(ScheduleWorkflowItemAction other)
        {
            return _workflowItem.Equals(other._workflowItem);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ScheduleWorkflowItemAction && Equals((ScheduleWorkflowItemAction)obj);
        }

        public override int GetHashCode()
        {
            return _workflowItem.GetHashCode();
        }
    }
}