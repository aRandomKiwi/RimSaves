using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace aRandomKiwi.ARS
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            //HarmonyInstance.Create("rimworld.randomKiwi.FFM").PatchAll(Assembly.GetExecutingAssembly());
            var harmony = new Harmony("rimworld.randomKiwi.FFM");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
