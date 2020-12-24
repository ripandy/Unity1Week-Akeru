using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.EventSystem;
using Pyra.VariableSystem;
using UnityEngine;

namespace Pyra.NetworkManagement
{
    public class NetworkManager : MonoBehaviour
    {
        [Header("Model Variables")]
        [SerializeField] private ConnectionStatusVariable connectionStatusVariable;
        [SerializeField] private StringVariable address;
        
        [Header("Network Events")]
        [SerializeField] private StringEvent stringMessageSender;
        [SerializeField] private StringEvent stringMessageReceiver;
        [SerializeField] private ByteArrayEvent byteMessageSender;
        [SerializeField] private ByteArrayEvent byteMessageReceiver;
        [SerializeField] private GameEvent onConnected;
        [SerializeField] private GameEvent onDisconnected;
        
        [Header("Command Events")]
        [SerializeField] private GameEvent onConnectCommand;
        [SerializeField] private GameEvent onDisconnectCommand;

        [Header("Network Handler Instance")]
        [SerializeField] private NetworkHandlerInstance networkHandlerInstance;

        private INetworkHandler _networkHandler;

        protected virtual void Awake()
        {
            _networkHandler = networkHandlerInstance.Instance;
        }

        protected virtual void OnEnable()
        {
            _networkHandler.OnConnected += NetworkHandlerOnOnConnected;
            _networkHandler.OnDisconnected += NetworkHandlerOnOnDisconnected;
            _networkHandler.OnStringMessageReceived += NetworkHandlerOnOnStringMessageReceived;
            _networkHandler.OnByteMessageReceived += NetworkHandlerOnOnByteMessageReceived;
        }
        
        protected virtual void OnDisable()
        {
            _networkHandler.OnConnected -= NetworkHandlerOnOnConnected;
            _networkHandler.OnDisconnected -= NetworkHandlerOnOnDisconnected;
            _networkHandler.OnStringMessageReceived -= NetworkHandlerOnOnStringMessageReceived;
            _networkHandler.OnByteMessageReceived -= NetworkHandlerOnOnByteMessageReceived;
        }

        protected virtual async void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.Run(_networkHandler.Initialize, cancellationToken: token);

            onConnectCommand.Subscribe(() =>
                {
                    if (connectionStatusVariable == ConnectionStatusEnum.Disconnected)
                    {
                        connectionStatusVariable.Value = ConnectionStatusEnum.Connecting;
                        _networkHandler.Connect(address).Forget();
                    }
                })
                .AddTo(token);
            
            onDisconnectCommand.Subscribe(async () =>
                {
                    await _networkHandler.Disconnect();
                    connectionStatusVariable.Value = ConnectionStatusEnum.Disconnected;
                })
                .AddTo(token);

            if (stringMessageSender != null)
                stringMessageSender
                    .Where(_ => connectionStatusVariable == ConnectionStatusEnum.Connected)
                    .Subscribe(_networkHandler.SendString)
                    .AddTo(token);

            if (byteMessageSender != null)
                byteMessageSender
                    .Where(_ => connectionStatusVariable == ConnectionStatusEnum.Connected)
                    .Subscribe(_networkHandler.SendBytes)
                    .AddTo(token);
        }
        
        private void NetworkHandlerOnOnConnected()
        {
            connectionStatusVariable.Value = ConnectionStatusEnum.Connected;
            onConnected.Raise();
        }

        private void NetworkHandlerOnOnDisconnected()
        {
            connectionStatusVariable.Value = ConnectionStatusEnum.Disconnected;
            onDisconnected.Raise();
        }

        private void NetworkHandlerOnOnStringMessageReceived(string message)
        {
            if (stringMessageReceiver != null)
                stringMessageReceiver.Raise(message);
        }
        
        private void NetworkHandlerOnOnByteMessageReceived(byte[] message)
        {
            if (byteMessageReceiver != null)
                byteMessageReceiver.Raise(message);
        }
    }
}