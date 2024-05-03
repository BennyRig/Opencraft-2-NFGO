namespace Unity.Netcode.gameobjects.EditorTests
{
    internal class NopMessageSender : INetworkMessageSender
    {
        public void Send(ulong clientId, NetworkDelivery delivery, FastBufferWriter batchData)
        {
        }
    }
}
