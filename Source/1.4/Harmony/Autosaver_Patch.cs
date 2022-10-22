using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using System.IO;

namespace aRandomKiwi.ARS
{
    internal static class Autosaver_Patch
    {
        [HarmonyPatch(typeof(Autosaver), "AutosaverTick")]
        public class AutosaverTick
        {
            [HarmonyPrefix]
            public static bool Listener()
            {
                if (Settings.disableAutosave)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(Autosaver), "NewAutosaveFileName")]
        public class SaveGame
        {
            [HarmonyPrefix]
            public static bool Listener(Autosaver __instance, ref string __result )
            {
                try
                {
                    string prefix = "";
                    if (Settings.curFolder != "Default")
                        prefix = Settings.curFolder + Utils.VFOLDERSEP;

                    string text = (from name in AutoSaveNames()
                                   where !SaveGameFilesUtility.SavedGameNamedExists(prefix+name)
                                   select name).FirstOrDefault<string>();
                    if (text != null)
                    {
                        __result = text;
                        //Preview backup
                        ScreenRecorder.saveName = Utils.replaceLastOccurrence(text,".rws","");
                        ScreenRecorder.wantScreenShot = true;
                        return false;
                    }
                    __result = AutoSaveNames().MinBy((string name) => new FileInfo(GenFilePaths.FilePathForSavedGame(prefix + name)).LastWriteTime);

                    //Preview backup
                    ScreenRecorder.saveName = __result;
                    ScreenRecorder.wantScreenShot = true;

                    return false;
                }
                catch (Exception e)
                {
                    Utils.logMsg("SaveGame Error : " + e.Message);
                    return true;
                }
            }

            private static IEnumerable<string> AutoSaveNames()
            {
                for (int i = 1; i <= Settings.nbAutosave; i++)
                {
                    yield return "Autosave-" + i;
                }
            }
        }
    }
}
