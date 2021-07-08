using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using RimWorld;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace aRandomKiwi.ARS
{
    [StaticConstructorOnStartup]
    public class RimSaves : Mod
    {
        public static Settings Settings;
        public static ScreenRecorder comp;

        public RimSaves(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
            Utils.curModRef = this;

            //Check for missing virtual folders
            try
            {
                string text = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                DirectoryInfo directoryInfo1 = new DirectoryInfo(text);
                if (!directoryInfo1.Exists)
                {
                    directoryInfo1.Create();
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(text);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                IOrderedEnumerable<FileInfo>  listSaves = from f in directoryInfo.GetFiles()
                                           where f.Extension == ".rws"
                                           orderby f.LastWriteTime descending
                                           select f;
                int pos1;
                int pos2;

                foreach (var file in listSaves)
                {
                    //Un Virtual folder est present 
                    if (file.FullName.Contains(Utils.VFOLDERSEP))
                    {
                        pos1 = 0;
                        pos2 = 0;
                        //Obtention du virtual folder

                        Utils.getVFPosFromPath(file.FullName, out pos1, out pos2);
                        string vf = file.FullName.Substring(pos1, pos2 - pos1);
                        bool present = false;
                        //Si vf non présent dans la liste interne on l'ajoute
                        foreach(var el in Settings.folders)
                        {
                            if(el == vf)
                            {
                                present = true;
                                break;
                            }
                        }

                        //Si pas deja present on l'ajoute 
                        if (!present) {
                            Settings.folders.Add(vf);
                            this.WriteSettings();
                         }
                    }
                }
                //Si folder par default pas present on le reajoute
                if (!Settings.folders.Contains("Default"))
                    Settings.folders.Insert(0,"Default");

                Log.Message(Utils.RSRelease);
            }
            catch (Exception e)
            {
                Utils.logMsg("Error in checking for missing folders : "+e.Message);
            }
            comp = GameObject.Find("Camera").AddComponent<ScreenRecorder>() as ScreenRecorder;
        }

        public void Save()
        {
            LoadedModManager.GetMod<RimSaves>().GetSettings<Settings>().Write();
        }

        public override string SettingsCategory()
        {
            return "RimSaves";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}