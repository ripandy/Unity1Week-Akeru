using System;
using Cysharp.Threading.Tasks;

namespace Pyra.NetworkManagement
{
    public interface INetworkHandler
    {
        event Action OnConnected;
        event Action OnDisconnected;
        event Action<string> OnStringMessageReceived;
        event Action<byte[]> OnByteMessageReceived;

        UniTask Initialize();
        UniTaskVoid Connect(string address);
        UniTask Disconnect();
        void SendString(string message);
        void SendBytes(byte[] message);
    }
}