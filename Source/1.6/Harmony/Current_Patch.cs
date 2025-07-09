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
    [HarmonyPatch(typeof(Current), "Notify_LoadedSceneChanged"), StaticConstructorOnStartup]
    class Notify_LoadedSceneChanged_Patch
    {
        [HarmonyPostfix]
        static void Listener()
        {
            if (GenScene.InEntryScene)
            {
                ScreenRecorder comp = GameObject.Find("Camera").AddComponent<ScreenRecorder>() as ScreenRecorder;
            }
            else
            {
                ScreenRecorder comp = GameObject.Find("Camera").AddComponent<ScreenRecorder>() as ScreenRecorder;
            }
        }
    }
}
