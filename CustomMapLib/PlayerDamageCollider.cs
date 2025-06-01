using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppPhoton.Pun;
using Il2CppPhoton.Realtime;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Subsystems;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Player = Il2CppRUMBLE.Players.Player;

namespace CustomMapLib
{
    [RegisterTypeInIl2Cpp]
    public class PlayerDamageCollider : MonoBehaviour // works
    {
        public Il2CppValueField<int> Damage;

        public Il2CppValueField<bool> ConstantDamage;
        public Il2CppValueField<float> DamageInterval;

        public class CollidingPlayer
        {
            public Player player;
            public DateTime lastDamaged;
            public DateTime nextDamageTick;
        }

        private List<CollidingPlayer> collidingPlayers = new List<CollidingPlayer>();

        public void Start()
        {
            if (ConstantDamage)
            {
                MelonCoroutines.Start(damageCoroutine());
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (!ConstantDamage)
            {
                if (collision.other.transform.root.name.ToLower().Contains("player controller"))
                {
                    if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
                    {
                        PlayerController collidingPlayer = collision.other.transform.root.GetComponent<PlayerController>();
                        Player player = collidingPlayer.assignedPlayer;
                        MelonLogger.Msg($"new colliding player with id:{player.Data.GeneralData.PlayFabMasterId}");
                        PlayerHealth healthSystem = player.Controller.GetSubsystem<PlayerHealth>();
                        short newHealth = (short)(player.Data.HealthPoints - Damage.Value);
                        healthSystem.SetHealth(newHealth, player.Data.HealthPoints);
                        return;
                    }
                }
            }
            else
            {
                if (collision.other.transform.root.name.ToLower().Contains("player controller"))
                {
                    PlayerController collidingPlayer = collision.other.transform.root.GetComponent<PlayerController>();
                    CollidingPlayer player = new CollidingPlayer()
                    {
                        player = collidingPlayer.assignedPlayer,
                        lastDamaged = DateTime.Now,
                        nextDamageTick = DateTime.Now.AddSeconds(DamageInterval.Value)
                    };
                    collidingPlayers.Add(player);
                }
            }
        }
        public void OnCollisionExit(Collision collision)
        {
            if (collision.other.transform.root.name.ToLower().Contains("player controller"))
            {
                Player collidingPlayer = collision.other.transform.root.GetComponent<PlayerController>().assignedPlayer;
                string collidingMasterID = collidingPlayer.Data.GeneralData.PlayFabMasterId;
                for (int i = 0; i < collidingPlayers.Count; i++)
                {
                    string listedMasterID = collidingPlayers[i].player.Data.GeneralData.PlayFabMasterId;
                    if (listedMasterID == collidingMasterID)
                    {
                        collidingPlayers.Remove(collidingPlayers[i]);
                        break;
                    }
                }
            }
        }

        public System.Collections.IEnumerator damageCoroutine()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
                {
                    /* 
                    List<CollidingPlayer> tempList = collidingPlayers.Distinct().ToList(); // probably inefficient but it's meant to remove duplicate players, dont question it please (unless you have a better way)
                    collidingPlayers.Clear();
                    foreach (CollidingPlayer player in tempList)
                    {
                        collidingPlayers.Add(player);
                    } // spoiler alert: it didnt work, help
                    */
                    collidingPlayers = collidingPlayers.GroupBy(player => player.player.Data.GeneralData.PlayFabMasterId).Select(players => players.First()).ToList(); // group by playfabmasterid, then select the first in each group, then convert back to list

                    foreach (CollidingPlayer collidingPlayer in collidingPlayers)
                    {
                        if (DateTime.Now >= collidingPlayer.nextDamageTick)
                        {
                            Player player = collidingPlayer.player;
                            PlayerHealth healthSystem = player.Controller.GetSubsystem<PlayerHealth>();
                            short newHealth = (short)(player.Data.HealthPoints - Damage.Value);
                            healthSystem.SetHealth(newHealth, player.Data.HealthPoints);
                            collidingPlayer.lastDamaged = DateTime.Now;
                            collidingPlayer.nextDamageTick = DateTime.Now.AddSeconds(DamageInterval.Value);
                        }
                    }
                }
            }
        }
    }
}
