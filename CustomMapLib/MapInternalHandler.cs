using Il2CppPhoton.Pun;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2CppRUMBLE;
using Il2CppRUMBLE.Managers;
using RMAPI = RumbleModdingAPI.Calls;
using Il2CppRUMBLE.Environment.MatchFlow;
using Il2CppExitGames.Client.Photon;
using Il2CppRUMBLE.Environment;
using System.Collections;
using Il2Cpp;
using Il2CppRUMBLE.Networking.MatchFlow;

namespace CustomMapLib.Components
{
    [RegisterTypeInIl2Cpp]
    public class MapInternalHandler : MonoBehaviour
    {
        public MapInternalHandler(IntPtr ptr) : base(ptr) { } // idk tbh but it's needed

        public Map _map;
        public bool inMatch;

        public Pedestal clientPedestal;
        public Vector3 ClientPedestalSequence1 = new Vector3(0, 0, -3);
        public Vector3 ClientPedestalSequence2 = new Vector3(0, -1, 0);

        public Pedestal hostPedestal;
        public Vector3 HostPedestalSequence1 = new Vector3(0, 0, 3);
        public Vector3 HostPedestalSequence2 = new Vector3(0, -1, 0);

        private string currentScene;

        public void OnEnable()
        {
            inMatch = true;
            _map.OnMapMatchLoad(PhotonNetwork.IsMasterClient);
            currentScene = _map._customMultiplayerMaps.currentScene;
            MelonCoroutines.Start(GetPedestals());
        }

        public void OnDisable()
        {
            inMatch = false;
            _map.OnMapDisabled();
        }

        private IEnumerator GetPedestals()
        {
            yield return new WaitForSeconds(1);
            PedestalManager manager = null;
            if (currentScene == "Map0")
            {
                manager = RMAPI.GameObjects.Map0.Logic.Pedestals.GetGameObject().GetComponent<PedestalManager>();
            }
            else if (currentScene == "Map1")
            {
                manager = RMAPI.GameObjects.Map1.Logic.Pedestals.GetGameObject().GetComponent<PedestalManager>();
            }
            foreach (Pedestal pedestal in manager.Pedestals)
            {
                foreach (MoveOffsetOverDuration moveSequence in pedestal.CurrentMoveSequence)
                {
                    if (moveSequence.offset == new Vector3(0, 0, 3))
                    {
                        hostPedestal = pedestal;
                    }
                    else if (moveSequence.offset == new Vector3(0, 0, -3))
                    {
                        clientPedestal = pedestal;
                    }
                }
            }
            MelonCoroutines.Start(coroutine());
        }
        private IEnumerator coroutine()
        {
            yield return new WaitForSeconds(2);
            if (inMatch && (currentScene == "Map0" || currentScene == "Map1"))
            {
                clientPedestal.CurrentMoveSequence[0].offset = ClientPedestalSequence1;
                clientPedestal.CurrentMoveSequence[1].offset = ClientPedestalSequence2;

                hostPedestal.CurrentMoveSequence[0].offset = HostPedestalSequence1;
                hostPedestal.CurrentMoveSequence[1].offset = HostPedestalSequence2;
            }
            MatchHandler.instance.firstRoundConfig.MoveOffsetSequencePedestalOne[0].offset = HostPedestalSequence1;
            MatchHandler.instance.firstRoundConfig.MoveOffsetSequencePedestalOne[1].offset = HostPedestalSequence2;
            MatchHandler.instance.firstRoundConfig.MoveOffsetSequencePedestalTwo[0].offset = ClientPedestalSequence1;
            MatchHandler.instance.firstRoundConfig.MoveOffsetSequencePedestalTwo[1].offset = ClientPedestalSequence2;

            MatchHandler.instance.repeatingRoundConfig.MoveOffsetSequencePedestalOne[0].offset = HostPedestalSequence1;
            MatchHandler.instance.repeatingRoundConfig.MoveOffsetSequencePedestalOne[1].offset = HostPedestalSequence2;
            MatchHandler.instance.repeatingRoundConfig.MoveOffsetSequencePedestalTwo[0].offset = ClientPedestalSequence1;
            MatchHandler.instance.repeatingRoundConfig.MoveOffsetSequencePedestalTwo[1].offset = ClientPedestalSequence2;

            MatchHandler.instance.finalRoundConfig.MoveOffsetSequencePedestalOne[0].offset = HostPedestalSequence1;
            MatchHandler.instance.finalRoundConfig.MoveOffsetSequencePedestalOne[1].offset = HostPedestalSequence2;
            MatchHandler.instance.finalRoundConfig.MoveOffsetSequencePedestalTwo[0].offset = ClientPedestalSequence1;
            MatchHandler.instance.finalRoundConfig.MoveOffsetSequencePedestalTwo[1].offset = ClientPedestalSequence2;
        }
    }
}
