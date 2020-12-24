using UnityEngine;

namespace Pyra.NetworkManagement
{
    public abstract class NetworkHandlerInstance : ScriptableObject
    {
        public abstract INetworkHandler Instance { get; }
    }
    
    public abstract class NetworkHandlerInstance<T> : NetworkHandlerInstance
        where T : INetworkHandler, new()
    {
        private readonly T _instance = new T();
        public override INetworkHandler Instance => _instance;
    }
}