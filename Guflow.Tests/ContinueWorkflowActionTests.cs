﻿using System;
using System.Linq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ContinueWorkflowActionTests
    {
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _siblingActivityName = "Sync";
        private const string _siblingActivityVersion = "2.0";

        [Test]
        public void Return_the_scheduling_decision_for_all_child_activities()
        {
            var workflow = new WorkflowWithMultipleChilds();
            var completedWorkflowItem = new ActivityItem(_activityName,_activityVersion,_positionalName,workflow);
            var completedActivityEventsGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res");
            var workflowAction = new ContinueWorkflowAction(completedWorkflowItem, new WorkflowContext(completedActivityEventsGraph));
            
            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0"), new ScheduleActivityDecision("Sync", "2.1") }));
        }

        [Test]
        public void Should_return_the_scheduling_decision_for_all_child_activities()
        {
            var workflow = new WorkflowWithMultipleChilds();
            var activityFailedEvent = CreateFailedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = activityFailedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0"), new ScheduleActivityDecision("Sync", "2.1") }));
        }

        [Test]
        public void Should_return_empty_decision_when_no_schedulable_child_item_found()
        {
            var workflow = new SingleActivityWorkflow();
            var activityFailedEvent = CreateFailedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = activityFailedEvent.Interpret(workflow).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Should_not_schedule_the_child_when_one_of_its_parent_is_not_completed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var activityFailedEvent = CreateFailedActivityEvent(_activityName, _activityVersion, _positionalName);
           
            var decisions = activityFailedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_all_of_its_parents_are_completed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityCompletedEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_failed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityFailedEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_timedout()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityTimedoutEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_when_one_of_its_parent_is_completed_and_one_is_cancelled()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityCancelledEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_timer_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent", childTimer = "child";
            var workflow = new WorkflowWithParentChildTimers(parentTimer,childTimer);
            var timerFiredEvent = CreateTimerFiredEvent(parentTimer);

            var decisions = timerFiredEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(childTimer) }));
        }

        [Test]
        public void Should_return_scheduling_decision_for_child_activity_when_parent_timer_is_fired()
        {
            const string parentTimer = "parent";
            var workflow = new WorkflowWithChildActivity(parentTimer);
            var timerFiredEvent = CreateTimerFiredEvent(parentTimer);

            var decisions = timerFiredEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(_activityName, _activityVersion) }));
        }

        [Test]
        public void Should_return_the_scheduling_decision_for_child_timer_when_parent_activity_is_completed()
        {
            const string timerName = "timer";
            var workflow = new WorkflowWithParentActivityAndChildTimers(timerName);
            var activityFailedEvent = CreateFailedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = activityFailedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(timerName)}));
        }


        private ActivityFailedEvent CreateFailedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityFailedEventGraph(activityName, activityVersion, positionalName, "id", "res","detail");
            return new ActivityFailedEvent(allHistoryEvents.First(), allHistoryEvents);
        }

        private TimerFiredEvent CreateTimerFiredEvent(string timerName)
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(timerName, TimeSpan.FromSeconds(2));
            return new TimerFiredEvent(timerFiredEventGraph.First(),timerFiredEventGraph);
        }


        private class WorkflowWithMultipleChilds : Workflow
        {
            public WorkflowWithMultipleChilds()
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnFailure(Continue);

                AddActivity("Transcode", "2.0").DependsOn(_activityName, _activityVersion, _positionalName);
                AddActivity("Sync", "2.1").DependsOn(_activityName, _activityVersion, _positionalName);
            }
        }

        private class WorkflowWithMultipleParents : Workflow
        {
            public WorkflowWithMultipleParents()
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnFailure(Continue);
                AddActivity(_siblingActivityName, _siblingActivityVersion);
                AddActivity("Transcode", "2.0").DependsOn(_activityName, _activityVersion, _positionalName).DependsOn(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnFailure(Continue);
            }
        }

        private class WorkflowWithParentChildTimers : Workflow
        {
            public WorkflowWithParentChildTimers(string timerName, string childTimer)
            {
                AddTimer(timerName);
                AddTimer(childTimer).DependsOn(timerName);
            }
        }

        private class WorkflowWithChildActivity : Workflow
        {
            public WorkflowWithChildActivity(string timerName)
            {
                AddTimer(timerName);
                AddActivity(_activityName, _activityVersion).DependsOn(timerName);
            }
        }

        private class WorkflowWithParentActivityAndChildTimers : Workflow
        {
            public WorkflowWithParentActivityAndChildTimers(string timerName)
            {
                AddActivity(_activityName, _activityVersion,_positionalName).OnFailure(Continue);
                AddTimer(timerName).DependsOn(_activityName, _activityVersion,_positionalName);
            }
        }
    }
}