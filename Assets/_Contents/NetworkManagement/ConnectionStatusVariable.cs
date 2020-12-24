using Pyra.VariableSystem;
using UnityEngine;

namespace Pyra.NetworkManagement
{
    public enum ConnectionStatusEnum
    {
        Disconnected,
        Connecting,
        Connected
    }
    
    [CreateAssetMenu(fileName = "ConnectionStatusVariable", menuName = "Pyra/NetworkManagement/ConnectionStatusVariable")]
    public class ConnectionStatusVariable : VariableSystemBase<ConnectionStatusEnum>
    {
    }
}