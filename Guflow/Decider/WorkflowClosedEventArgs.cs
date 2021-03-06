﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;

namespace Guflow.Decider
{
    public sealed class WorkflowClosedEventArgs : EventArgs
    {
        public WorkflowClosedEventArgs(string workflowId, string workflowRunId)
        {
            WorkflowId = workflowId;
            WorkflowRunId = workflowRunId;
        }

        public string WorkflowRunId { get; private set; }
        public string WorkflowId { get; private set; }
    }
}