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
    internal static class GenFilePaths_Patch
    {
        [HarmonyPatch(typeof(GenFilePaths), "get_AllSavedGameFiles")]
        public class get_AllSavedGameFiles
        {
            [HarmonyPrefix]
            public static bool Listener(ref IEnumerable<FileInfo>  __result)
            {
                try
                {
                    string text = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                    DirectoryInfo directoryInfo1 = new DirectoryInfo(text);
                    if (!directoryInfo1.Exists)
                    {
                        directoryInfo1.Create();
                    }

                    DirectoryInfo directoryInfo = new DirectoryInfo( text );
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }

                    if (Settings.curFolder == "Default")
                    {
                        //Return only path which dont contain the virtual folder signature
                        __result = from f in directoryInfo.GetFiles()
                                   where f.Extension == ".rws" && !f.FullName.Contains(Utils.VFOLDERSEP)
                                   orderby f.LastWriteTime descending
                                   select f;
                    }
                    else
                    {
                        __result = from f in directoryInfo.GetFiles()
                                   where f.Extension == ".rws" && f.FullName.Contains(Settings.curFolder + Utils.VFOLDERSEP)
                                   orderby f.LastWriteTime descending
                                   select f;
                    }

                    return false;
                }
                catch(Exception e)
                {
                    Utils.logMsg("get_AllSavedGameFiles Error : " + e.Message);
                    return true;
                }
            }
        }
    }
}
