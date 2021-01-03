using System;
using System.Linq;
using Pyra;
using Pyra.Collection;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public enum CubeSide
    {
        Front,
        Back,
        Up,
        Down,
        Left,
        Right
    }
    
    [CreateAssetMenu(fileName = "CubeSideCollection", menuName = MenuHelper.DefaultCollectionMenu + "CubeSideCollection")]
    public class CubeSideCollection : Collection<CubeSide, int>
    {
        public CubeSide onFloor = CubeSide.Down;

        public bool IsIntact(CubeSide cubeSide) => this[cubeSide] < 0;
        public bool IsCompleted => !ContainsValue(-1);
        public int IntactCount => this.Count(pair => pair.Value < 0);
        
        public void ResetSides()
        {
            foreach (CubeSide side in Enum.GetValues(typeof(CubeSide)))
            {
                if (!ContainsKey(side))
                    Add(side, -1);
                this[side] = -1;
            }
            
            onFloor = CubeSide.Down;
        }
    }
}