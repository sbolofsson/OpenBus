using System;
using System.Messaging;
using log4net;

namespace OpenBus.Common.Queues
{
    /// <summary>
    /// Class for working with MSMQ queues
    /// </summary>
    public static class QueueManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(QueueManager));

        /// <summary>
        /// Create a queue if it does not exist.
        /// </summary>
        /// <param name="queueName"></param>
        public static void EnsureQueueExists(string queueName)
        {
            if (String.IsNullOrEmpty(queueName))
            {
                Logger.Error("QueueManager: Queue name was null/empty. Could not check if it exists.");
                return;
            }

            if (!MessageQueue.Exists(queueName))
            {
                try
                {
                    MessageQueue.Create(queueName, true);
                    Logger.Info(String.Format("QueueManager: Queue '{0}' was created since it did not exist.", queueName));
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("QueueManager: Could not create queue '{0}'.", queueName), ex);
                }
            }
        }

        /// <summary>
        /// Deletes a queue if it exists.
        /// </summary>
        /// <param name="queueName"></param>
        public static void DeleteQueue(string queueName)
        {
            if (String.IsNullOrEmpty(queueName))
            {
                Logger.Error("QueueManager: Queue name was null/empty. Could not check if it exists.");
                return;
            }

            if (MessageQueue.Exists(queueName))
            {
                try
                {
                    MessageQueue.Delete(queueName);
                    Logger.Debug(String.Format("QueueManager: Queue '{0}' was deleted.", queueName));
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(String.Format("QueueManager: Queue '{0}' could not be deleted.", queueName), ex);
                }
            }
        }
    }
}
