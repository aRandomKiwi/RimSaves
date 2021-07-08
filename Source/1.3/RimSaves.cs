using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using RimWorld;
using System.IO;
using System;
using System.Linq;

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
                    //A Virtual folder is present
                    if (file.FullName.Contains(Utils.VFOLDERSEP))
                    {
                        pos1 = 0;
                        pos2 = 0;
                        //Obtaining the virtual folder

                        Utils.getVFPosFromPath(file.FullName, out pos1, out pos2);
                        string vf = file.FullName.Substring(pos1, pos2 - pos1);
                        bool present = false;
                        //If vf not present in the internal list we add it
                        foreach (var el in Settings.folders)
                        {
                            if(el == vf)
                            {
                                present = true;
                                break;
                            }
                        }

                        //If not already present we add it
                        if (!present) {
                            Settings.folders.Add(vf);
                            this.WriteSettings();
                         }
                    }
                }
                //If folder by default not present we add it
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