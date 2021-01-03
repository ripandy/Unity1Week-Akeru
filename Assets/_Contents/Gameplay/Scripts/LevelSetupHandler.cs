using Pyra.Collection;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class LevelSetupHandler : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private Grid _activeGrid;
        [SerializeField] private IntVariable _cubeIndex;
        [SerializeField] private IntVariable _activeLevel;
        [SerializeField] private StringCollection _levelList;
        [SerializeField] private StringVariable _levelName;

        private void Awake()
        {
            _activeGrid.Initialize($"{_levelList[_activeLevel]}");
            // _activeGrid.LoadFromJson("Default.json");
            // _activeGrid.Initialize();
            // _activeGrid.SaveToJson("10.json");
            
            _cubeIndex.Value = _activeGrid.dropPoint;
            _levelName.Value = _activeGrid.title;
        }

        private void Start()
        {
            _gameplayState.Value = GameplayStateEnum.Setup;
        }
    }
}