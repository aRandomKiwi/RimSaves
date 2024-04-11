using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.IO;
using System.Globalization;
using RimWorld.Planet;

namespace aRandomKiwi.ARS
{
    [StaticConstructorOnStartup]
    static class Utils
    {
        public static string basePathRS = "";
        public static string filter="";
        public static bool initDialog = false;
        public static string selectedSave = "";
        public static string loadedSave = "";

        public static Vector2 metaScrollPosition;

        public static Dictionary<string, Texture2D> cachedPreviews = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Dictionary<string, string>> cachedMeta = new Dictionary<string, Dictionary<string, string>>();

        public static List<string> metaFields = new List<string> { "seed","scenario", "faction","storyteller", "time", "storytellerDifficulty", "nbMods", "nbColony", "nbColonist", "playTime"};

        public static string TranslateTicksToTextIRLSeconds(int ticks)
        {
            //If less than one hour ingame then display seconds
            if (ticks < 2500)
                return ticks.ToStringSecondsFromTicks();
            else
                return ticks.ToStringTicksToPeriodVerbose(true);
        }

        public static String FormatSize(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public static string replaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        public static string getBasePathRSPreviews()
        {
            string ret = "";
            ret = Path.Combine(GenFilePaths.SaveDataFolderPath, "RimSaves");
            DirectoryInfo directoryInfo = new DirectoryInfo(ret);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            ret = Path.Combine(ret, "Previews");
            directoryInfo = new DirectoryInfo(ret);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return ret;
        }

        public static string getBasePathSaves()
        {
            string ret = "";
            ret = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
            DirectoryInfo directoryInfo = new DirectoryInfo(ret);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return ret;
        }

        public static string getBasePathRSMeta()
        {
            string ret = "";
            ret = Path.Combine(GenFilePaths.SaveDataFolderPath, "RimSaves");
            DirectoryInfo directoryInfo = new DirectoryInfo(ret);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            ret = Path.Combine(ret, "Meta");
            directoryInfo = new DirectoryInfo(ret);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return ret;
        }

        public static Texture2D loadPreview(string fullPath)
        {
            if (cachedPreviews.ContainsKey(fullPath))
                return cachedPreviews[fullPath];
            else
            {
                Texture2D tmp = Utils.customTexLoad(fullPath);
                if (tmp != null)
                    cachedPreviews[fullPath] = tmp;

                return tmp;
            }
        }

        public static void updateCachedPreviewPath(string prevFP, string newFP)
        {
            if (cachedPreviews.ContainsKey(prevFP))
            {
                cachedPreviews[newFP] = cachedPreviews[prevFP];
            }
        }

        public static void updateCachedMetaPath(string prevFP, string newFP)
        {
            if (cachedMeta.ContainsKey(prevFP))
            {
                cachedMeta[newFP] = cachedMeta[prevFP];
            }
        }

        public static int getNumberActiveMods()
        {
            int ret = 0;
            foreach(var m in ModLister.AllInstalledMods)
            {
                if (m.Active)
                    ret++;
            }
            return ret;
        }

        public static int getNumberColonists()
        {
            int ret = 0;
            foreach(var m in Find.Maps)
            {
                ret += m.mapPawns.ColonistCount;
            }

            foreach(var c in Find.World.worldObjects.Caravans)
            {
                if(c.pawns != null)
                {
                    foreach(var p in c.pawns)
                    {
                        if (p.IsColonist)
                            ret++;
                    }
                }
            }

            return ret;
        }

        public static void updateMeta(string saveName)
        {
            try
            {
                Dictionary<string, string> meta = new Dictionary<string, string>();

                meta["seed"] = Find.World.info.seedString;

                meta["faction"] = Find.FactionManager.OfPlayer.Name;

                if (ModLister.AllInstalledMods != null)
                    meta["nbMods"] = getNumberActiveMods().ToString(); //ScribeMetaHeaderUtility.loadedModIdsList.Count.ToString();
                else
                    meta["nbMods"] = "";
                if (Find.Scenario != null)
                    meta["scenario"] = Find.Scenario.name;
                else
                    meta["scenario"] = "";

                if (Find.Storyteller != null)
                    meta["storyteller"] = Find.Storyteller.def.LabelCap;
                else
                    meta["storyteller"] = "";

                if (Find.Storyteller != null && Find.Storyteller.difficulty != null)
                    meta["storytellerDifficulty"] = Find.Storyteller.difficultyDef.LabelCap;
                else
                    meta["storytellerDifficulty"] = "";

                int nbColony = 0;
                foreach (var m in Find.Maps)
                {
                    if (m.IsPlayerHome)
                        nbColony++;
                }
                meta["nbColony"] = nbColony.ToString();
                meta["nbColonist"] = getNumberColonists().ToString(); // Find.ColonistBar.Entries.Count.ToString();

                StringBuilder stringBuilder = new StringBuilder();
                if (Find.GameInfo != null)
                {
                    TimeSpan timeSpan = new TimeSpan(0, 0, (int)Find.GameInfo.RealPlayTimeInteracting);
                    stringBuilder.Append(string.Concat(new object[]
                    {
                        timeSpan.Days,
                        "LetterDay".Translate(),
                        " ",
                        timeSpan.Hours,
                        "LetterHour".Translate(),
                        " ",
                        timeSpan.Minutes,
                        "LetterMinute".Translate(),
                        " ",
                        timeSpan.Seconds,
                        "LetterSecond".Translate()
                    }));
                    meta["playTime"] = stringBuilder.ToString();
                }
                else
                    meta["playTime"] = "";

                Vector2 location;
                location.x = 0;
                location.y = 0;

                if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector != null && Find.WorldSelector.selectedTile >= 0)
                {
                    location = Find.WorldGrid.LongLatOf(Find.WorldSelector.selectedTile);
                }
                else if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector != null && Find.WorldSelector.NumSelectedObjects > 0)
                {
                    location = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
                }
                else
                {
                    if (Find.CurrentMap == null)
                    {
                        if (Find.WorldGrid != null)
                            location = Find.WorldGrid.LongLatOf(0);
                    }
                    else
                    {
                        if (Find.WorldGrid != null)
                            location = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
                    }
                }

                if (Find.TickManager != null)
                {
                    int index = GenDate.HourInteger((long)Find.TickManager.TicksAbs, location.x);
                    int num = GenDate.DayOfTwelfth((long)Find.TickManager.TicksAbs, location.x);
                    Season season = GenDate.Season((long)Find.TickManager.TicksAbs, location);
                    Quadrum quadrum = GenDate.Quadrum((long)Find.TickManager.TicksAbs, location.x);
                    int num2 = GenDate.Year((long)Find.TickManager.TicksAbs, location.x);
                    string text = season.LabelCap();

                    meta["time"] = GenDate.DateReadoutStringAt((long)Find.TickManager.TicksAbs, location) + " (" + season.Label() + ")";
                }
                else
                {
                    meta["time"] = "";
                }
                //meta["lastEvent1"] = 

                if (Find.Archive != null && Find.Archive.ArchivablesListForReading.Count() > 0)
                {
                    var orderByResult = from s in Find.Archive.ArchivablesListForReading
                                        orderby s.CreatedTicksGame
                                        select s;

                    int nb = 0;
                    string prefix = "";
                    meta["lastIncident"] = "";
                    foreach (var el in orderByResult.Reverse())
                    {
                        if (el is Letter)
                        {
                            Letter cl = (Letter)el;
                            prefix = cl.def.color.r + "," + cl.def.color.g + "," + cl.def.color.b + "-";
                            meta["lastIncident"] += prefix + el.ArchivedLabel;
                            if (nb == 14)
                                break;
                            meta["lastIncident"] += "\n";
                            nb++;
                        }
                    }
                }


                string fp = Path.Combine(getBasePathRSMeta(), saveName) + ".dat";

                //Caching
                cachedMeta[saveName] = meta;
                //foreacFind.Archive.ArchivablesListForReading
                serialize(meta, fp);
            }
            catch(Exception e)
            {
                Log.Message("[RimSaves] :  " + e.Message);
            }
        }

        public static Dictionary<string, string> getSaveMeta(string saveName)
        {
            string fp = Path.Combine(getBasePathRSMeta(),saveName)+".dat";

            if (cachedMeta.ContainsKey(saveName))
                return cachedMeta[saveName];
            else
            {
                if (File.Exists(fp))
                {
                    Dictionary<string, string> ret = unserialize(fp);
                    if (ret != null)
                        cachedMeta[saveName] = ret;
                    return ret;
                }
                else
                    return null;
            }
        }

        public static void setSaveMeta(string saveName, Dictionary<string, string> data)
        {
            if (cachedMeta.ContainsKey(saveName))
            {
                cachedMeta[saveName] = data;
            }
            string fp = Path.Combine(getBasePathRSMeta(), saveName) + ".dat";
            serialize(data, fp);
        }

        public static Texture2D customTexLoad(string fullPath, int w = 64, int h = 64)
        {
            if (!File.Exists(fullPath))
                return null;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(fullPath));
            return texture;
        }

        public static string addPrefix(string fileName, bool fullPath = true)
        {
            string vfPrefix = "";
            if (Settings.curFolder != "Default")
            {
                //If for any reason there is the signature of VF then we squeeze the addition of the VF
                if (!fileName.Contains(Utils.VFOLDERSEP))
                    vfPrefix = Settings.curFolder + Utils.VFOLDERSEP;
            }

            string path = "";

            if (fullPath)
                path = GenFilePaths.FilePathForSavedGame(vfPrefix + fileName);
            else
                path = vfPrefix + fileName;

            return path;
        }

        public static void showFolderList(Action<string> cb,string folderToAvoid, bool showNbSave = true)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            Settings.folders.Sort();
            foreach (string cfolder in Settings.folders)
            {
                string nbs = "";
                int nb = 0;

                if (showNbSave)
                {
                    nb = getNbSavesInVF(cfolder);
                    nbs = " "+"ARS_nbSaves".Translate(nb);
                }
                //If folder to avoid we continue the loop without adding it
                if (folderToAvoid == cfolder)
                    continue;
                list.Add(new FloatMenuOption(Utils.OPTNSTART+cfolder+nbs, delegate
                {
                    cb(cfolder);
                }, MenuOptionPriority.Default, null, null, 0f, null, null));
            }
            if(list.Count != 0)
                Find.WindowStack.Add(new FloatMenu(list));
        }

        public static void changeFolder(string cfolder, Dialog_FileList instance)
        {
            //Change of current file
            Settings.curFolder = cfolder;
            Utils.curModRef.WriteSettings();
            Traverse.Create(instance).Method("ReloadFiles").GetValue();
        }

        public static void getVFPosFromPath(string path,out int pos1, out int pos2)
        {
            pos1 = 0;
            pos2 = path.IndexOf(Utils.VFOLDERSEP);

            for (int i = pos2; i > 0; i--)
            {
                if (path.ElementAt(i) == Path.DirectorySeparatorChar)
                {
                    pos1 = i + 1;
                    break;
                }
            }
        }

        public static string getFilenameWithoutVF(string fn)
        {
            if (fn.Contains(VFOLDERSEP))
            {
                int pos1 = fn.IndexOf(Utils.VFOLDERSEP);
                string vf = fn.Substring(0, pos1 + 3);
                return fn.Replace(vf, "");
            }
            else
            {
                return fn;
            }
        }

        public static int getNbSavesInVF(string vf)
        {
            try
            {
                string text = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                DirectoryInfo directoryInfo = new DirectoryInfo(text);
                IOrderedEnumerable<FileInfo> res;

                if (vf== "Default")
                {
                    //Return only path which dont contain the virtual folder signature
                    res = from f in directoryInfo.GetFiles()
                               where f.Extension == ".rws" && !f.FullName.Contains(Utils.VFOLDERSEP)
                               orderby f.LastWriteTime descending
                               select f;
                }
                else
                {
                    res = from f in directoryInfo.GetFiles()
                               where f.Extension == ".rws" && f.FullName.Contains(vf + Utils.VFOLDERSEP)
                               orderby f.LastWriteTime descending
                               select f;
                }

                return res.Count();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static void renameCurFolder(string newName)
        {
            string text = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
            DirectoryInfo directoryInfo = new DirectoryInfo(text);
            IOrderedEnumerable<FileInfo> res;

            string curPrefix = Settings.curFolder + Utils.VFOLDERSEP;
            string newPrefix = newName + Utils.VFOLDERSEP;

            //Constitution list of saves storing the specified VF
            res = from f in directoryInfo.GetFiles()
                  where f.Extension == ".rws" && f.FullName.Contains(curPrefix)
                  orderby f.LastWriteTime descending
                  select f;

            //For each of these we will replace the vf by the new one
            foreach (var file in res)
            {
                string newPath = file.FullName;
                newPath = newPath.Replace(curPrefix, newPrefix);

                //Rename backup file
                System.IO.File.Move(file.FullName, newPath);
                //Log.Message(file.FullName + " " + newPath);
            }

            //Constitution list of previews storing the specified VF
            string text2 = Utils.getBasePathRSPreviews();
            directoryInfo = new DirectoryInfo(text2);
            res = from f in directoryInfo.GetFiles()
                  where f.Extension == ".jpg" && f.FullName.Contains(curPrefix)
                  orderby f.LastWriteTime descending
                  select f;

            //Same for the preview
            foreach (var file in res)
            {
                string newPath = file.FullName;
                newPath = newPath.Replace(curPrefix, newPrefix);

                
                System.IO.File.Move(file.FullName, newPath);
                //Updating the preview cache
                Utils.updateCachedPreviewPath(file.FullName, newPath);
            }

            //Same meta
            //Constitution list of previews storing the specified VF
            text2 = Utils.getBasePathRSMeta();
            directoryInfo = new DirectoryInfo(text2);
            res = from f in directoryInfo.GetFiles()
                  where f.Extension == ".dat" && f.FullName.Contains(curPrefix)
                  orderby f.LastWriteTime descending
                  select f;

            //Pareil pour les preview
            foreach (var file in res)
            {
                string newPath = file.FullName;
                newPath = newPath.Replace(curPrefix, newPrefix);


                System.IO.File.Move(file.FullName, newPath);
                //Updating the preview cache
                Utils.updateCachedMetaPath(file.FullName, newPath);
            }

            //Search through records to replace name
            Settings.folders[Settings.folders.FindIndex(ind => ind.Equals(Settings.curFolder))] = newName;
            Settings.curFolder = newName;
            Utils.curModRef.WriteSettings();

        }

        public static bool isValidFilename(string fn)
        {
            return !string.IsNullOrEmpty(fn) &&
              fn.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        public static void logMsg(string msg)
        {
            Log.Message("[RimSaves] " + msg);
        }

        public static bool ControlIsHeld
        {
            get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand); }
        }

        public static bool HomeIsHeld
        {
            get {
                if(Settings.keyBinding == 1)
                    return Input.GetKey(KeyCode.Home);
                else
                    return Input.GetKey(KeyCode.F5);
            }
        }

        public static bool EndIsHeld
        {
            get {
                if (Settings.keyBinding == 1)
                    return Input.GetKey(KeyCode.End);
                else
                    return Input.GetKey(KeyCode.F9);
            }
        }

        public static object Serializer { get; private set; }

        static void serialize(Dictionary<string, string> data, string path)
        {
            string ret= "";
            int nb = data.Count;
            
            foreach (var el in data)
            {
                ret += System.Uri.EscapeDataString(el.Key) + ":" + System.Uri.EscapeDataString(el.Value)+",";
            }

            ret = ret.TrimEnd(',');
            System.IO.File.WriteAllText(path, ret);
        }

        public static Dictionary<string, string> unserialize(string path)
        {
            if (!File.Exists(path))
                return null;

            string text = System.IO.File.ReadAllText(@path);
            string[] parts = text.Split(',');
            Dictionary<string, string> ret = null;
            foreach(var cp in parts)
            {
                if (!cp.Contains(":"))
                    continue;

                string[] parts2 = cp.Split(new[] { ':' }, 2);
                if (ret == null)
                    ret = new Dictionary<string, string>();
                ret.Add(System.Uri.UnescapeDataString(parts2[0]), System.Uri.UnescapeDataString(parts2[1]));
            }
            return ret;
        }

        public static string getUniqueSuffix()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return "."+now.ToUnixTimeMilliseconds().ToString();
        }

        public static List<string> negativeIncidents = null;
        public static List<string> positiveIncidents = null;

        public static GCQS GCQSI;
        public static readonly string VFOLDERSEP = "#§#";
        public static readonly string OPTNSTART = "-            ";
        public static RimSaves curModRef;
        public static readonly string RSRelease = "RimSaves NX";
    }
}
