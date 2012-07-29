namespace OpenBus.Common
{
    /// <summary>
    /// Class holding constant values
    /// </summary>
    public static class Constants
    {
        public static class Bus
        {
            public static class Msmq
            {
                public static class Addresses
                {
                    public static readonly string MyQueueAddress = "MyQueueAddress";
                }

                public static class Queues
                {
                    public static readonly string PublishQueue = "PublishQueue";
                    public static readonly string SubscribeQueue = "SubscribeQueue";
                    public static readonly string MyQueue = "MyQueue";
                }
            }
        }
    }
}
