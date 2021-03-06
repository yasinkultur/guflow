﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple = false,Inherited = true)]
    public class WorkflowEventAttribute : Attribute
    {
        private readonly EventName _eventName;

        public WorkflowEventAttribute(EventName eventName)
        {
            _eventName = eventName;
        }

        internal bool IsFor(EventName eventName)
        {
            return _eventName == eventName;
        }
    }
}