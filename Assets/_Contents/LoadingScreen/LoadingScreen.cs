using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.Utilities;
using Pyra.VariableSystem;
using UnityEngine;

namespace Pyra.LoadingScreen
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private FloatVariable progress;

        private void Start()
        {
            progress.Subscribe(value => value.Orange($"{GetType().Name}: Loading Progress")).AddTo(this.GetCancellationTokenOnDestroy());
        }
    }
}