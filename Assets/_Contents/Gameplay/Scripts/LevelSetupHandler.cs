using Pyra.Collection;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class LevelSetupHandler : MonoBehaviour
    {
        [SerializeField] private Grid _activeGrid;
        [SerializeField] private IntVariable _cubeIndex;
        [SerializeField] private IntVariable _activeLevel;
        [SerializeField] private StringCollection _levelList;
        [SerializeField] private StringVariable _levelName;

        private void Awake()
        {
            _activeGrid.Initialize($"{_levelList[_activeLevel]}.json");
            _cubeIndex.Value = _activeGrid.dropPoint;
            _levelName.Value = _activeGrid.title;
        }
    }
}