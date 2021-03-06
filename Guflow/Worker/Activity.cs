﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Guflow.Properties;

namespace Guflow.Worker
{
    /// <summary>
    /// Represent an activity.
    /// </summary>
    public abstract class Activity
    {
        private readonly ActivityExecutionMethod _executionMethod;
        private IHeartbeatSwfApi _heartbeatApi;
        private IErrorHandler _errorHandler = ErrorHandler.NotHandled;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected bool FailOnException;
        protected readonly ActivityHeartbeat Hearbeat = new ActivityHeartbeat();
        private string _currentTaskToken = string.Empty;
        protected Activity()
        {
            _executionMethod = new ActivityExecutionMethod(GetType());
            ConfigureHeartbeat();
            FailOnException = true;
        }

        internal void SetSwfApi(IHeartbeatSwfApi heartbeatApi)
        {
            _heartbeatApi = heartbeatApi;
        }
        internal void SetErrorHandler(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        internal async Task<ActivityResponse> ExecuteAsync(ActivityArgs activityArgs)
        {
            try
            {
                _currentTaskToken = activityArgs.TaskToken;
                Hearbeat.SetFallbackErrorHandler(_errorHandler);
                Hearbeat.StartHeartbeatIfEnabled(_heartbeatApi, activityArgs.TaskToken);
                var retryableFunc = new RetryableFunc(_errorHandler);
                return await retryableFunc.ExecuteAsync(()=>ExecuteActivityMethod(activityArgs), Defer);
            }
            finally
            {
                Hearbeat.StopHeartbeat();
            }
        }

        private async Task<ActivityResponse> ExecuteActivityMethod(ActivityArgs activityArgs)
        {
            try
            {
                return await _executionMethod.ExecuteAsync(this, activityArgs, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                return Cancel(exception.Message);
            }
            catch (Exception exception)
            {
                if (FailOnException)
                    return Fail(exception.GetType().Name, exception.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Successfully complete the activity with given result.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected ActivityResponse Complete(string result)
        {
            return new ActivityCompleteResponse(_currentTaskToken, result);
        }
        /// <summary>
        /// Cancel the activity with given details.
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        protected ActivityResponse Cancel(string details)
        {
            return new ActivityCancelResponse(_currentTaskToken, details);
        }
        /// <summary>
        /// Fails the activity with given reason and details.
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        protected ActivityResponse Fail(string reason,string details)
        {
            return new ActivityFailResponse(_currentTaskToken, reason, details);
        }
        /// <summary>
        /// Do not send any response to Amazon SWF.It is useful when you do not have response to return Amazon SWF and possibly a human intervention is need to send the response
        /// back to Amazon SWF. Later on you can send the reponse by using ActivityResponse's derived classes.
        /// </summary>
        protected ActivityResponse Defer => ActivityResponse.Defer;

        private void ConfigureHeartbeat()
        {
            var heartbeatAttribute = GetType().GetCustomAttribute<EnableHeartbeatAttribute>();
            if(heartbeatAttribute ==null)
                return;
          
            Hearbeat.Enable(TimeSpan.FromMilliseconds(HeartbeatInterval(heartbeatAttribute)));
            Hearbeat.CancellationRequested += (s, e) => _cancellationTokenSource.Cancel();
        }

        private ulong HeartbeatInterval(EnableHeartbeatAttribute attribute)
        {
            ulong intervalMillisec = 0;
            if (attribute.IntervalInMilliSeconds > 0)
                intervalMillisec = attribute.IntervalInMilliSeconds;

            if (intervalMillisec == 0)
            {
                var description = ActivityDescription.FindOn(GetType());
                intervalMillisec = description.DefaultHeartbeatTimeout.HasValue
                    ? (uint)description.DefaultHeartbeatTimeout.Value.TotalMilliseconds
                    : 0;
            }
            if (intervalMillisec == 0)
                throw new ActivityConfigurationException(
                    string.Format(Resources.Heartbeat_is_enabled_but_interval_is_missing, GetType().Name));
            return intervalMillisec;
        }
    }
}