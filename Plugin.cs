﻿using System;
using System.Collections.Generic;
using BepInEx;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Models;
using UnityEngine;
using Utilla;
using Photon.Pun;
using BepInEx.Configuration;
using Photon.Voice.Unity;
using BepInEx.Logging;
using HarmonyLib.Tools;
using GorillaNetworking;
using System.Collections;
using System.Threading.Tasks;
using RoomUtils.Patches;

[assembly: InfoWatchCompatible]


namespace RoomUtils
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        internal new ConfigFile Config => base.Config;

        internal InfoWatchPage infoWatchPageInstance;

        bool inRoom;
        private bool wasInRoom = false;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
            {
                {
                    PluginInfo.HashKey, PluginInfo.Version
                }
            });
        }
        

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            // Could apply initial trigger states here if needed
        }

        public void OnJoin()
        {
            inRoom = true;
            
        }

        public void OnLeave()
        {
            inRoom = false;
            
        }

        /*public static void SpoofRank(bool enabled, string tier = "")
        {
            RankedPatch.enabled = enabled;
            RankedPatch.targetTier = tier;
        }

        public static void SpoofPlatform(bool enabled, string target = "")
        {
            RankedPatch.enabled = enabled;
            RankedPatch.targetPlatform = target;
        } */

        [ShowOnHomeScreen(DisplayTitle = "Room Utils")]
        internal class InfoWatchPage : GorillaInfoWatch.Models.InfoWatchScreen
        {
            public override string Title => "Room Utils";

            private static ConfigEntry<bool> disableJoinTriggers = Plugin.Instance.Config.Bind(
                "Room Utils", "DisableJoinTriggers", false, "Disable join room triggers");

            private static ConfigEntry<bool> disableMapTriggers = Plugin.Instance.Config.Bind(
                "Room Utils", "DisableMapTriggers", false, "Disable map transition triggers");

            private static ConfigEntry<bool> disableQuitBox = Plugin.Instance.Config.Bind(
                "Room Utils", "DisableQuitBox", false, "Disable quitbox trigger");

            public override ScreenContent GetContent()
            {
                var lines = new LineBuilder();

                lines.Add(" ", new List<Widget_Base>());

                lines.Add($"Disconnect",
                        new List<Widget_Base> { new Widget_PushButton(Disconnect) });
                lines.Add($"Join Random",
                        new List<Widget_Base> { new Widget_PushButton(JoinRandom) });
                // Spoofing options
                lines.Add(" ", new List<Widget_Base>());

                /*
                bool spoofingEnabled = RankedPatch.enabled;
                string spoofedTier = RankedPatch.targetTier ?? "";
                string spoofedPlatform = RankedPatch.targetPlatform ?? "";

                lines.Add("Spoof Ranked Join",
                    new List<Widget_Base> {
        new Widget_Switch(spoofingEnabled, (bool value) =>
        {
            Plugin.SpoofRank(value, RankedPatch.targetTier);
            SetContent();
        })
                    });

                lines.Add("Tier",
                    new List<Widget_Base> {
        new Widget_TextBox(spoofedTier, (string value) =>
        {
            RankedPatch.targetTier = value;
            SetContent();
        })
                    });

                lines.Add("Platform",
                    new List<Widget_Base> {
        new Widget_TextBox(spoofedPlatform, (string value) =>
        {
            RankedPatch.targetPlatform = value;
            SetContent();
        })
                    }); */




                lines.Add(" ", new List<Widget_Base>());

                // Get current active states on game start
                bool roomTriggersActive = IsTriggerActive("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab");
                bool mapTriggersActive = IsTriggerActive("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab");
                bool quitBoxActive = IsTriggerActive("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox");

                lines.Add("Room Triggers",
                    new List<Widget_Base> { new Widget_Switch(roomTriggersActive, (bool value) =>
        {
            SetTriggerState("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab", value);
            SetContent();
        })});


                lines.Add("Map Triggers",
                    new List<Widget_Base> { new Widget_Switch(mapTriggersActive, (bool value) =>
        {
            SetTriggerState("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab", value);
            SetContent();
        })});


                lines.Add("Quitbox",
                    new List<Widget_Base> { new Widget_Switch(quitBoxActive, (bool value) =>
        {
            SetTriggerState("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox", value);
            SetContent();
        })});


                bool afkKickDisabled = PhotonNetworkController.Instance != null && PhotonNetworkController.Instance.disableAFKKick;
                bool initialState = !afkKickDisabled;

                lines.Add("AFK Kick",
                    new List<Widget_Base> { new Widget_Switch(initialState, (bool value) =>
        {
            if (PhotonNetworkController.Instance != null)
            {
                PhotonNetworkController.Instance.disableAFKKick = !value;
                UnityEngine.Debug.Log("[ROOM UTILS - IW] AFK Kick " + (value ? "enabled" : "disabled") + ".");
            }

              SetContent();
        })});

                lines.Add($"Anti Knockback",
                        new List<Widget_Base> { new Widget_PushButton(NoKnockback) });
                lines.Add($"Knockback",
                        new List<Widget_Base> { new Widget_PushButton(Knockback) });

                return lines;
            }

            // Helper method to check if trigger GameObject is active
            private bool IsTriggerActive(string objectPath)
            {
                var obj = GameObject.Find(objectPath);
                return obj != null && obj.activeSelf;
            }

           
            
            private void Disconnect(object[] args)
            {
                if (NetworkSystem.Instance.InRoom)
                {
                    PhotonNetwork.Disconnect();
                }
                else { UnityEngine.Debug.LogWarning("[ROOM UTILS - IW] Attempted to disconnect from room when not connected."); }
                SetContent();
            }

            private void Knockback(object[] args)
            {
                Patches.KnockbackPatch.enabled = false;
            }

            private void NoKnockback(object[] args)
            {
                Patches.KnockbackPatch.enabled = true;
            }

            private async void JoinRandom(object[] args)
            {
                if (PhotonNetwork.InRoom)
                {
                    NetworkSystem.Instance.ReturnToSinglePlayer();
                    PhotonNetwork.Disconnect();
                    await Task.Delay(2500);
                }
                else
                {
                    UnityEngine.Debug.Log("Not connected to a room.");
                }
                

                string gamemode = PhotonNetworkController.Instance.currentJoinTrigger == null ? "forest" : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.GetJoinTriggerForZone(gamemode), JoinType.Solo);
            

            SetContent();
            }

            



            private void SetTriggerState(string objectPath, bool enabled)
            {
                GameObject target = GameObject.Find(objectPath);
                if (target != null)
                {
                    target.SetActive(enabled);
                }
            }

        }
    }
    
}
