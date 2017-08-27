﻿using System;
using Guflow.Properties;

namespace Guflow
{
    public class Log
    {
        private static readonly ILog NullLog = new NullLog();
        public static Func<Type, ILog> NullLogger = t => NullLog;
        public static Func<Type, ILog> ConsoleLogger = t => new ConsoleLog(t.Name);
        private static Func<Type, ILog> _logFactory = NullLogger;
      
        public static ILog GetLogger<T>()
        {
            var log = _logFactory(typeof(T));
            if(log==null)
                throw new LogException(Resources.Null_logger_is_returned);

            return log;
        }

        public static void Register(Func<Type, ILog> logFactoryFunc)
        {
            Ensure.NotNull(logFactoryFunc, "logFactoryFunc");
            _logFactory = logFactoryFunc;
        }
    }
}