﻿using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class WorkflowStartedEvent : WorkflowEvent
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly WorkflowExecutionStartedEventAttributes _workflowStartedAttributes;

        public WorkflowStartedEvent(HistoryEvent workflowStartedEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
            _workflowStartedAttributes = workflowStartedEvent.WorkflowExecutionStartedEventAttributes;
        }

        public string ChildPolicy { get { return _workflowStartedAttributes.ChildPolicy.Value; } }
        public string ContinuedExecutionRunId { get { return _workflowStartedAttributes.ContinuedExecutionRunId; } }
        public TimeSpan ExecutionStartToCloseTimeout { get { return TimeSpan.FromSeconds(Convert.ToInt32(_workflowStartedAttributes.ExecutionStartToCloseTimeout)); } }
        public string Input { get { return _workflowStartedAttributes.Input; } }
        public string LambdaRole { get { return _workflowStartedAttributes.LambdaRole; } }
        public long ParentInitiatedEventId { get { return _workflowStartedAttributes.ParentInitiatedEventId; } }

        public string ParentWorkflowRunId
        {
            get
            {
                if (_workflowStartedAttributes.ParentWorkflowExecution != null)
                    return _workflowStartedAttributes.ParentWorkflowExecution.RunId;
                return string.Empty;
            }
        }
        public string ParentWorkflowId
        {
            get
            {
                if (_workflowStartedAttributes.ParentWorkflowExecution != null)
                    return _workflowStartedAttributes.ParentWorkflowExecution.WorkflowId;
                return string.Empty;
            }
        }
        public IEnumerable<string> TagList
        {
            get { return _workflowStartedAttributes.TagList; }
        }
        public string TaskList
        {
            get { return _workflowStartedAttributes.TaskList.Name; }
        }

        public string TaskPriority
        {
            get { return _workflowStartedAttributes.TaskPriority; }
        }

        public TimeSpan TaskStartToCloseTimeout
        {
            get { return TimeSpan.FromSeconds(Convert.ToInt32(_workflowStartedAttributes.TaskStartToCloseTimeout)); }
        }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowStarted(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { return new WorkflowContext(_allHistoryEvents); }
        }

        internal override SchedulableItem FindSchedulableItemIn(HashSet<SchedulableItem> allSchedulableItems)
        {
            throw new System.NotImplementedException();
        }
    }
}
