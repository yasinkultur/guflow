﻿using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowFailureFailedEventTests
    {
        private WorkflowFailureFailedEvent _failedEvent;

        [SetUp]
        public void Setup()
        {
            _failedEvent = new WorkflowFailureFailedEvent(HistoryEventFactory.CreateWorkflowFailureFailedEvent("cause"));
        }

        [Test]
        public void Populates_properties_from_swf_event_attributes()
        {
            Assert.That(_failedEvent.Cause,Is.EqualTo("cause"));
        }

        [Test]
        public void By_default_returns_fail_workflow_action_when_interpreted()
        {
            var workflowAction = _failedEvent.Interpret(new EmptyWorkflow());

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow("FAILED_TO_FAIL_WORKFLOW",_failedEvent.Cause)));
        }

        [Test]
        public void Can_return_custom_action_when_interpreted()
        {
            var customAction = new Mock<WorkflowAction>().Object;

            var workflowAction = _failedEvent.Interpret(new WorkflowToReturnCustomAction(customAction));

            Assert.That(workflowAction,Is.EqualTo(customAction));
        }

        private class WorkflowToReturnCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;

            public WorkflowToReturnCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.FailureFailed)]
            public WorkflowAction OnFailureToFailWorkflow(WorkflowFailureFailedEvent @event)
            {
                return _workflowAction;
            }
        }
    }
}