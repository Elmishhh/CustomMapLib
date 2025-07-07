using CustomMapLib.Components;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Newtonsoft.Json;
using Il2CppPlayFab.EconomyModels;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class MapData
{
    [JsonPropertyName("mapName")]
    public string mapName {  get; set; }
    [JsonPropertyName("mapCreator")]
    public string mapCreator { get; set; }
    [JsonPropertyName("mapVersion")]
    public string mapVersion { get; set; }
    [JsonPropertyName("mapType")]
    public int mapType { get; set; }
}

namespace CustomMapLib
{
    public static class Importer
    {

        public static void CreateAllMaps()
        {
            string path = @"UserData\CustomMapLib";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

            foreach (string filePath in Directory.GetFiles(path))
            {
                ImportMap(filePath);
            }
        }

        public static void ImportMap(string path)
        {
            string extractPath = Path.Combine(Application.temporaryCachePath, "tempMapImport");
            if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
            Directory.CreateDirectory(extractPath);

            ZipFile.ExtractToDirectory(path, extractPath);

            string jsonPath = Path.Combine(extractPath, "MapData.json");
            string json = File.ReadAllText(jsonPath);
            MapData data = JsonConvert.DeserializeObject<MapData>(json);

            string bundlePath = Path.Combine(extractPath, "mapBundle");
            Il2CppAssetBundle bundle = Il2CppAssetBundleManager.LoadFromFile(bundlePath);

            GameObject temp = bundle.LoadAllAssets<GameObject>().Where(go => go.GetComponent<CustomMap>() != null).First();
            GameObject mapObject = GameObject.Instantiate(temp);
            CustomMap map = mapObject.GetComponent<CustomMap>();
            map.MapName = data.mapName;
            map.MapCreator = data.mapCreator;
            map.MapVersion = data.mapVersion;
            map.MapType = (CustomMap.MapTypes)data.mapType;
            GameObject.DontDestroyOnLoad(mapObject);

            bundle.Unload(false);

            NonScriptedMap dummymap = new NonScriptedMap();
            dummymap.NonScriptedCustomMap = map;
            dummymap.Initialize(map.MapName, map.MapVersion, map.MapCreator);
            dummymap.mapParent = mapObject;
        }
    }

    [RegisterTypeInIl2Cpp]
    public sealed class CustomMap : MonoBehaviour // the name is a bit confusing so i'll just seal this class aswell (sealed classes cant be inherited from, if you havent read the note on it from Map.cs)
    {
        public CustomMap(IntPtr ptr) : base(ptr) { }

        public enum MapTypes
        {
            Gym = 0,
            Park = 1,
            Match = 2,
            Any = 3
        };

        private bool showGym() // not sure if these are needed but they are in the editor script so who knows
        {
            return MapType == MapTypes.Gym || MapType == MapTypes.Any;
        }
        private bool showPark()
        {
            return MapType == MapTypes.Park || MapType == MapTypes.Any;
        }
        private bool showMatch()
        {
            return MapType == MapTypes.Match || MapType == MapTypes.Any;
        }

        public string MapName; // these sadly do not get serialized, must used a json
        public string MapCreator;
        public string MapVersion;
        public MapTypes MapType;
        public Il2CppValueField<float> PlayerKillboxDistance;
        public Il2CppValueField<float> StructureKillboxDistance;

        public Il2CppValueField<Vector3> GymPositionOffset;
        public Il2CppValueField<Quaternion> GymRotationOffset;

        public Il2CppValueField<Vector3> ParkPositionOffset;
        public Il2CppValueField<Quaternion> ParkRotationOffset;

        public Il2CppValueField<Vector3> MatchPositionOffset;
        public Il2CppValueField<Quaternion> MatchRotationOffset;


        public Il2CppValueField<Vector3> ClientPedestalPosition1;
        public Il2CppValueField<Vector3> ClientPedestalPosition2;

        public Il2CppValueField<Vector3> HostPedestalPosition1;
        public Il2CppValueField<Vector3> HostPedestalPosition2;

        /*
        public void Start()
        {
            MelonLogger.Msg($"name: {MapName}, creator: {MapCreator}, version: {MapVersion}, type {MapType}");
            MelonLogger.Msg($"Gym position offset: {GymPositionOffset.Value}, Gym rotation offset: {GymRotationOffset.Value.eulerAngles}");
            MelonLogger.Msg($"Park position offset: {ParkPositionOffset.Value}, Park rotation offset: {ParkRotationOffset.Value.eulerAngles}");
            MelonLogger.Msg($"Match position offset: {MatchPositionOffset.Value}, Match rotation offset: {MatchRotationOffset.Value.eulerAngles}");
        }
        */
    }
}
