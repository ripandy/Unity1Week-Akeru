using Pyra;
using Pyra.Collection;
using Pyra.Utilities;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public enum GridState
    {
        None = -1,
        Empty,
        Fill
    }

    public struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    
    [CreateAssetMenu(fileName = "NewGrid", menuName = MenuHelper.DefaultCollectionMenu + "Grid")]
    public class Grid : Collection<GridState>
    {
        public int width;
        public int height;

        public GridState this[int x, int y] => this[y * width + x];

        public void Initialize()
        {
            Clear();
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    Add(GridState.Empty);
                }
            }
        }
        
        public Point ToGrid(int index) => new Point(index % width, Mathf.FloorToInt((float) index / width));
        public int ToIndex(int x, int y) => y * width + x;
        public int ToIndex(Point point) => ToIndex(point.x, point.y);
        
        protected virtual string BasePath => Application.dataPath;
        protected virtual string JsonFilename => name + ".json";
        public bool JsonFileExist => ExternalJsonHandler.IsJsonExist(BasePath, JsonFilename);
        
        public virtual void SaveToJson()
        {
            ExternalJsonHandler.SaveToJson(this, BasePath, JsonFilename);
        }

        public virtual void LoadFromJson()
        {
            if (JsonFileExist)
                ExternalJsonHandler.LoadFromJson(this, BasePath, JsonFilename);
            else
                SaveToJson();
        }
    }
}