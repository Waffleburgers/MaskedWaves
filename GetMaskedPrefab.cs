using HarmonyLib;

namespace MaskedWaves.Patches
{
    [HarmonyPatch]
    internal class GetMaskedPrefab
    {
        [HarmonyPatch(typeof(Terminal), "Start")]
        [HarmonyPostfix]
        static void SavesPrefab(ref SelectableLevel[] ___moonsCatalogueList)
        {
            //makes it usable
            foreach (var moon in ___moonsCatalogueList)
            {
                //checks all variables in a list called enemies, Check code if doing an inside enemy because i believe there are different lists for different types
                foreach (SpawnableEnemyWithRarity enemy in moon.Enemies)
                { 
                    //if its masked
                    if (enemy.enemyType.enemyName == "Masked")
                    {
                        //SpawnableEnemyWithRarity has a variable enemyType which has a bunch of useful variables but one of them is the prefab of the enemy used for spawning
                        MaskedSpawn.maskedPrefab = enemy.enemyType.enemyPrefab;
                        MaskedSpawn.mls.LogInfo("Masked prefab found");
                    }
                }
            }
        }
    }
}
