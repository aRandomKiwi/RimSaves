using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using System.IO;
using System.Threading;

namespace aRandomKiwi.ARS
{
    internal static class GameDataSaveLoader_Patch
    {
        [HarmonyPatch(typeof(GameDataSaveLoader), "SaveGame")]
        public class SaveGame
        {
            [HarmonyPrefix]
            public static bool Listener(string fileName, ref int ___lastSaveTick)
            {
                try
                {
                    if (!Find.GameInfo.permadeathMode)
                    {
                        //Preview backup
                        ScreenRecorder.saveName = Utils.addPrefix(fileName, false);
                        ScreenRecorder.wantScreenShot = true;

                        Utils.updateMeta(Utils.addPrefix(fileName, false));
                    }

                    //We prefix the name of the backup of the current virtual directory, if applicable
                    if (Settings.curFolder == "Default")
                        return true;

                    string path = Utils.addPrefix(fileName);

                    SafeSaver.Save(path, "savegame", delegate
                    {
                        ScribeMetaHeaderUtility.WriteMetaHeader();
                        Game game = Current.Game;
                        Scribe_Deep.Look<Game>(ref game, "game", new object[0]);
                    }, Find.GameInfo.permadeathMode);
                    ___lastSaveTick = Find.TickManager.TicksGame;

                    return false;
                }
                catch (Exception e)
                {
                    Utils.logMsg("SaveGame Error : " + e.Message);
                    return true;
                }
            }
        }
    }
}
