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
                    //Sauvegarde preview
                    ScreenRecorder.saveName = Utils.addPrefix(fileName, false);
                    if (!Find.GameInfo.permadeathMode)
                        ScreenRecorder.wantScreenShot = true;
                    else
                        ScreenRecorder.wantScreenShot = false;
                    Utils.updateMeta(Utils.addPrefix(fileName, false));

                    //On  prefix le nom de la sauvegarde du virtual directory courant le cas echeant
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
