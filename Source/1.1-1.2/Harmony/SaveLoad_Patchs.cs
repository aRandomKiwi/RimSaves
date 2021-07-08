using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;
using HarmonyLib;
using System.IO;

namespace aRandomKiwi.ARS
{
    [HarmonyPatch(typeof(Map), nameof(Map.FinalizeLoading)), StaticConstructorOnStartup]
    public class MapFinalizeLoadPatch
    {
        public static void Postfix(Map __instance)
        {
            
        }
    }
}
