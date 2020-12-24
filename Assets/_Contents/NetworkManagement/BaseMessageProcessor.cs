using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.EventSystem;
using UnityEngine;

namespace Pyra.NetworkManagement
{
    public abstract class BaseMessageProcessor : MonoBehaviour
    {
        [Header("Messages to Process")]
        [SerializeField] private StringEvent receivedStringMessage;
        [SerializeField] private ByteArrayEvent receivedBytesMessage;
        
        protected virtual void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            
            if (receivedStringMessage != null)
                receivedStringMessage.Subscribe(ProcessStringMessage).AddTo(token);

            if (receivedBytesMessage != null)
                receivedBytesMessage.Subscribe(ProcessByteArrayMessage).AddTo(token);
        }

        protected virtual void ProcessStringMessage(string message) {}
        protected virtual void ProcessByteArrayMessage(byte[] bytes) {}
    }
}