﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class WorkflowTask
    {
        private readonly DecisionTask _decisionTask;
        private readonly Func<HostedWorkflows, IEnumerable<WorkflowDecision>> _actionToExecute;
        private WorkflowTask(DecisionTask decisionTask)
        {
            _decisionTask = decisionTask;
            _actionToExecute = ExecuteTasks;
        }

        private WorkflowTask()
        {
            _actionToExecute = (w) => Enumerable.Empty<WorkflowDecision>();
        }

        public static readonly WorkflowTask Empty = new WorkflowTask();

        public static WorkflowTask CreateFor(DecisionTask decisionTask)
        {
            return new WorkflowTask(decisionTask);
        }

        public IEnumerable<WorkflowDecision> ExecuteFor(HostedWorkflows hostedWorkflows)
        {
            return _actionToExecute(hostedWorkflows);
        }
        public async Task SendDecisions(IEnumerable<WorkflowDecision> decisions, Domain domain, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this == Empty)
                return;

            var request = CreateResponseRequest(decisions);
            await domain.RespondDecisionTaskCompletedAsync(request, cancellationToken);
        }
        private IEnumerable<WorkflowDecision> ExecuteTasks(HostedWorkflows hostedWorkflows)
        {
            var workflowType = _decisionTask.WorkflowType;
            var workflow = hostedWorkflows.FindBy(workflowType.Name, workflowType.Version);
            var historyEvents = new WorkflowHistoryEvents(_decisionTask.Events, _decisionTask.StartedEventId, _decisionTask.PreviousStartedEventId);

            using (var execution = workflow.NewExecutionFor(historyEvents))
            {
                return execution.Execute().ToArray();
            }
        }

        private RespondDecisionTaskCompletedRequest CreateResponseRequest(IEnumerable<WorkflowDecision> decisions)
        {
            return new RespondDecisionTaskCompletedRequest()
            {
                TaskToken = _decisionTask.TaskToken,
                Decisions = decisions.Select(s => s.Decision()).ToList()
            };
        }
    }
}