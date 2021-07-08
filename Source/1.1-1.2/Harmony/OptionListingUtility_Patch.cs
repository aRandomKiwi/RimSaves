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
using System.Threading;
using Verse.Profile;

namespace aRandomKiwi.ARS
{
    [HarmonyPatch(typeof(OptionListingUtility), "DrawOptionListing"), StaticConstructorOnStartup]
    class OptionListingUtility_Patch
    {
        [HarmonyPrefix]
        static bool Listener(Rect rect, List<ListableOption> optList)
        {
            string match = "SaveAndQuitToMainMenu".Translate();

            foreach (var el in optList)
            {
                if(el.label == match)
                {
                    el.action = delegate
                    {
                        ScreenRecorder.screenshotSaved = false;
                        ScreenRecorder.saveName = Utils.addPrefix(Current.Game.Info.permadeathModeUniqueName, false);
                        ScreenRecorder.wantScreenShot = true;

                        /*int nb = 0;
                        while (!ScreenRecorder.screenshotSaved && nb <= 5000)
                        {
                            Thread.Sleep(1);
                            nb++;
                        }
                        Log.Message("VALUE NB ===>" + nb);*/

                        LongEventHandler.QueueLongEvent(delegate
                        { 
                            GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
                            MemoryUtility.ClearAllMapsAndWorld();
                        }, "Entry", "SavingLongEvent", false, null);
                };
                    break;
                }
            }

            return true;
        }
    }
}
