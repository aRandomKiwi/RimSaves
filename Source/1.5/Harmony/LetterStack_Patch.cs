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

namespace aRandomKiwi.ARS
{
    [HarmonyPatch(typeof(LetterStack), "ReceiveLetter", new Type[] { typeof(Letter), typeof(string), typeof(int), typeof(bool) })]
    class ReceiveLetter_Patch
    {
        [HarmonyPostfix]
        static void Listener(LetterStack __instance, Letter let, string debugInfo)
        {
            if (Settings.saveOnNegativeIncident && Utils.negativeIncidents.Contains(let.def.defName) )
            {
                Utils.GCQSI.quicksave("NegativeIncident");
            }
        }
    }
}
