using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;
using Verse.AI;
using System.IO;

namespace aRandomKiwi.ARS
{
    internal static class Dialog_SaveFileList_Save_Patch
    {
        [HarmonyPatch(typeof(Dialog_SaveFileList_Save), "DoFileInteraction")]
        public class Dialog_SaveFileList_Save_DoFileInteraction
        {
            [HarmonyPrefix]
            public static bool Listener(Dialog_SaveFileList_Save __instance, string mapName)
            {
                try
                {
                    mapName = GenFile.SanitizedFileName(mapName);
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        GameDataSaveLoader.SaveGame(mapName);
                    }, "SavingLongEvent", false, null);

                    //Si signature presente on vire le prefix
                    mapName = Utils.getFilenameWithoutVF(mapName);

                    Messages.Message("SavedAs".Translate(mapName), MessageTypeDefOf.SilentInput, false);
                    PlayerKnowledgeDatabase.Save();
                    __instance.Close(true);

                    //Sauvegarde preview
                    /*ScreenRecorder.saveName = Utils.addPrefix(mapName, false);
                    ScreenRecorder.wantScreenShot = true;*/

                    //Log.Message("ICI");
                    return false;
                }
                catch (Exception e)
                {
                    Utils.logMsg("Dialog_SaveFileList_Save.DoFileInteraction Error : " + e.Message);
                    return true;
                }
            }
        }
    }
}