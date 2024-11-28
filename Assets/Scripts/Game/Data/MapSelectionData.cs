using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Data/Map Selection Data", fileName = "MapSelectionData")]
    public class MapSelectionData:ScriptableObject
    {
        public List<MapInfo> maps;
    }
}

[Serializable]
public struct MapInfo
{
    public Sprite MapThumbnail;
    public string MapName;
    public string SceneName;
}