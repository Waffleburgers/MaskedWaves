using HarmonyLib;

namespace MaskedWaves.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class RoundStartAndEnd
    {

        [HarmonyPatch(nameof(StartOfRound.SetPlayerObjectExtrapolate))]
        [HarmonyPostfix]
        static void StartSpawning()
        {
            MaskedSpawn.mls.LogInfo("SetPlayerObjectExtrapolate was called");
            //happens twice at the beginning so it only does it on the second one (would use MiscShipStartup or whatever but its private and idk how to do that or if you can) stored in Plugin.cs because its annoying
            if (MaskedSpawn.Instance.k == 1)
            {
                //reset stuff for the round and declare that the round has started
                MaskedSpawn.Instance.waveCount = 0;
                MaskedSpawn.Instance.timeToNextWave = MaskedSpawn.Instance.StartDelay.Value;
                MaskedSpawn.Instance.started = true;
            }
            else { MaskedSpawn.Instance.k++; }
        }

        [HarmonyPatch(nameof(StartOfRound.ShipHasLeft))]
        [HarmonyPostfix]
        static void PauseSpawning()
        {
            //declare that ship has left
            MaskedSpawn.mls.LogInfo("ShipHasLeft was called");
            MaskedSpawn.Instance.k = 0;
            MaskedSpawn.Instance.started = false;
        }
    }
}
