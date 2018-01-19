﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    public interface IActivityTaskPoller
    {
        void PollForNewTask();

        void StopPolling();
    }
}