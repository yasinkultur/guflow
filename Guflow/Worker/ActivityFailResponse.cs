﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    /// <summary>
    /// Represent a failed activity response.
    /// </summary>
    public sealed class ActivityFailResponse : ActivityResponse
    {
        private readonly string _reason;
        private readonly string _details;
        private readonly string _taskToken;

        public ActivityFailResponse(string taskToken, string reason, string details)
        {
            Ensure.NotNullAndEmpty(taskToken, "taskToken");
            _reason = reason;
            _details = details;
            _taskToken = taskToken;
        }

        public override async Task SendAsync(IAmazonSimpleWorkflow simpleWorkflow, CancellationToken cancellationToken)
        {
            var request = new RespondActivityTaskFailedRequest()
            {
                TaskToken = _taskToken,
                Reason = _reason,
                Details = _details
            };

            await simpleWorkflow.RespondActivityTaskFailedAsync(request, cancellationToken);
        }

        private bool Equals(ActivityFailResponse other)
        {
            return string.Equals(_taskToken, other._taskToken) && string.Equals(_reason, other._reason) && string.Equals(_details, other._details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ActivityFailResponse && Equals((ActivityFailResponse)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_reason != null ? _reason.GetHashCode() : 0) * 397) ^ (_details != null ? _details.GetHashCode() : 0) ^ _taskToken.GetHashCode() ;
            }
        }

    }
}