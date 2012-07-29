namespace OpenBus.Common.Contracts
{
    public interface IServer
    {
        /// <summary>
        /// Starts the server
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the server
        /// </summary>
        void Stop();
    }
}
