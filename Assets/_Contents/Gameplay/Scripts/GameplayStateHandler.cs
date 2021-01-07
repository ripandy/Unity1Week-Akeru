using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class GameplayStateHandler : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private List<ActiveStateObject> _statesToHandle;

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _gameplayState.Subscribe(OnStateChanged, token);
        }

        private void OnStateChanged(GameplayStateEnum newState)
        {
            foreach (var stateObject in _statesToHandle)
            {
                stateObject.Activate(stateObject.state == newState);
            }
        }
    }

    [Serializable]
    internal class ActiveStateObject
    {
        public GameplayStateEnum state;
        public List<GameObject> gameObjects;

        public void Activate(bool active)
        {
            gameObjects.ForEach(o => o.SetActive(active));
        }
    }
}