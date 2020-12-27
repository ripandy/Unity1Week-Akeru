using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public enum GameplayStateEnum
    {
        Setup,
        Drop,
        Fill,
        CubeComplete,
        Win,
        Lose
    }

    [CreateAssetMenu(fileName = "GameplayState", menuName = "Pyra/Variables/GameplayState")]
    public class GameplayStateVariable : VariableSystemBase<GameplayStateEnum>
    {
    }
}