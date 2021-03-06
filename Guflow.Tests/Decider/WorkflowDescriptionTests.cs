﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Configuration;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowDescriptionTests
    {
        [Test]
        public void Throws_exception_when_workflow_description_is_not_supplied()
        {
            Assert.Throws<WorkflowDescriptionMissingException>(() => WorkflowDescription.FindOn<WorkflowWithoutAttribute>());
        }

        [Test]
        public void Workflow_description_has_only_version_property_set()
        {
            var d = WorkflowDescription.FindOn<WorkflowWithOnlyVersion>();
           
            Assert.That(d.Version, Is.EqualTo("1.0"));
            Assert.That(d.Name, Is.EqualTo(nameof(WorkflowWithOnlyVersion)));
            Assert.IsNull(d.Description);
            Assert.IsNull(d.DefaultTaskListName);
            Assert.IsNull(d.DefaultChildPolicy);
            Assert.IsNull(d.DefaultLambdaRole);
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(0));
            Assert.IsNull(d.DefaultExecutionStartToCloseTimeout);
            Assert.IsNull(d.DefaultTaskStartToCloseTimeout);
        }

        [Test]
        public void Workflow_description_attribute_has_property_set()
        {
            var d = WorkflowDescription.FindOn<WorkflowWithTimeoutSet>();
           
            Assert.That(d.DefaultChildPolicy, Is.EqualTo("policy"));
            Assert.That(d.DefaultLambdaRole, Is.EqualTo("role"));
            Assert.That(d.DefaultTaskListName, Is.EqualTo("tlist"));
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(1));
            Assert.That(d.Description, Is.EqualTo("desc"));
            Assert.That(d.Name, Is.EqualTo("test"));
            Assert.That(d.Version , Is.EqualTo("1.0"));
            Assert.That(d.DefaultExecutionStartToCloseTimeout,Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(d.DefaultTaskStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(9)));
        }

        [Test]
        public void Throws_exception_version_is_empty()
        {
            Assert.Throws<ArgumentException>(() => WorkflowDescription.FindOn<WorkflowWithEmptyVersion>());
        }

        [Test]
        public void Throws_exception_when_type_does_not_derive_from_workflow_class()
        {
           Assert.Throws<NonWorkflowTypeException>(()=> WorkflowDescription.FindOn(typeof (NonWorkflow)));
        }

        [Test]
        public void Invalid_arguments_tests()
        {
            Assert.Throws<ArgumentNullException>(() => WorkflowDescription.FindOn(null));
        }

        [Test]
        public void Read_workflow_description_from_factory_method_when_provided()
        {
            var d = WorkflowDescription.FindOn<WorkflowWithDescriptionFactoryMethod>();

            Assert.That(d.Name, Is.EqualTo("test"));
            Assert.That(d.Version, Is.EqualTo("1.0"));
            Assert.That(d.Description, Is.EqualTo("desc"));
            Assert.That(d.DefaultTaskListName, Is.EqualTo("tname"));
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(1));
            Assert.That(d.DefaultExecutionStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(d.DefaultTaskStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(9)));
            Assert.That(d.DefaultChildPolicy, Is.EqualTo("policy"));
            Assert.That(d.DefaultLambdaRole, Is.EqualTo("lambdarole"));
            
        }

        [Test]
        public void Factory_method_has_priority_over_attributes()
        {
            var d = WorkflowDescription.FindOn<WorkflowWithDescriptionFactoryMethod>();

            Assert.That(d.Name, Is.EqualTo("test"));
            Assert.That(d.Version, Is.EqualTo("1.0"));
            Assert.That(d.Description, Is.EqualTo("desc"));
            Assert.That(d.DefaultTaskListName, Is.EqualTo("tname"));
            Assert.That(d.DefaultTaskPriority, Is.EqualTo(1));
            Assert.That(d.DefaultExecutionStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(d.DefaultTaskStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(9)));
            Assert.That(d.DefaultChildPolicy, Is.EqualTo("policy"));
            Assert.That(d.DefaultLambdaRole, Is.EqualTo("lambdarole"));

        }

        private class WorkflowWithoutAttribute : Workflow
        {
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithOnlyVersion : Workflow
        {
            
        }

        [WorkflowDescription("1.0", Name ="test", DefaultExecutionStartToCloseTimeoutInSeconds = 10,
            DefaultTaskStartToCloseTimeoutInSeconds = 9, DefaultChildPolicy = "policy", DefaultTaskListName = "tlist",
            DefaultLambdaRole = "role", DefaultTaskPriority = 1, Description = "desc")]
        private class WorkflowWithTimeoutSet : Workflow
        {

        }

        [WorkflowDescription("")]
        private class WorkflowWithEmptyVersion : Workflow
        {

        }
        private class NonWorkflow
        {
            
        }


        [WorkflowDescription("2.0", Name = "test1", DefaultExecutionStartToCloseTimeoutInSeconds = 10,
            DefaultTaskStartToCloseTimeoutInSeconds = 9, DefaultChildPolicy = "policy1", DefaultTaskListName = "tlist34",
            DefaultLambdaRole = "role1", DefaultTaskPriority = 10, Description = "desc323")]
        private class WorkflowWithDescriptionFactoryMethod : Workflow
        {
            private static  WorkflowDescription WorkflowDescription => new WorkflowDescription("1.0")
            {
                Name = "test",
                Description = "desc",
                DefaultTaskListName = "tname",
                DefaultExecutionStartToCloseTimeout = TimeSpan.FromSeconds(10),
                DefaultTaskStartToCloseTimeout = TimeSpan.FromSeconds(9),
                DefaultChildPolicy= "policy",
                DefaultLambdaRole ="lambdarole", 
                DefaultTaskPriority =1
            };
        }

        private class WorkflowWithFactoryMethodAndAttribute : Workflow
        {
            public static WorkflowDescription WorkflowDescription => new WorkflowDescription("1.0")
            {
                Name = "test",
                Description = "desc",
                DefaultTaskListName = "tname",
                DefaultExecutionStartToCloseTimeout = TimeSpan.FromSeconds(10),
                DefaultTaskStartToCloseTimeout = TimeSpan.FromSeconds(9),
                DefaultChildPolicy = "policy",
                DefaultLambdaRole = "lambdarole",
                DefaultTaskPriority = 1
            };
        }
    }
}