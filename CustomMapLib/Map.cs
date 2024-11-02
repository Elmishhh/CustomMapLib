using System.Reflection;
using HarmonyLib;
using Il2CppRUMBLE.Players;
using Il2CppSystem.Xml.Serialization;
using JetBrains.Annotations;
using MelonLoader;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using CustomMapLib.Components;
using Il2CppRUMBLE;
using Il2CppRUMBLE.MoveSystem;
using Il2CppPhoton.Pun;
using Il2CppRUMBLE.Environment;
using Il2CppSteamworks;
using RumbleModUI;
using Il2CppRUMBLE.Pools;
using Il2CppPhoton.Realtime;
using Il2CppSystem.Collections;
using System.Collections;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players.Subsystems;
using RMAPI = RumbleModdingAPI.Calls;
using Il2CppExitGames.Client.Photon;
using Il2Cpp;
using Il2CppRUMBLE.Networking.MatchFlow;
using AssetsTools.NET;
using Il2CppRUMBLE.Environment.MatchFlow;
using System.Runtime.CompilerServices;
using RC = RumbleModdingAPI.Calls.ControllerMap.RightController;
using Il2CppRUMBLE.Utilities;
using UnityEngine.VFX.Utility;

[assembly: AssemblyDescription(CustomMapLib.BuildInfo.Description)]
[assembly: AssemblyCopyright("Created by " + CustomMapLib.BuildInfo.Author)]
[assembly: AssemblyTrademark(CustomMapLib.BuildInfo.Company)]
[assembly: MelonInfo(typeof(CustomMapLib.Map), CustomMapLib.BuildInfo.Name, CustomMapLib.BuildInfo.Version, CustomMapLib.BuildInfo.Author, CustomMapLib.BuildInfo.DownloadLink)]
[assembly: MelonColor(255, 255, 0, 0)]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]

namespace CustomMapLib
{
    public static class BuildInfo
    {
        public const string Name = "CustomMapLib"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "allows you to make more complex custom maps"; // Description for the Mod.  (Set as null if none)
        public const string Author = "elmish"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }
    public class Map : MelonMod
    {
        public string mapName;
        public string creatorName;
        public string mapVersion;
        public GameObject mapParent;
        public bool mapInitialized;
        public MapInternalHandler handler;
        public CustomMultiplayerMaps.main _customMultiplayerMaps;

        private static List<Map> _InitializedMaps = new List<Map>();
        public static CustomMultiplayerMaps.main _staticCustomMultiplayerMaps;

        public static Shader urp_lit;
        private static bool _loaderInitialized;

        public _InternalPedestalSequences._InternalHostPedestal HostPedestal = new _InternalPedestalSequences._InternalHostPedestal();
        public _InternalPedestalSequences._InternalClientPedestal ClientPedestal = new _InternalPedestalSequences._InternalClientPedestal();

        public static GameObject physicMaterialHolder;
        public static Collider physicMaterialHolderCollider;

        public void Initialize(string _mapName, string _mapVersion, string _creatorName, Map _instance)
        {
            if (!mapInitialized)
            {
                mapName = _mapName;
                mapVersion = _mapVersion;
                creatorName = _creatorName;
                _customMultiplayerMaps = (CustomMultiplayerMaps.main)FindMelon("CustomMultiplayerMaps", "UlvakSkillz");
                _staticCustomMultiplayerMaps = _customMultiplayerMaps;
                string mapCombinedName = $"{mapName} {mapVersion}";
                _customMultiplayerMaps.CustomMultiplayerMaps.AddToList(mapCombinedName, true, 0, $"Enable or Disable {mapName} - {creatorName}", new RumbleModUI.Tags());

                HostPedestal._instance = _instance;
                ClientPedestal._instance = _instance;

                _InitializedMaps.Add(_instance);
                mapInitialized = true;
            }
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            try
            {
                if (_loaderInitialized)
                {
                    MelonCoroutines.Start(coroutine());
                }
            }
            catch { }
            try
            {
                if (_loaderInitialized)
                {
                    MelonLogger.Msg("disabling maps?");
                    foreach (Map map in _InitializedMaps)
                    {
                        map.handler.inMatch = false;
                    }
                    MelonLogger.Msg("finished disabling maps?");
                }
            }
            catch
            {
                MelonLogger.Msg("idk why it's erroring");
            }
            if (sceneName == "Gym" && !_loaderInitialized)
            {
                ClassInjector.RegisterTypeInIl2Cpp<MapInternalHandler>();
                urp_lit = Shader.Find("Universal Render Pipeline/Lit");
                CreateOriginal();
                _loaderInitialized = true;
                LoadPhysicsMaterial();
            }
        }

        public GameObject CreatePrimitiveObject(PrimitiveType primitiveType, Vector3 position, Quaternion rotation, Vector3 scale, ObjectType type, SpecialState specials = null)
        {
            GameObject temp = GameObject.CreatePrimitive(type);
            temp.transform.position = position;
            temp.transform.rotation = rotation;
            temp.transform.localScale = scale;
            temp.GetComponent<Renderer>().material.shader = urp_lit;
            Collider col = temp.GetComponent<Collider>();
            if (type != ObjectType.Wall)
            {
                GameObject.Destroy(col);
                col = temp.AddComponent<MeshCollider>();
                temp.layer = (int)type;
            }
            else
            {
                temp.layer = 11;
            }
            GroundCollider groundcol = temp.AddComponent<GroundCollider>();
            groundcol.isMainGroundCollider = true;  
            groundcol.collider = col;
            temp.transform.SetParent(mapParent.transform);
            if (specials != null)
            {
                col.material = GameObject.Instantiate(physicMaterialHolderCollider.material);
                switch (specials.state)
                {
                    case SpecialStates.Bouncy:
                        col.material.bounciness = specials.Bouncines;
                        col.material.bouncyness = specials.Bouncines;
                        break;

                    case SpecialStates.Slippery:
                        col.material.dynamicFriction = specials.Friction;
                        col.material.dynamicFriction2 = specials.Friction;
                        col.material.staticFriction = specials.Friction;
                        col.material.staticFriction2 = specials.Friction;
                        col.material.frictionCombine = PhysicMaterialCombine.Minimum;
                        break;

                    case SpecialStates.Both:
                        col.material.bounciness = specials.Bouncines;
                        col.material.bouncyness = specials.Bouncines;

                        col.material.dynamicFriction = specials.Friction;
                        col.material.dynamicFriction2 = specials.Friction;
                        col.material.staticFriction = specials.Friction;
                        col.material.staticFriction2 = specials.Friction;
                        col.material.frictionCombine = PhysicMaterialCombine.Minimum;
                        break;

                    default:
                        MelonLogger.Error("what the fuck how, ping me with your code this should not be physically possible to get this error");
                        break;
                }
            }
            return temp;
        }

        public virtual void OnMapMatchLoad(bool amHost) { }
        public virtual void OnMapDisabled() { }
        public virtual void OnMapCreation() { }
        public virtual void OnRoundStarted() { }

        public enum ObjectType
        {
            CombatFloor = 9,
            NonCombatFloor = 11,
            Wall = 1
        }
        public class SpecialState
        {
            public float Bouncines = 0;
            public float Friction = 0;

            public SpecialStates state;
        }
        public enum SpecialStates
        {
            Bouncy = 0,
            Slippery = 1,
            Both = 2
        };

        public class _InternalPedestalSequences
        {
            public class _InternalClientPedestal
            {
                public Map _instance;
                public void SetFirstSequence(Vector3 newPosition)
                {
                    _instance.handler.ClientPedestalSequence1 = newPosition;
                }
                public void SetSecondSequence(Vector3 newPosition)
                {
                    _instance.handler.ClientPedestalSequence2 = newPosition;
                }
            }
            public class _InternalHostPedestal
            {
                public Map _instance;
                public void SetFirstSequence(Vector3 newPosition)
                {
                    _instance.handler.HostPedestalSequence1 = newPosition;
                }
                public void SetSecondSequence(Vector3 newPosition)
                {
                    _instance.handler.HostPedestalSequence2 = newPosition;
                }
            }
        }

        public void LoadPhysicsMaterial()
        {
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("CustomMapLib.Resources.physicsmaterial"))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                Il2CppAssetBundle bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
                physicMaterialHolder = new GameObject();
                physicMaterialHolderCollider = physicMaterialHolder.AddComponent<BoxCollider>();
                physicMaterialHolderCollider.material = GameObject.Instantiate(bundle.LoadAsset<UnityEngine.PhysicMaterial>("baseMaterial"));
                GameObject.DontDestroyOnLoad(physicMaterialHolder);
            }
        }
        #region harmony patch hell
            public static void Prefix()
        {
            MelonLogger.Msg("adding custom maps from CustomMapLib");
            foreach (Map map in _InitializedMaps)
            {
                string mapCombinedName = $"{map.mapName} {map.mapVersion}";
                map.mapParent = new GameObject(mapCombinedName);
                map.mapParent.transform.SetParent(map._customMultiplayerMaps.mapsParent.transform);
                map.mapParent.SetActive(false);
                map.handler = map.mapParent.AddComponent<MapInternalHandler>();
                map.handler._map = map;
                map.OnMapCreation();
                MelonLogger.Msg($"Loading {map.mapName} by {map.creatorName}");
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "PreLoadMaps")]
        public static class MapLoadPatch
        {
            public static void Postfix()
            {
                MelonLogger.Msg("adding custom maps from CustomMapLib");
                foreach (Map map in _InitializedMaps)
                {
                    string mapCombinedName = $"{map.mapName} {map.mapVersion}";
                    map.mapParent = new GameObject(mapCombinedName);
                    map.mapParent.transform.SetParent(map._customMultiplayerMaps.mapsParent.transform);
                    map.mapParent.SetActive(false);
                    map.handler = map.mapParent.AddComponent<MapInternalHandler>();
                    map.handler._map = map;
                    map.OnMapCreation();
                    MelonLogger.Msg($"Loading {map.mapName} by {map.creatorName}");
                }
            }
        }

        [HarmonyPatch(typeof(MatchHandler), "ExecuteNextRound")]
        public static class RoundPatch
        {
            public static void Prefix()
            {
                foreach (Map map in _InitializedMaps)
                {
                    if (map.handler.inMatch)
                    {
                        map.OnRoundStarted();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "GetEnabledMapsString")]
        public static class MapListPatch
        {
            public static bool Prefix(ref string __result)
            {
                MelonLogger.Msg("GetEnabledMapsString was called");
                int loadedmaps = 13;
                string mapList = "";
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    MelonLogger.Msg("GetEnabledMapsString - Opponent has mod");
                    foreach (var map in _InitializedMaps) loadedmaps++;

                    for (int i = 4; i < loadedmaps; i++)
                    {
                        string mapName = _staticCustomMultiplayerMaps.CustomMultiplayerMaps.Settings[i].Name;
                        bool mapEnabled = (bool)_staticCustomMultiplayerMaps.CustomMultiplayerMaps.Settings[i].Value;
                        if (mapEnabled == true)
                        {
                            mapList += $"{mapName}|";
                        }
                    }
                    MelonLogger.Msg("GetEnabledMapsString - " + mapList);
                    __result = mapList;
                    return false;
                }
                MelonLogger.Msg("opponent does not have mod");
                return true;
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "ModsReceived")]
        public static class Patch2
        {
            public static bool Prefix()
            {
                MelonLogger.Msg($"ModsReceived was called");
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    MelonLogger.Msg($"ModsReceived - both players have mod");
                    MelonLogger.Msg($"{_staticCustomMultiplayerMaps.enabled}, {_staticCustomMultiplayerMaps.EventSent}, {PhotonNetwork.IsMasterClient}");
                    if (_staticCustomMultiplayerMaps.enabled && !_staticCustomMultiplayerMaps.EventSent && !PhotonNetwork.IsMasterClient)
                    {
                        MelonLogger.Msg(_staticCustomMultiplayerMaps.currentScene);
                        if ((_staticCustomMultiplayerMaps.currentScene == "Map0") || (_staticCustomMultiplayerMaps.currentScene == "Map1"))
                        {
                            MelonLogger.Msg($"ModsReceived - raising event");
                            PhotonNetwork.RaiseEvent(_staticCustomMultiplayerMaps.myEventCode, _staticCustomMultiplayerMaps.GetEnabledMapsString(), CustomMultiplayerMaps.main.eventOptions, SendOptions.SendReliable);
                        }
                    }
                    return false;
                }
                MelonLogger.Msg($"ModsReceived - opponent does not have mod");
                return true;
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "OnEvent")]
        public static class NetworkEventRecievedPatch
        {
            public static bool Prefix(ref EventData eventData)
            {
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    if (eventData.Code == _staticCustomMultiplayerMaps.myEventCode)
                    {
                        MelonLogger.Msg($"OnEvent - opponent has mod");
                        MelonLogger.Msg($"OnEvent - replacing event code 69");
                        string[] enabledMaps = eventData.CustomData.ToString().Split('|');
                        MelonLogger.Msg("CustomMapLib || OnEvent intercepted successfully");
                        _staticCustomMultiplayerMaps.ProcessEventCode69(enabledMaps); // for future referrence, this chooses a map
                        return false;
                    }
                    else if (eventData.Code == _staticCustomMultiplayerMaps.myEventCode2)
                    {
                        MelonLogger.Msg($"OnEvent - opponent has mod");
                        MelonLogger.Msg($"OnEvent - replacing event code 70");
                        string[] availableMaps = new string[] { eventData.CustomData.ToString() };
                        _staticCustomMultiplayerMaps.ProcessEventCode70(availableMaps);
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "ProcessEventCode69")]
        public static class ProcessPatch
        {
            public static bool Prefix(ref string[] processedString)
            {
                MelonLogger.Msg($"ProcessEventCode69 called");
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    MelonLogger.Msg($"ProcessEventCode69 - opponent has mod");
                    string selectedMap = SelectRandomMap(processedString); // processedString = the opponent's enabled maps
                    PhotonNetwork.RaiseEvent(_staticCustomMultiplayerMaps.myEventCode2, selectedMap, CustomMultiplayerMaps.main.eventOptions2, SendOptions.SendReliable);
                }
                return true;
            }
            private static string SelectRandomMap(string[] opponentMaps)
            {
                string[] myMaps = _staticCustomMultiplayerMaps.GetEnabledMapsString().Split('|');

                List<string> availableMaps = new List<string>();
                foreach (string opponentMap in opponentMaps)
                {
                    foreach (string myMap in myMaps)
                    {
                        if (opponentMap == myMap)
                        {
                            availableMaps.Add(myMap);
                        }
                    }
                }
                System.Random rng = new System.Random();
                MelonLogger.Error($"FINAL CHOOSING MAP - list length: {availableMaps.Count}, maps: {availableMaps.ToString()}");
                string map = availableMaps[rng.Next(0, availableMaps.Count - 2)]; // - 2 because there's always an empty one at the end
                MelonLogger.Msg($"map selected: {map}");
                return map;
            }
        }
        [HarmonyPatch(typeof(CustomMultiplayerMaps.main), "ProcessEventCode70")]
        public static class SecondProcessPatch
        {
            public static bool Prefix(ref string[] processedString)
            {
                MelonLogger.Msg($"ProcessEventCode70 called");
                if (RMAPI.Mods.doesOpponentHaveMod(BuildInfo.Name, BuildInfo.Version, true))
                {
                    MelonLogger.Msg($"ProcessEventCode70 - both players have mod");
                    if (_staticCustomMultiplayerMaps.currentScene == "Map0") _staticCustomMultiplayerMaps.UnLoadMap0();
                    else if (_staticCustomMultiplayerMaps.currentScene == "Map1") _staticCustomMultiplayerMaps.UnLoadMap1();
                    PoolManager.instance.ResetPools(AssetType.Structure);
                    for (int i = 0; i < _staticCustomMultiplayerMaps.mapsParent.transform.childCount; i++)
                    {
                        if (_staticCustomMultiplayerMaps.mapsParent.transform.GetChild(i).name == processedString[0]) _staticCustomMultiplayerMaps.mapsParent.transform.GetChild(i).gameObject.SetActive(true);
                    }

                    return false;
                }
                return true;
            }
        }
        #endregion
        #region cosmetic stuff
        private static GameObject ddolStaff;
        //private static Texture2D texture = new Texture2D(1, 2);
        //private static GameObject ddolSigil;
        private void CreateOriginal()
        {
            Il2CppAssetBundle bundle;
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("CustomMapLib.Resources.asset"))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
                ddolStaff = GameObject.Instantiate(bundle.LoadAsset<GameObject>("untitled2"));
                GameObject.DontDestroyOnLoad(ddolStaff);
                ddolStaff.transform.position = Vector3.one * 9999;
                //ddolStaff.transform.position = new Vector3(0, 100, 0);
                //ddolStaff.transform.rotation = Quaternion.Euler(-90, 0, 0);
            }
            /*
            using (Stream imageStream = MelonAssembly.Assembly.GetManifestResourceStream("CustomMapLib.Resources.Zoltraak.png"))
            {
                byte[] imageData = new byte[imageStream.Length];
                imageStream.Read(imageData, 0, imageData.Length);
                texture.LoadImage(imageData);
                ddolSigil = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ddolSigil.transform.localScale = new Vector3(1.75f, 1, 1);
                MeshRenderer renderer = ddolSigil.GetComponent<MeshRenderer>();
                renderer.material.shader = urp_lit;
                renderer.material.mainTexture = texture;
                renderer.material.SetFloat("_Surface", 0);
                renderer.material.EnableKeyword("_ALPHATEST_ON");
                renderer.material.SetFloat("_AlphaClip", 1);
                renderer.material.SetFloat("_Cutoff", 0.5f);
                GameObject.DontDestroyOnLoad(ddolSigil);

                GameObject duplicate = GameObject.Instantiate(ddolSigil);

                ddolSigil.transform.position = new Vector3(-0.4f, 100, -8.1f);
                ddolSigil.transform.SetParent(ddolStaff.transform);

                duplicate.transform.SetParent(ddolSigil.transform);
                duplicate.transform.localPosition = new Vector3(0.4571f, 0, 0);
                duplicate.transform.rotation = Quaternion.Euler(0, 0, 180);


                ddolStaff.transform.position = Vector3.one * 9999f;
                ddolStaff.transform.rotation = Quaternion.identity;
                ddolSigil.transform.rotation = Quaternion.identity;
                ddolSigil.SetActive(false);
            }
            */
        }
        private System.Collections.IEnumerator coroutine()
        {
            while (PlayerManager.instance.localPlayer == null) yield return new WaitForFixedUpdate();
            for (int i = 0; i < 200; i++) yield return new WaitForFixedUpdate();
            whyisitnulling();
        }
        private void whyisitnulling()
        {
            try
            {
                foreach (Il2CppRUMBLE.Players.Player player in PlayerManager.instance.AllPlayers)
                {
                    if (player.Data.GeneralData.InternalUsername == "3F73EBEC8EDD260F")
                    {
                        GameObject chest = null;
                        if (player.Controller.transform.childCount == 11)
                        {
                            chest = player.Controller.gameObject.transform.GetChild(5).GetChild(7).GetChild(0).GetChild(2).GetChild(0).gameObject;
                        }
                        else if (player.Controller.transform.childCount == 12)
                        {
                            chest = player.Controller.gameObject.transform.GetChild(6).GetChild(7).GetChild(0).GetChild(2).GetChild(0).gameObject;
                        }
                        GameObject staff = GameObject.Instantiate(ddolStaff);
                        staff.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
                        staff.transform.SetParent(chest.transform);
                        staff.transform.localPosition = new Vector3(0, 0.1f, -0.15f);
                        staff.transform.localRotation = Quaternion.Euler(35, 90, 0);
                        /*
                        if (_staticCustomMultiplayerMaps.currentScene == "Gym" || _staticCustomMultiplayerMaps.currentScene == "Park")
                        {
                            staff.AddComponent<MagicHandler>();
                        }*/
                    }
                }
            }
            catch { }
        }
        /*
        [RegisterTypeInIl2Cpp]
        public class MagicHandler : MonoBehaviour
        {
            public GameObject sigil;
            public GameObject beam;

            public static RaiseEventOptions eventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            public static byte eventCode = 17;

            public bool rightTrigger;
            public bool prevRightTrigger;

            public bool rightGrip;
            public bool prevRightGrip;

            public GameObject chest;
            public GameObject rightHand;

            public bool HoldingStaff;

            public void Start()
            {
                sigil = transform.GetChild(2).gameObject;
                chest = transform.parent.gameObject;
                rightHand = chest.transform.root.GetChild(1).GetChild(2).gameObject;

                PhotonNetwork.NetworkingClient.EventReceived += (System.Action<EventData>)OnEventReceived;
            }


            public void Update()
            {
                if (PlayerManager.instance.localPlayer.Data.GeneralData.InternalUsername == "3F73EBEC8EDD260F")
                {
                    prevRightTrigger = rightTrigger;
                    rightTrigger = RC.GetTrigger() > 0.5f;

                    prevRightGrip = rightGrip;
                    rightGrip = RC.GetGrip() > 0.5f;

                    if (rightTrigger && !prevRightTrigger && HoldingStaff) MelonCoroutines.Start(Fire(sigil.transform.position, sigil.transform.rotation, sigil.transform.localScale, true));

                    if (rightGrip && !prevRightGrip && Vector3.Distance(rightHand.transform.position, transform.position) < 0.15) SnapToHand(true);
                    else if (!rightGrip && prevRightGrip) SnapToBack(true);
                }
            }

            public void SnapToBack(bool raiseEvent)
            {
                try
                {
                    transform.SetParent(chest.transform);
                    transform.localPosition = new Vector3(0, 0.1f, -0.15f);
                    transform.localRotation = Quaternion.Euler(35, 90, 0);
                    HoldingStaff = false;

                    if (raiseEvent)
                    {
                        string eventData = "staff\\back";
                        PhotonNetwork.RaiseEvent(eventCode, eventData, eventOptions, SendOptions.SendReliable);
                    }
                }
                catch { }
            }
            public void SnapToHand(bool raiseEvent)
            {
                try
                {
                    transform.SetParent(rightHand.transform);
                    transform.localPosition = new Vector3(0, -0.05f, 0);
                    transform.localRotation = Quaternion.Euler(75, 0, 0);
                    HoldingStaff = true;

                    if (raiseEvent)
                    {
                        string eventData = "staff\\hand";
                        PhotonNetwork.RaiseEvent(eventCode, eventData, eventOptions, SendOptions.SendReliable);
                    }
                }
                catch { }
            }
            public static System.Collections.IEnumerator Fire(Vector3 position, Quaternion rotation, Vector3 scale, bool raiseEvent)
            {
                GameObject instancedSigil = GameObject.Instantiate(ddolSigil);
                instancedSigil.transform.position = position;
                instancedSigil.transform.rotation = rotation;
                instancedSigil.transform.localScale = scale * 0.012f;
                instancedSigil.SetActive(true);

                yield return new WaitForSeconds(0.4f);

                GameObject spell = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spell.transform.rotation = instancedSigil.transform.rotation;
                spell.transform.position = instancedSigil.transform.position + instancedSigil.transform.up * 1;
                spell.transform.localScale = new Vector3(1.2f, 0.4f, 1.2f);
                MeshRenderer renderer = spell.GetComponent<MeshRenderer>();
                renderer.material.shader = urp_lit;
                renderer.material.color = new Color32(157, 0, 255, 255);

                Collider collider = spell.GetComponent<Collider>();
                collider.isTrigger = true;

                spell.AddComponent<KillPlayerOnCollision>().useLayermask = false;

                spell.AddComponent<ProjectileHandler>().instancedSigil = instancedSigil;

                Rigidbody rb = spell.AddComponent<Rigidbody>();
                rb.useGravity = false;

                if (raiseEvent)
                {
                    string _position = $"{position.x},{position.y},{position.z}";
                    string _rotation = $"{rotation.eulerAngles.x},{rotation.eulerAngles.y},{rotation.eulerAngles.z}";
                    string _scale = $"{scale.x},{scale.y},{scale.z}";
                    string eventData = $"spell\\{_position}|{_rotation}|{_scale}";
                    PhotonNetwork.RaiseEvent(eventCode, eventData, eventOptions, SendOptions.SendReliable);
                }
            }
            public void OnEventReceived(EventData data)
            {
                if (data.Code == eventCode)
                {
                    string[] dataType = data.CustomData.ToString().Split('\\');
                    if (dataType[0] == "spell")
                    {
                        string[] sentData = dataType[1].Split('|');
                        MelonLogger.Msg(1);
                        MelonLogger.Msg(sentData[0]);
                        Vector3 position = StringToVector3(sentData[0]);
                        MelonLogger.Msg(2);
                        MelonLogger.Msg(sentData[1]);
                        Quaternion rotation = Quaternion.Euler(StringToVector3(sentData[1]));
                        MelonLogger.Msg(3);
                        MelonLogger.Msg(sentData[2]);
                        Vector3 scale = StringToVector3(sentData[2]);
                        MelonLogger.Msg(4);
                        Fire(position, rotation, scale, false);
                    }
                    else if (dataType[0] == "staff")
                    {
                        if (dataType[1] == "back") SnapToBack(false);
                        else if (dataType[1] == "hand") SnapToHand(false);
                    }
                }
            }
            private Vector3 StringToVector3(string line)
            {
                string[] stringarray = line.Split(',');
                float[] position = new float[3];
                for (int i = 0; i < 3; i++) position[i] = float.Parse(stringarray[i]);
                Vector3 temp = new Vector3(position[0], position[1], position[2]);
                return temp;
            }
        }

        [RegisterTypeInIl2Cpp]
        public class ProjectileHandler : MonoBehaviour
        {
            public GameObject instancedSigil;
            public Rigidbody rb;
            public void FixedUpdate()
            {
                if (rb != null)
                {
                    rb.velocity = transform.up * 160;
                }
            }
            public void Start() => MelonCoroutines.Start(coroutine());
            public System.Collections.IEnumerator coroutine()
            {
                gameObject.layer = 11;
                rb = transform.GetComponent<Rigidbody>();
                yield return new WaitForSeconds(0.7f);
                GameObject.Destroy(instancedSigil);
                yield return new WaitForSeconds(0.5f);
                GameObject.Destroy(gameObject);
            }

            public void OnTriggerEnter(Collider other)
            {
                Structure structure = other.gameObject.GetComponent<Structure>();
                Structure structure2 = other.transform.parent.gameObject.GetComponent<Structure>();
                if (structure != null && PhotonNetwork.IsMasterClient)
                {
                    structure.Kill(Vector3.zero, true, true, true);
                }
                else if (structure2 != null && PhotonNetwork.IsMasterClient)
                {
                    structure2.Kill(Vector3.zero, true, true, true);
                }
            }
        }
        */
        #endregion
        /*
[HarmonyPatch(typeof(ParkBoardGymVariant), "OnPlayerEnteredTrigger")]
public static class Patch
{
private static void Prefix()
{
if (SteamFriends.GetPersonaName() == "elmish")
{
ParkBoardGymVariant parkBoardGymVariant = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Parkboard").GetComponent<ParkBoardGymVariant>();
parkBoardGymVariant.hostPlayerCapacity = 255;
}
}
}
[HarmonyPatch(typeof(PhotonNetwork), "CreateRoom")]
public static class AlsoAPatch
{
private static void Prefix(ref string roomName)
{
if (SteamFriends.GetPersonaName() == "elmish")
{
string temp = "<color=#FF0000>c</color><color=#DF001F>m</color><color=#BF003F>o</color><color=#9F005F>n</color><color=#7F007F>,</color> <color=#3F00BF>j</color><color=#1F00DF>o</color><color=#0000FF>i</color><color=#0000DF>n</color> <color=#00009F>m</color><color=#00007F>e</color><color=#00005F>.</color><color=#00003F>.</color><color=#00001F>.</color>";
roomName = $"{roomName.Split('|')[0]}|<color=#800000>???</color>|{temp}"; //UID?|room name|room gamemode
}
}
}
bool q;
bool prevq;
bool k;
bool prevk;

Mod myMod = new Mod();
test _instance;

Il2CppRUMBLE.Pools.Pool<PooledMonoBehaviour> hitMarkerPool;

public override void OnLateInitializeMelon()
{
GameObject obj = new GameObject();
_instance = obj.AddComponent<test>();
obj.AddComponent<PhotonView>().ViewID = 204968724;
GameObject.DontDestroyOnLoad(obj);

myMod.ModName = "rumble war crimes =)";
myMod.ModVersion = "1.0.0";
myMod.SetFolder("Test");
myMod.AddToList("room code", "", "the room code you want to join", new Tags { });
myMod.ModSaved += OnSaved;
myMod.GetFromFile();
UI.instance.AddMod(myMod);
}
public void OnSaved()
{
PhotonNetwork.JoinRoom(myMod.Settings[0].Value.ToString());
}
public override void OnSceneWasLoaded(int buildIndex, string sceneName)
{
base.OnSceneWasLoaded(buildIndex, sceneName);
if (sceneName == "Park" || sceneName == "Map0" || sceneName == "Map1")
{
foreach (Il2CppRUMBLE.Players.Player player in PlayerManager.instance.AllPlayers)
{
string internalID = player.Data.GeneralData.InternalUsername;
if (internalID != PlayerManager.instance.localPlayer.Data.GeneralData.InternalUsername) MelonLogger.Msg(internalID);
}
}
}
public async override void OnUpdate()
{
prevq = q;
prevk = k;
q = Input.GetKeyDown(KeyCode.Q);
k = Input.GetKeyDown(KeyCode.K);
if (q && !prevq)
{
MelonCoroutines.Start(_instance.test2());
}
if (k && !prevk)
{
MelonLogger.Msg("joining room");
await delay(10000);
MelonLogger.Msg("attempting to join");
string roomcode = File.ReadAllLines(@"UserData\sekrit2.txt")[0];
if (roomcode != "") PhotonNetwork.JoinRoom(roomcode);
}
}
public async Task delay(int time)
{
await Task.Delay(time);
}
[HarmonyPatch(typeof(Il2CppRUMBLE.Networking.PhotonInterfaceCollection), "OnFriendListUpdate", new Type[] { typeof(Il2CppSystem.Collections.Generic.List<FriendInfo>) })]
public static class Patch2
{
public static void Prefix(Il2CppSystem.Collections.Generic.List<FriendInfo> friendList)
{
MelonLogger.Msg("patch called");
foreach (FriendInfo friend in friendList)
{
MelonLogger.Msg($"UserId:{friend.UserId}, is online:{friend.IsOnline}, is in room:{friend.IsInRoom}, current room: {friend.Room}");
}
}
}
[RegisterTypeInIl2Cpp]
public class test : MonoBehaviourPunCallbacks
{
public test(IntPtr ptr) : base(ptr) { }
public override void OnConnectedToMaster()
{
MelonLogger.Msg("connected to master!");
MelonCoroutines.Start(test2());
}
public System.Collections.IEnumerator test2()
{
if (!PhotonNetwork.InLobby)
{
MelonLogger.Msg("joining room");
PhotonNetwork.JoinLobby(TypedLobby.Default);
while (!PhotonNetwork.InLobby)
{
yield return new WaitForFixedUpdate();
}
}
yield return new WaitForFixedUpdate();
string[] sekrit = File.ReadAllLines(@"UserData\sekrit.txt");
bool e = PhotonNetwork.FindFriends(sekrit);
PhotonNetwork.LeaveLobby();
}
}
*/ // UNRELATED
    }
}
