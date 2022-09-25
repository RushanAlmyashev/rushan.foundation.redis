using System;

namespace Rushan.Foundation.Redis.Logger
{
    internal class EmptyLogger : ILogger
    {
        public void Debug(string message)
        {

        }

        public void Error(string message)
        {

        }

        public void Error(Exception ex, string message = null)
        {

        }

        public void Info(string message)
        {

        }

        public void Warn(string message)
        {

        }


        public void Trace(string message)
        {

        }
    }
}
