﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ScheduleActivityDecisionTests
    {
        private Identity _activityIdentity;
        private ScheduleActivityDecision _scheduleActivityDecision;

        [SetUp]
        public void Setup()
        {
            _activityIdentity = Identity.New("Download", "1.0", "First");
            _scheduleActivityDecision = new ScheduleActivityDecision(_activityIdentity);
        }
        [Test]
        public void Equality_tests()
        {
            Assert.True(new ScheduleActivityDecision(Identity.New("Download","1.0","First")).Equals(new ScheduleActivityDecision(Identity.New("Download","1.0","First"))));
            Assert.False(new ScheduleActivityDecision(Identity.New("Download", "1.0", "First")).Equals(new ScheduleActivityDecision(Identity.New("Download", "2.0", "First"))));
        }

        [Test]
        public void Should_return_aws_decision_to_schedule_the_activity()
        {
            var swfDecision = _scheduleActivityDecision.Decision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.ScheduleActivityTask));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ActivityId,Is.EqualTo(_activityIdentity.Id.ToString()));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ActivityType.Name,Is.EqualTo("Download"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ActivityType.Version, Is.EqualTo("1.0"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.Control.FromJson<ActivityScheduleData>().PN, Is.EqualTo("First"));
        }

        [Test]
        public void By_default_aws_activity_decision_has_null_timeouts()
        {
            var swfDecision = _scheduleActivityDecision.Decision();

            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.HeartbeatTimeout,Is.Null);
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ScheduleToCloseTimeout, Is.Null);
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ScheduleToStartTimeout, Is.Null);
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.StartToCloseTimeout, Is.Null);
        }

        [Test]
        public void Can_set_the_optional_attribute_on_aws_decision_to_finit_limit()
        {
            var timeouts = new ActivityTimeouts();
            timeouts.HeartbeatTimeout = TimeSpan.FromSeconds(20);
            timeouts.ScheduleToCloseTimeout = TimeSpan.FromSeconds(30);
            timeouts.ScheduleToStartTimeout = TimeSpan.FromSeconds(40);
            timeouts.StartToCloseTimeout = TimeSpan.FromSeconds(50);
            _scheduleActivityDecision.Timeouts = timeouts;

            var swfDecision = _scheduleActivityDecision.Decision();

            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.HeartbeatTimeout,Is.EqualTo("20"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ScheduleToCloseTimeout, Is.EqualTo("30"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ScheduleToStartTimeout, Is.EqualTo("40"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.StartToCloseTimeout, Is.EqualTo("50"));
        }

        [Test]
        public void Can_set_the_timeouts_on_aws_decision_to_maximum_limit()
        {
            var timeouts = new ActivityTimeouts();
            timeouts.HeartbeatTimeout = TimeSpan.MaxValue;
            timeouts.ScheduleToCloseTimeout = TimeSpan.MaxValue;
            timeouts.ScheduleToStartTimeout = TimeSpan.MaxValue;
            timeouts.StartToCloseTimeout = TimeSpan.MaxValue;
            _scheduleActivityDecision.Timeouts = timeouts;

            var swfDecision = _scheduleActivityDecision.Decision();

            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.HeartbeatTimeout, Is.EqualTo("NONE"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ScheduleToCloseTimeout, Is.EqualTo("NONE"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.ScheduleToStartTimeout, Is.EqualTo("NONE"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.StartToCloseTimeout, Is.EqualTo("NONE"));
        }

        [Test]
        public void  By_default_optional_attributes_of_aws_activity_decision_are_null()
        {
            var swfDecision = _scheduleActivityDecision.Decision();

            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.Input,Is.Null);
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.TaskList, Is.Null);
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.TaskPriority, Is.Null);
        }

        [Test]
        public void Can_set_the_optional_attributes_of_aws_activity_decision()
        {
            _scheduleActivityDecision.UseInputFunc(()=>"input");
            _scheduleActivityDecision.TaskList = "list";
            _scheduleActivityDecision.TaskPriority = 20;
            var swfDecision = _scheduleActivityDecision.Decision();

            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.Input, Is.EqualTo("input"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.TaskList.Name, Is.EqualTo("list"));
            Assert.That(swfDecision.ScheduleActivityTaskDecisionAttributes.TaskPriority, Is.EqualTo("20"));
        }
    }
}