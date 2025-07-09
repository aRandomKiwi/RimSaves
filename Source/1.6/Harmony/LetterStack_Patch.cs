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
    [HarmonyPatch(typeof(LetterStack))]
    [HarmonyPatch("ReceiveLetter", new Type[] { typeof(Letter), typeof(string), typeof(int), typeof(bool) })]
    class ReceiveLetter_Patch
    {
        [HarmonyPostfix]
        static void Listener(LetterStack __instance, Letter let, string debugInfo, int delayTicks, bool playSound)
        {
            if (Settings.saveOnNegativeIncident && Utils.negativeIncidents.Contains(let.def.defName) )
            {
                DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
                long cts = dto.ToUnixTimeSeconds();
                if (cts - Settings.nbMinSecBetweenIncidentsTsNegative < Settings.nbMinSecBetweenIncidents)
                    return;
                else
                    Settings.nbMinSecBetweenIncidentsTsNegative = cts;

                string name = "BadEvent";
                if (Settings.addEventLabelSuffix)
                    name = name + "." + Utils.SanitizeFileName(let.Label);
                Utils.GCQSI.quicksave(name);
            }
            else if (Settings.saveOnPositiveIncident && Utils.positiveIncidents.Contains(let.def.defName))
            {
                DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
                long cts = dto.ToUnixTimeSeconds();
                if (cts - Settings.nbMinSecBetweenIncidentsTsPositive < Settings.nbMinSecBetweenIncidents)
                    return;
                else
                    Settings.nbMinSecBetweenIncidentsTsPositive = cts;
                string name = "GoodEvent";
                if (Settings.addEventLabelSuffix)
                    name = name + "." + Utils.SanitizeFileName(let.Label);
                Utils.GCQSI.quicksave(name);
            }
        }
    }
}
