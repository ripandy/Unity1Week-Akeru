using System.Linq;
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
        Fresh,
        Filled
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
        public int width = 10;
        public int height = 10;

        public int dropPoint = 55;

        public string title; 

        public GridState this[int x, int y] => this[y * width + x];

        public void Initialize(string filename)
        {
            LoadFromJson(filename);
            Initialize();
        }

        public void Initialize()
        {
            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    var idx = ToIndex(i, j);
                    if (Count <= idx)
                        Add(GridState.Empty);
                    if (this[idx] != GridState.None)
                        this[idx] = GridState.Empty;
                }
            }
        }
        
        public Point ToGrid(int index) => new Point(index % width, Mathf.FloorToInt((float) index / width));
        public int ToIndex(int x, int y) => y * width + x;
        public int ToIndex(Point point) => ToIndex(point.x, point.y);

        public bool Completed => this.All(state => state != GridState.Empty);

        #if UNITY_EDITOR
        private string BasePath => Application.dataPath;
        #else
        private string BasePath => Application.persistentDataPath;
        #endif
        
        private string JsonFilename => name + ".json";

        public void SaveToJson(string jsonFilename)
        {
            ExternalJsonHandler.SaveToJson(this, BasePath, jsonFilename);
        }

        public void SaveToJson() => SaveToJson(JsonFilename);
        
        public void LoadFromJson(string jsonFilename)
        {
            if (ExternalJsonHandler.IsJsonExist(BasePath, jsonFilename))
                ExternalJsonHandler.LoadFromJson(this, BasePath, jsonFilename);
            else
                SaveToJson();
        }

        public void LoadFromJson() => LoadFromJson(JsonFilename);
    }
}