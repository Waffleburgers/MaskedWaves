using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MaskedWaves.Patches;
using System;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

namespace MaskedWaves
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MaskedSpawn : BaseUnityPlugin
    {
        private const string modGUID = "Waffle.MaskedWaves";
        private const string modName = "Masked Waves";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static bool isHost;

        public static MaskedSpawn Instance;

        public static ManualLogSource mls;

        internal ConfigEntry<int> MaskedBase;
        internal ConfigEntry<float> MaskedMult;
        internal ConfigEntry<float> MaskedMultMult;
        internal ConfigEntry<int> WaveInterval;
        internal ConfigEntry<int> StartDelay;
        internal ConfigEntry<int> IntervalMod;


        public static GameObject maskedPrefab;

        //used to check if a round is in progress
        public bool started = false;

        public int k = 0;

        int lastSpawned;
        float combinedMult;

        public int waveCount = 0;
        public float timeToNextWave;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Masked Waves has loaded");


            MaskedBase = Config.Bind("General", "MaskedBase", 2, "This value controls how many masked will spawn on the first wave");
            MaskedMult = Config.Bind("General", "MaskedMultiplier", 2f, "This value controls the multiplier of each wave (EX: if MaskedBase is 2 and MaskedMultiplier is 2 on wave 2 it will be 4 and so on) rounds so if its too low with too low MaskedBase will stay constant");
            MaskedMultMult = Config.Bind("General", "MaskedMultiplierMultiplier", 1f, "This value controls how much the multiplier of each wave changes each wave. Used to make it get less out of hand, but you can make it more than 1 to make it insane or delay when it first multiplies (MaskedMult = 1, MaskedMultMult = 1.1 etc). Set to 1 to disable");
            WaveInterval = Config.Bind("General", "WaveInterval", 60, "This value controls how many seconds are in between each wave");
            StartDelay = Config.Bind("General", "StartDelay", 10, "This value controls how many seconds it will take to spawn the first wave");
            IntervalMod = Config.Bind("General", "IntervalMod", 1, "This value controls how much the wave interval changes each time. Greater than 1 makes it longer, 1 makes it constant (disables), less than 1 makes it less, 0 or less makes it always instant so probably dont do that");


            harmony.PatchAll(typeof(MaskedSpawn));
            harmony.PatchAll(typeof(GetMaskedPrefab));
            harmony.PatchAll(typeof(RoundStartAndEnd));
        }

        void Update()
        {
            //make sure a round is in progress
            if (started)
            {
                //every update subtract time to count down
                timeToNextWave -= Time.deltaTime;
                if (timeToNextWave <= 0)
                {
                    //I commented on all this then changed all the math and stuff to add MaskedMultMult so im too lazy to check the stuff and add much more but you can figure it out
                    if (waveCount == 0)
                    {
                        lastSpawned = MaskedBase.Value;
                        //instead of doing an else if wavecount == 1 and not doing maskedmultmult this account for the first time it's used
                        combinedMult = MaskedMult.Value / MaskedMultMult.Value;
                    }
                    else
                    {
                        combinedMult = combinedMult * MaskedMultMult.Value;
                        lastSpawned = (int)Math.Round(lastSpawned * combinedMult);
                    }
                    mls.LogInfo(waveCount);
                    //set the time to next wave with the wave interval and mod value (uses pow and wave count so you dont have to create extra variables that get modded each time)
                    timeToNextWave = (int)(WaveInterval.Value * Math.Pow(IntervalMod.Value, waveCount));

                    //spawn enemy count
                    SpawnEnemies(lastSpawned);
                    waveCount++;
                }
            }
        }

        //stolen from game master, just in case
        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        static void setIsHost()
        {
            mls.LogInfo("Host Status: " + RoundManager.Instance.NetworkManager.IsHost.ToString());
            isHost = RoundManager.Instance.NetworkManager.IsHost;
        }

        private static void SpawnEnemies(int amount)
        {
            // doesn't work regardless if not host but just in case (stolen from game master)
            if (!isHost) { return; }
            mls.LogInfo("Spawning");
            for (int i = 0; i < amount; i++)
            {
                mls.LogInfo("Spawned masked #" + i.ToString() + ".");
                GameObject obj = UnityEngine.Object.Instantiate(maskedPrefab,//if you're trying to spawn specific enemies use this, its assigned in GetMaskedPrefab
                                                                GameObject.FindGameObjectsWithTag("OutsideAINode")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length - 1)].transform.position, //spawns at random outside enemy spawn, way easier than vent but some enemies dont like being outside the facility
                                                                Quaternion.Euler(Vector3.zero));
                obj.gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true); //used in the code i dont entirely actually know what it does though
            }
        }   
    }
}
