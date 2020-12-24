using Pyra.VariableSystem;
using UnityEngine;

namespace Pyra.ApplicationStateManagement
{
    public enum ApplicationStateEnum
    {
        Boot,
        MainMenu,
        GamePlay
    }

    [CreateAssetMenu(fileName = "ApplicationState", menuName = "Pyra/Variables/ApplicationState")]
    public class ApplicationStateVariable : VariableSystemBase<ApplicationStateEnum>
    {
    }
}