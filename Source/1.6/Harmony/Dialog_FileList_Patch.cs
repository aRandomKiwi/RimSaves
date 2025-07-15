using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Verse.Sound;

namespace aRandomKiwi.ARS
{
    internal static class Dialog_FileList_Patch
    {
        public static bool needReload = false;
        //private static string uniqueSaveNameSuffix = Utils.getUniqueSuffix();
        [HarmonyPatch(typeof(Dialog_FileList), "get_InitialSize")]
        public class get_InitialSize
        {
            [HarmonyPrefix]
            public static bool Listener(Dialog_FileList __instance, ref Vector2 __result, ref string ___typingName)
            {
                if (!(__instance is Dialog_SaveFileList
                    || __instance is Dialog_SaveFileList_Load
                    || __instance is Dialog_SaveFileList_Save))
                {
                    return true;
                }
                Utils.focusedNameArea = false;

                __result = new Vector2(1036f, 700f);
                Utils.initDialog = true;
                Utils.selectedSave = "";
                Utils.selectedSaves.Clear();
                if (Settings.uniqueSaveName) {
                    //If already contain suffix then we remove it and happend the new one
                    Regex rgx = new Regex(@"\.[0-9]{5,}$");
                    if (rgx.IsMatch(___typingName))
                    {
                        ___typingName = rgx.Replace(___typingName, "");
                    }
                    ___typingName += Utils.getUniqueSuffix();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Dialog_FileList), "DoWindowContents")]
        public class DoWindowContents
        {
            [HarmonyPrefix]
            public static bool Listener(Dialog_FileList __instance, Rect inRect, 
                ref List<SaveFileInfo> ___files, ref float ___bottomAreaHeight, ref Vector2 ___scrollPosition, ref string ___interactButLabel, ref string ___typingName, bool ___focusedNameArea)
            {
                try
                {
                    //If scenario we do not override the vanilla behavior
                    if (!(__instance is Dialog_SaveFileList
                    || __instance is Dialog_SaveFileList_Load
                    || __instance is Dialog_SaveFileList_Save))
                        return true;

                    //Calculation of number of fields matching to have ascender size
                    int nbRow = 0;
                    foreach (SaveFileInfo current in ___files)
                    {
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(current.FileInfo.Name);
                        if (fileNameWithoutExtension.Contains(Utils.VFOLDERSEP))
                            fileNameWithoutExtension = fileNameWithoutExtension.Substring(fileNameWithoutExtension.IndexOf(Utils.VFOLDERSEP) + 3);

                        if (fileNameWithoutExtension.ToLower().Contains(Utils.filter.ToLower()))
                            nbRow++;
                    }

                    bool isSaveDialog = (__instance is Dialog_SaveFileList_Save);
                    Vector2 vector = new Vector2(inRect.width-15-40, 40f);
                    inRect.height -= 45f;
                    float y = vector.y;
                    float height = (float)nbRow * y;
                    Rect viewRect = new Rect(300f, 0f, inRect.width - 356f, height );
                    Rect outRect = new Rect(inRect.AtZero());
                    outRect.x = 340;
                    outRect.width = inRect.width - 340f;
                    outRect.height -= ___bottomAreaHeight + 70f;
                    outRect.y += 70f;

                    //Search area
                    Rect searchSelRect = new Rect(inRect.AtZero());
                    searchSelRect.width = inRect.width - (340f+32f+15f);
                    searchSelRect.height = 32f;
                    searchSelRect.x = 376f;

                    //Search icon
                    Rect searchSelRectIcon = new Rect(inRect.AtZero());
                    searchSelRectIcon.width = 32f;
                    searchSelRectIcon.height = 32f;
                    searchSelRectIcon.x = 340f;


                    //Drawing the folder selector
                    Rect folderSelRect = new Rect(inRect.AtZero());
                    folderSelRect.width = inRect.width - 340f-150f-8f-36f;
                    folderSelRect.x = 374f;
                    folderSelRect.y += 36f;
                    folderSelRect.height = 32f;

                    Rect folderSelRectBtnAdd = new Rect(folderSelRect);
                    folderSelRectBtnAdd.width = 32f;
                    folderSelRectBtnAdd.x = folderSelRect.x+folderSelRect.width+4;

                    Rect folderSelRectBtnEdit = new Rect(folderSelRectBtnAdd);
                    folderSelRectBtnEdit.x += folderSelRectBtnAdd.width + 4;

                    Rect folderSelRectBtnMassMove = new Rect(folderSelRectBtnEdit);
                    folderSelRectBtnMassMove.x += folderSelRectBtnEdit.width + 4;

                    Rect folderSelRectBtnDel = new Rect(folderSelRectBtnMassMove);
                    folderSelRectBtnDel.x += folderSelRectBtnMassMove.width + 4;


                    //Preview image
                    Texture2D preview = Tex.texBGNoPreview;

                    //Check if no preview for the save selected
                    if (Utils.selectedSave != "")
                    {
                        string previewpathBase=Utils.getBasePathRSPreviews();
                        string previewpath = "";
                        previewpath = Path.Combine(previewpathBase, Utils.selectedSave)+".dat";
                        if(!File.Exists(previewpath))
                            previewpath = Path.Combine(previewpathBase, Utils.selectedSave) + ".jpg";

                        Texture2D tmp = Utils.loadPreview(previewpath);
                        if (tmp != null)
                            preview = tmp;
                    }

                    if(Widgets.ButtonImage(new Rect(0, 0, 335, 220), preview, Color.white, Color.yellow))
                    {
                        Find.WindowStack.Add(new Dialog_ShowPreview(preview));
                    }

                    //Filter icon
                    Widgets.ButtonImage(searchSelRectIcon, Tex.texSearch, Color.white, Color.white);
                    //Filter
                    GUI.SetNextControlName("ARS_SearchBar");
                    Utils.filter = Widgets.TextField(searchSelRect, Utils.filter);
                    if (Utils.initDialog)
                    {
                        if(___bottomAreaHeight <= 10)
                            UI.FocusControl("ARS_SearchBar", __instance);
                        Utils.initDialog = false;
                    }

                    if (isSaveDialog)
                    {
                        //__instance.DoTypeInField(inRect.AtZero());
                        //Traverse.Create(__instance).Method("DoTypeInField", inRect.AtZero()).GetValue();
                        DoTypeInField(inRect.AtZero(), ref ___typingName,  ref __instance);
                    }

                    //Selected map meta data
                    Rect metaSaveRect = new Rect(0, 230, 335, 400f);
                    metaSaveRect.height -= ___bottomAreaHeight;
                    Rect metaSaveRectData = new Rect(5, 235, 330, 395f);
                    metaSaveRectData.height -= ___bottomAreaHeight;
                    if (isSaveDialog)
                    {
                        metaSaveRect.height -= 50;
                        metaSaveRectData.height -= 50;
                    }
                    Widgets.DrawLightHighlight(metaSaveRect);

                    //Display of meta data if applicable
                    Dictionary<string, string> meta = Utils.getSaveMeta(Utils.selectedSave);
                    
                    float metaHeight = (float)nbRow * y;
                    Rect metaOutRect = new Rect(metaSaveRect);
                    metaOutRect.height += 350f;
                    metaOutRect.width -= 25;

                    if (isSaveDialog)
                    {
                        metaOutRect.height -= 50;
                    }

                    if (Utils.selectedSave != "")
                    {
                        Widgets.BeginScrollView(metaSaveRectData, ref Utils.metaScrollPosition, metaOutRect, true);

                        Listing_Standard list = new Listing_Standard() { };
                        list.Begin(metaOutRect);

                        //Size display
                        long length = new System.IO.FileInfo(Path.Combine(Utils.getBasePathSaves(), Utils.selectedSave + ".rws")).Length;
                        GUI.color = Color.cyan;
                        list.Label("ARS_SavePreviewSize".Translate(Utils.FormatSize(length)));
                        GUI.color = Color.white;


                        if (meta != null)
                        {
                            string data = "";

                            foreach (var k in Utils.metaFields)
                            {
                                if (!meta.ContainsKey(k))
                                    data = "";
                                else
                                    data = meta[k];

                                list.Label(("ARS_SavePreview" + k.CapitalizeFirst()).Translate(data));
                            }

                            list.Label("ARS_SavePreviewLastIncident".Translate());

                            if (!meta.ContainsKey("lastIncident"))
                                data = "";
                            else
                                data = meta["lastIncident"];
                            if (data != "")
                            {
                                string[] parts = data.Split('\n');
                                foreach (var cp in parts)
                                {
                                    if (!cp.Contains('-'))
                                        continue;
                                    string[] parts2 = cp.Split(new[] { '-' }, 2);
                                    if (parts2.Count() != 2)
                                        continue;
                                    if (!parts2[0].Contains(','))
                                        continue;
                                    string[] colors = parts2[0].Split(',');
                                    if (colors.Count() != 3)
                                        continue;
                                    Color c = new Color(float.Parse(colors[0], CultureInfo.InvariantCulture), float.Parse(colors[1], CultureInfo.InvariantCulture), float.Parse(colors[2], CultureInfo.InvariantCulture));
                                    GUI.color = c;
                                    list.Label("-" + parts2[1]);
                                    GUI.color = Color.white;
                                }
                            }
                            GUI.color = Color.white;
                        }
                        else
                        {
                            //Message display explaining that there is no meta available
                            GUI.color = Color.yellow;
                            list.Label("ARS_SavePreviewMissing".Translate());
                            GUI.color = Color.white;
                        }
                        list.End();

                        Widgets.EndScrollView();
                    }

                    //Folder icon
                    Widgets.ButtonImage(new Rect(340f, 36f, 32f, 32f), Tex.texFolder, Color.white, Color.white);
                    GUI.color = Color.green;
                    if (Widgets.ButtonText(folderSelRect, Settings.curFolder+ " " + "ARS_nbSaves".Translate(Utils.getNbSavesInVF(Settings.curFolder))))
                    {
                        Utils.showFolderList(delegate(string cfolder)
                        {
                            //Change of current file
                            /*Settings.curFolder = cfolder;
                            Utils.curModRef.WriteSettings();
                            Traverse.Create(__instance).Method("ReloadFiles").GetValue();*/
                            if (Settings.curFolder != cfolder)
                            {
                                Utils.selectedSave = "";
                                Utils.selectedSaves.Clear();
                            }
                            Utils.changeFolder(cfolder, __instance);
                        },"");
                    }
                    GUI.color = Color.white;
                    if (Widgets.ButtonImageWithBG(folderSelRectBtnAdd, Tex.texAdd, new Vector2(24,24)))
                    {
                        Find.WindowStack.Add(new Dialog_Input(delegate(string value)
                        {
                            Settings.folders.Add(value);
                            Settings.curFolder =value;
                            Utils.curModRef.WriteSettings();

                            Utils.changeFolder(Settings.curFolder, __instance);
                        },delegate(string value)
                        {
                            //Entry validation routine
                            //Check if the folder name does not already exist
                            foreach (string el in Settings.folders)
                            {
                                if (el == value)
                                {
                                    Messages.Message("ARS_FolderAlreadyExist".Translate(), MessageTypeDefOf.RejectInput, false);
                                    return false;
                                }
                            }
                            return true;
                        }));
                    }
                    TooltipHandler.TipRegion(folderSelRectBtnAdd, "ARS_ToolTipAddFolder".Translate());

                    Texture2D texEdit = Tex.texEdit;
                    if (Settings.curFolder == "Default")
                        texEdit = Tex.texEditDisabled;

                    if (Widgets.ButtonImageWithBG(folderSelRectBtnEdit, texEdit, new Vector2(24, 24)))
                    {
                        if (Settings.curFolder != "Default")
                        {
                            Find.WindowStack.Add(new Dialog_Input(delegate (string value)
                            {
                                Utils.renameCurFolder(value);
                                //If folder selected == folder moved then reset
                                Utils.selectedSave = "";
                                Utils.selectedSaves.Clear();

                                //Refreshing the view
                                Utils.changeFolder(Settings.curFolder, __instance);
                            }, delegate (string value)
                            {
                                //Entry validation routine
                                //Check if the folder name does not already exist
                                foreach (string el in Settings.folders)
                                {
                                    if (el == value)
                                    {
                                        Messages.Message("ARS_FolderAlreadyExist".Translate(), MessageTypeDefOf.RejectInput, false);
                                        return false;
                                    }
                                }
                                return true;
                            }, Settings.curFolder));
                        }
                    }
                    TooltipHandler.TipRegion(folderSelRectBtnEdit, "ARS_ToolTipEditFolderName".Translate());

                    Texture2D texMove = Tex.texMove;
                    bool massMoveAllowed = true;
                    if (Utils.selectedSaves.Count == 0)
                    {
                        texMove = Tex.texMoveDisabled;
                        massMoveAllowed = false;
                    }

                    if (Widgets.ButtonImageWithBG(folderSelRectBtnMassMove, texMove, new Vector2(24, 24)) && massMoveAllowed)
                    {
                        Utils.showFolderList(delegate (string cfolder)
                        {
                            Utils.selectedSavesToMove.AddRange(Utils.selectedSaves);
                            Utils.selectedSavesToMoveFolder = cfolder;
                            Utils.selectedSaves.Clear();
                        }, Settings.curFolder);
                    }
                    TooltipHandler.TipRegion(folderSelRectBtnMassMove, "ARS_MassMoveSelectedSaves".Translate());

                    Texture2D delTex = Tex.texDeleteFolder;
                    if (Utils.selectedSaves.Count != 0)
                        delTex = Tex.texDeleteSelected;

                    if (Widgets.ButtonImageWithBG(folderSelRectBtnDel, delTex, new Vector2(24, 24)))
                    {
                        if (Utils.selectedSaves.Count != 0)
                        {
                            Find.WindowStack.Add(new Dialog_Msg("ARS_ConfirmRemoveSelectedFilesTitle".Translate(), "ARS_ConfirmRemoveSelectedFilesDesc".Translate(Utils.selectedSaves.Count), delegate
                            {
                                Utils.selectedSavesToDelete.AddRange(Utils.selectedSaves);
                                Utils.selectedSaves.Clear();
                            }, false, Color.red));
                        }
                        else
                        {
                            Find.WindowStack.Add(new Dialog_Msg("ARS_ConfirmRemoveFolderTitle".Translate(), "ARS_ConfirmRemoveFolderDesc".Translate(), delegate
                            {
                                //Confirmation of the deletion of the virtual folder we delete all the saves which are attached to it
                                foreach (FileInfo current in GenFilePaths.AllSavedGameFiles)
                                {
                                    current.Delete();
                                }

                                //Delete all associated previews
                                //Constitution list of previews storing the specified VF
                                string text2 = Utils.getBasePathRSPreviews();
                                string curPrefix = "";

                                if (Settings.curFolder != "Default")
                                    curPrefix = Settings.curFolder + Utils.VFOLDERSEP;

                                DirectoryInfo directoryInfo = new DirectoryInfo(text2);
                                IOrderedEnumerable<FileInfo> res;
                                //Handling default folder removing
                                if (curPrefix == "")
                                {
                                    res = from f in directoryInfo.GetFiles()
                                          where (f.Extension == ".jpg" || f.Extension == ".dat") && !f.FullName.Contains(Utils.VFOLDERSEP)
                                          orderby f.LastWriteTime descending
                                          select f;
                                }
                                else
                                {
                                    res = from f in directoryInfo.GetFiles()
                                          where (f.Extension == ".jpg" || f.Extension == ".dat") && f.FullName.Contains(curPrefix)
                                          orderby f.LastWriteTime descending
                                          select f;
                                }

                                //Same for previews
                                foreach (var file in res)
                                {
                                    file.Delete();
                                }

                                //Same for meta
                                text2 = Utils.getBasePathRSMeta();
                                curPrefix = "";

                                if (Settings.curFolder != "Default")
                                    curPrefix = Settings.curFolder + Utils.VFOLDERSEP;

                                directoryInfo = new DirectoryInfo(text2);

                                if (curPrefix == "")
                                {
                                    res = from f in directoryInfo.GetFiles()
                                          where f.Extension == ".dat" && !f.FullName.Contains(Utils.VFOLDERSEP)
                                          orderby f.LastWriteTime descending
                                          select f;
                                }
                                else
                                {
                                    res = from f in directoryInfo.GetFiles()
                                          where f.Extension == ".dat" && f.FullName.Contains(curPrefix)
                                          orderby f.LastWriteTime descending
                                          select f;
                                }

                                foreach (var file in res)
                                {
                                    file.Delete();
                                }

                                // Delete deleted folder reference
                                if (Settings.curFolder != "Default")
                                    Settings.folders.Remove(Settings.curFolder);

                                Utils.selectedSave = "";
                                Utils.selectedSaves.Clear();

                                Utils.changeFolder("Default", __instance);
                            }, false, Color.red));
                        }
                    }

                    if (Utils.selectedSaves.Count != 0)
                        TooltipHandler.TipRegion(folderSelRectBtnDel, "ARS_ToolTipDelSelSaves".Translate());
                    else
                        TooltipHandler.TipRegion(folderSelRectBtnDel, "ARS_ToolTipDelFolder".Translate());

                    GUI.color = Color.white;
                    if (isSaveDialog)
                    {
                        outRect.height -= 45;
                        viewRect.height -= 45;
                    }

                    Widgets.BeginScrollView(outRect, ref ___scrollPosition, viewRect, true);
                    float num = 0f;
                    int num2 = 0;
                    bool first = true;

                    foreach (SaveFileInfo current in ___files)
                    {
                        if (num + vector.y >= ___scrollPosition.y && num <= ___scrollPosition.y + outRect.height)
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(current.FileInfo.Name);
                            string prefixedFileName = Utils.addPrefix(fileNameWithoutExtension, false);
                            if (fileNameWithoutExtension.Contains(Utils.VFOLDERSEP))
                                fileNameWithoutExtension = fileNameWithoutExtension.Substring(fileNameWithoutExtension.IndexOf(Utils.VFOLDERSEP) + 3);

                            if(!fileNameWithoutExtension.ToLower().Contains(Utils.filter.ToLower()))
                                continue;

                            Rect rect = new Rect(0f, num, vector.x, vector.y);
                            if (num2 % 2 == 0)
                            {
                                Widgets.DrawAltRect(rect);
                            }
                            Widgets.DrawHighlightIfMouseover(rect);

                            if(prefixedFileName == Utils.selectedSave || Utils.selectedSaves.Contains(prefixedFileName))
                            {
                                Widgets.DrawHighlightSelected(rect);
                            }

                            GUI.BeginGroup(rect);

                            Texture2D texSel = Tex.texUnSel;
                            if (Utils.selectedSaves.Contains(prefixedFileName))
                                texSel = Tex.texSel;
                            Rect rectSelection = new Rect(rect.width - 36f, (rect.height - 36f) / 2f, 36f, 36f);
                            if (Widgets.ButtonImage(rectSelection, texSel, Color.white, GenUI.SubtleMouseoverColor))
                            {
                                if (Utils.selectedSaves.Contains(prefixedFileName))
                                {
                                    Utils.selectedSaves.Remove(prefixedFileName);
                                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                                }
                                else
                                {
                                    Utils.selectedSaves.Add(prefixedFileName);
                                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                                }
                            }
                            TooltipHandler.TipRegion(rectSelection, "ARS_SelectUnSelectSave".Translate());

                            Rect rect2 = new Rect(rect.width - 72f, (rect.height - 28f) / 2f, 28f, 28f);
                            bool needToBeDeleted = Utils.selectedSavesToDelete.Contains(prefixedFileName);
                            if (needToBeDeleted || (!Settings.enableLiteMode && Widgets.ButtonImage(rect2,Tex.texDeleteX2, Color.white, GenUI.SubtleMouseoverColor)))
                            {
                                FileInfo localFile = current.FileInfo;
                                Action confirm = delegate
                                {
                                    if (Utils.selectedSaves.Contains(prefixedFileName))
                                        Utils.selectedSaves.Remove(prefixedFileName);

                                    localFile.Delete();

                                    string prefix = "";
                                    if (Settings.curFolder != "Default")
                                        prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                    //If preview associated
                                    string pathPreviewBase = Utils.getBasePathRSPreviews();
                                    string pathPreview = "";
                                    pathPreview = Path.Combine(pathPreviewBase, prefixedFileName + ".dat");
                                    if(!File.Exists(pathPreview))
                                        pathPreview = Path.Combine(pathPreviewBase, prefixedFileName + ".jpg");

                                    if (File.Exists(pathPreview))
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(pathPreview);
                                        }
                                        catch (UnauthorizedAccessException)
                                        {
                                            Log.Message("Cannot delete "+pathPreview);
                                        }
                                    }

                                    //If associated meta
                                    string pathMeta = Utils.getBasePathRSMeta();
                                    pathMeta = Path.Combine(pathMeta, prefixedFileName + ".dat");

                                    if (File.Exists(pathMeta))
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(pathMeta);
                                        }
                                        catch (UnauthorizedAccessException)
                                        {
                                            Log.Message("Cannot delete "+ pathMeta);
                                        }
                                    }

                                    //If folder selected == folder deleted then reset
                                    if (Utils.selectedSave == prefixedFileName)
                                    {
                                        Utils.selectedSave = "";
                                        if (Utils.selectedSaves.Contains(prefixedFileName))
                                            Utils.selectedSaves.Remove(prefixedFileName);
                                    }

                                    if (needToBeDeleted)
                                    {
                                        if (Utils.selectedSavesToDelete.Contains(prefixedFileName))
                                            Utils.selectedSavesToDelete.Remove(prefixedFileName);
                                        if (Utils.selectedSaves.Contains(prefixedFileName))
                                            Utils.selectedSaves.Remove(prefixedFileName);

                                        //Widgets.EndScrollView();
                                        needReload = true;
                                    }
                                    else
                                    {
                                        Traverse.Create(__instance).Method("ReloadFiles").GetValue();
                                    }
                                    //__instance.ReloadFiles();
                                };
                                if (needToBeDeleted)
                                {
                                    confirm();
                                    continue;
                                }
                                else
                                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(localFile.Name), confirm, true, null, WindowLayer.SubSuper));
                            }
                            if(!Settings.enableLiteMode)
                                TooltipHandler.TipRegion(rect2, "DeleteThisSavegame".Translate());

                            Rect rect20 = new Rect(rect.width - 108f, (rect.height - 36f) / 2f, 36f, 36f);
                            bool concernedByMassMove = Utils.selectedSavesToMove.Contains(prefixedFileName);
                            if (concernedByMassMove || (!Settings.enableLiteMode && Widgets.ButtonImage(rect20, Tex.texMove, Color.white, GenUI.SubtleMouseoverColor)))
                            {
                                Action<string> confirm = delegate (string cfolder)
                                {
                                    if (Utils.selectedSaves.Contains(prefixedFileName))
                                        Utils.selectedSaves.Remove(prefixedFileName);

                                    string newFile;
                                    //File stored in Default?
                                    if (!current.FileInfo.FullName.Contains(Utils.VFOLDERSEP))
                                    {
                                        string dir = Path.GetDirectoryName(current.FileInfo.FullName);
                                        newFile = dir + Path.DirectorySeparatorChar + cfolder + Utils.VFOLDERSEP + current.FileInfo.Name;
                                    }
                                    else
                                    {
                                        string tmpPath = current.FileInfo.FullName;
                                        //Search and delete / xxxx§§§
                                        int pos1;
                                        int pos2;

                                        Utils.getVFPosFromPath(tmpPath, out pos1, out pos2);

                                        //If destination folder is the default then we sueeze the signature of VFOLDER
                                        if (cfolder == "Default")
                                            pos2 += 3;

                                        //Log.Message("toSubstitute : " + pos2 + " " + pos1);
                                        string toSubstitute = tmpPath.Substring(pos1, pos2 - pos1);
                                        //Log.Message("toSubstitute : " + toSubstitute);
                                        if (cfolder == "Default")
                                            newFile = tmpPath.Replace(toSubstitute, "");
                                        else
                                            newFile = tmpPath.Replace(toSubstitute, cfolder);
                                    }

                                    //Check the existence of a file with the same name in the destination, if necessary we add a particle to the file to be moved
                                    if (File.Exists(newFile))
                                    {
                                        string tmp;
                                        for (int j = 1; j != 100; j++)
                                        {
                                            tmp = newFile;
                                            tmp = tmp.Replace(".rws", " #" + j + ".rws");
                                            if (!File.Exists(tmp))
                                            {
                                                Messages.Message("ARS_WarningFileMoveAlreadyExist".Translate(Path.GetFileName(newFile), Path.GetFileName(tmp)), MessageTypeDefOf.NeutralEvent, false);
                                                newFile = tmp;
                                                break;
                                            }
                                        }
                                    }


                                    //Move the backup to the new folder
                                    //Log.Message(newFile);
                                    System.IO.File.Move(current.FileInfo.FullName, newFile);

                                    //We MOVE also the preview if there is preview
                                    string pathPreviewBase = Utils.getBasePathRSPreviews();
                                    string pathPreview = "";
                                    pathPreview = Path.Combine(pathPreviewBase, prefixedFileName + ".dat");
                                    if(!File.Exists(pathPreview))
                                        pathPreview = Path.Combine(pathPreviewBase, prefixedFileName + ".jpg");

                                    if (File.Exists(pathPreview))
                                    {
                                        string newPathPreview = Utils.getBasePathRSPreviews();

                                        string lastPart = newFile.Split(Path.DirectorySeparatorChar).Last();

                                        newPathPreview = Path.Combine(newPathPreview, lastPart);
                                        newPathPreview = Utils.replaceLastOccurrence(newPathPreview, ".rws", ".dat");

                                        try
                                        {
                                            System.IO.File.Move(pathPreview, newPathPreview);
                                        }
                                        catch (UnauthorizedAccessException)
                                        {
                                            System.IO.File.Copy(pathPreview, newPathPreview);
                                        }
                                        //Updating the preview cache
                                        Utils.updateCachedPreviewPath(pathPreview, newPathPreview);
                                    }
                                    //We MOVE also the meta if there is preview
                                    string pathMeta = Utils.getBasePathRSMeta();
                                    pathMeta = Path.Combine(pathMeta, prefixedFileName + ".dat");

                                    if (File.Exists(pathMeta))
                                    {
                                        string newPathMeta = Utils.getBasePathRSMeta();

                                        string lastPart = newFile.Split(Path.DirectorySeparatorChar).Last();

                                        newPathMeta = Path.Combine(newPathMeta, lastPart);
                                        newPathMeta = Utils.replaceLastOccurrence(newPathMeta, ".rws", ".dat");
                                        try
                                        {
                                            System.IO.File.Move(pathMeta, newPathMeta);
                                        }
                                        catch (UnauthorizedAccessException)
                                        {
                                            System.IO.File.Copy(pathMeta, newPathMeta);
                                        }
                                        //Updating the preview cache
                                        Utils.updateCachedMetaPath(pathMeta, newPathMeta);
                                    }

                                    //Si dossier selectionné == dossier déplacé alors reset
                                    if (Utils.selectedSave == prefixedFileName)
                                    {
                                        Utils.selectedSave = "";
                                        Utils.selectedSaves.Clear();
                                    }

                                    //Window refresh
                                    if (concernedByMassMove)
                                    {
                                        needReload = true;
                                        if (Utils.selectedSavesToMove.Contains(prefixedFileName))
                                            Utils.selectedSavesToMove.Remove(prefixedFileName);
                                        if (Utils.selectedSavesToMove.Count == 0)
                                            Utils.selectedSavesToMoveFolder = "";
                                    }
                                    else
                                        Utils.changeFolder(Settings.curFolder, __instance);
                                };
                                if (concernedByMassMove)
                                {
                                    confirm(Utils.selectedSavesToMoveFolder);
                                    continue;
                                }
                                else
                                    Utils.showFolderList(confirm, Settings.curFolder);
                            }
                            if(!Settings.enableLiteMode)
                                TooltipHandler.TipRegion(rect20, "ARS_ToolTipMoveSave".Translate());

                            Rect rect21;
                            if (Settings.enableLiteMode)
                                rect21 = rect2;
                            else
                                rect21 = new Rect(rect.width - 144f, (rect.height - 36f) / 2f, 36f, 36f);

                            if (Widgets.ButtonImage(rect21, Tex.texMore, Color.white, GenUI.SubtleMouseoverColor))
                            {
                                List<FloatMenuOption> listFO = new List<FloatMenuOption>();

                                if (Settings.enableLiteMode)
                                {
                                    listFO.Add(new FloatMenuOption(Utils.OPTNSTART + ___interactButLabel, delegate
                                    {
                                        Utils.saveToLoad = prefixedFileName;
                                        //__instance.DoFileInteraction(Path.GetFileNameWithoutExtension(current.FileInfo.Name));
                                        Traverse.Create(__instance).Method("DoFileInteraction", Path.GetFileNameWithoutExtension(current.FileInfo.Name)).GetValue();
                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));

                                    listFO.Add(new FloatMenuOption(Utils.OPTNSTART + "ARS_ToolTipMoveSave".Translate(), delegate
                                    {
                                        Utils.showFolderList(delegate (string cfolder)
                                        {
                                            Utils.selectedSavesToMove.Add(prefixedFileName);
                                            Utils.selectedSavesToMoveFolder = cfolder;
                                        }, Settings.curFolder);
                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));

                                    listFO.Add(new FloatMenuOption(Utils.OPTNSTART + "DeleteThisSavegame".Translate(), delegate
                                    {

                                        FileInfo localFile = current.FileInfo;
                                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(localFile.Name), delegate
                                        {
                                            Utils.selectedSavesToDelete.Add(prefixedFileName);
                                        }, true, null, WindowLayer.SubSuper));
                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                                }

                                listFO.Add(new FloatMenuOption(Utils.OPTNSTART + "ARS_OP_RENAME".Translate(), delegate
                                {
                                    Find.WindowStack.Add(new Dialog_Input(delegate (string value)
                                    {
                                        string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                                        string prefix = "";
                                        if (Settings.curFolder != "Default")
                                            prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                        path = Path.Combine(path, prefix + value + ".rws");

                                        //Effective name change
                                        System.IO.File.Move(current.FileInfo.FullName, path);

                                        string pathPreviewBase = Utils.getBasePathRSPreviews();
                                        string pathPreview = "";
                                        pathPreview = Path.Combine(pathPreviewBase, prefixedFileName + ".dat");
                                        if (!File.Exists(pathPreview))
                                            pathPreview = Path.Combine(pathPreviewBase, prefixedFileName + ".jpg");

                                        if (File.Exists(pathPreview))
                                        {
                                            string newPathPreview = Utils.getBasePathRSPreviews();

                                            if (Settings.curFolder != "Default")
                                                prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                            newPathPreview = Path.Combine(newPathPreview, prefix + value + ".dat");

                                            System.IO.File.Move(pathPreview, newPathPreview);
                                            //Updating the preview cache
                                            Utils.updateCachedPreviewPath(pathPreview, newPathPreview);
                                        }

                                        string pathMeta = Utils.getBasePathRSMeta();
                                        pathMeta = Path.Combine(pathMeta, prefixedFileName + ".dat");

                                        if (File.Exists(pathMeta))
                                        {
                                            string newPathMeta = Utils.getBasePathRSMeta();

                                            if (Settings.curFolder != "Default")
                                                prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                            newPathMeta = Path.Combine(newPathMeta, prefix + value + ".dat");

                                            System.IO.File.Move(pathMeta, newPathMeta);
                                            //Updating the preview cache
                                            Utils.updateCachedMetaPath(pathMeta, newPathMeta);
                                        }

                                        //Prevent ghost selected save
                                        if (Utils.selectedSaves.Contains(prefixedFileName))
                                            Utils.selectedSaves.Remove(prefixedFileName);

                                        //If folder selected == folder moved then reset
                                        if (Utils.selectedSave == prefixedFileName)
                                        {
                                            Utils.selectedSave = "";
                                            Utils.selectedSaves.Clear();
                                        }

                                        //Reload file list
                                        Utils.changeFolder(Settings.curFolder, __instance);
                                    }, delegate (string value)
                                    {
                                        //Entry validation routine
                                        //Check if there is a save with the same name
                                        string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                                        string prefix = "";
                                        if (Settings.curFolder != "Default")
                                            prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                        path = Path.Combine(path, prefix + value + ".rws");
                                        if (File.Exists(path))
                                        {
                                            Messages.Message("ARS_SaveAlreadyExist".Translate(), MessageTypeDefOf.RejectInput, false);
                                            return false;
                                        }
                                        return true;
                                    }, Utils.getFilenameWithoutVF(Path.GetFileNameWithoutExtension(current.FileInfo.FullName))));

                                }, MenuOptionPriority.Default, null, null, 0f, null, null));

                                listFO.Add(new FloatMenuOption(Utils.OPTNSTART + "ARS_OP_DUPLICATE".Translate(), delegate
                                {
                                    Find.WindowStack.Add(new Dialog_Input(delegate (string value)
                                    {
                                        string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                                        string prefix = "";
                                        if (Settings.curFolder != "Default")
                                            prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                        path = Path.Combine(path, prefix + value + ".rws");

                                        //Effective name change
                                        System.IO.File.Copy(current.FileInfo.FullName, path);

                                        //If preview associated
                                        string pathPreviewBase = Utils.getBasePathRSPreviews();
                                        string pathPreview = "";
                                        pathPreview = Path.Combine(pathPreviewBase, prefixedFileName + ".dat");
                                        if (!File.Exists(pathPreview))
                                            pathPreview = Path.Combine(pathPreviewBase, prefixedFileName + ".jpg");

                                        if (File.Exists(pathPreview))
                                        {
                                            string newPathPreview = Utils.getBasePathRSPreviews();

                                            if (Settings.curFolder != "Default")
                                                prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                            newPathPreview = Path.Combine(newPathPreview, prefix + value + ".dat");

                                            System.IO.File.Copy(pathPreview, newPathPreview);
                                        }

                                        //If associated meta
                                        string pathMeta = Utils.getBasePathRSMeta();
                                        pathMeta = Path.Combine(pathMeta, prefixedFileName + ".dat");

                                        if (File.Exists(pathMeta))
                                        {
                                            string newPathMeta= Utils.getBasePathRSMeta();

                                            if (Settings.curFolder != "Default")
                                                prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                            newPathMeta = Path.Combine(newPathMeta, prefix + value + ".dat");

                                            System.IO.File.Copy(pathMeta, newPathMeta);
                                        }

                                        //Reload file list
                                        Utils.changeFolder(Settings.curFolder, __instance);
                                    }, delegate (string value)
                                    {
                                        //Entry validation routine
                                        //Check if there is a save with the same name
                                        string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                                        string prefix = "";
                                        if (Settings.curFolder != "Default")
                                            prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                        path = Path.Combine(path, prefix + value + ".rws");
                                        if (File.Exists(path))
                                        {
                                            Messages.Message("ARS_SaveAlreadyExist".Translate(), MessageTypeDefOf.RejectInput, false);
                                            return false;
                                        }
                                        return true;
                                    }, Utils.getFilenameWithoutVF(Path.GetFileNameWithoutExtension(current.FileInfo.FullName))));

                                }, MenuOptionPriority.Default, null, null, 0f, null, null));

                                Find.WindowStack.Add(new FloatMenu(listFO));
                            }

                            Text.Font = GameFont.Small;
                            Rect rect3 = new Rect(rect21.x - 100f, (rect.height - 36f) / 2f, 100f, 36f);
                            if (!Settings.enableLiteMode && Widgets.ButtonText(rect3, ___interactButLabel, true, false, true))
                            {
                                Utils.saveToLoad = prefixedFileName;
                                //__instance.DoFileInteraction(Path.GetFileNameWithoutExtension(current.FileInfo.Name));
                                Traverse.Create(__instance).Method("DoFileInteraction", Path.GetFileNameWithoutExtension(current.FileInfo.Name)).GetValue();
                            }
                            Rect rect4;
                            if (Settings.enableLiteMode)
                                rect4 = new Rect(rect2.x - 94f, 0f, 94f, rect.height);
                            else
                                rect4 = new Rect(rect3.x - 94f, 0f, 94f, rect.height);

                            Dialog_FileList.DrawDateAndVersion(current, rect4);
                            GUI.color = Color.white;
                            Text.Anchor = TextAnchor.UpperLeft;
                            //GUI.color = __instance.FileNameColor(current);
                            GUI.color = (Color)Traverse.Create(__instance).Method("FileNameColor", current).GetValue();

                            Rect rect5;
                            string tfFN;
                            if (Settings.enableLiteMode)
                            {
                                rect5 = new Rect(308f, 0f, rect4.x - 308f, rect.height);
                                tfFN = fileNameWithoutExtension.Truncate(rect4.x - 308f - 80f, null);
                            }
                            else
                            {
                                tfFN = fileNameWithoutExtension.Truncate(240f, null);
                                rect5 = new Rect(308f, 0f, rect4.x - 320f, rect.height);
                            }
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Text.Font = GameFont.Small;


                            //Widgets.Label(rect5, fileNameWithoutExtension.Truncate(rect5.width * 1.8f, null));
                            if (Widgets.ButtonText(rect5, tfFN,false, false))
                            {
                                Utils.selectedSave = prefixedFileName;
                                ___typingName = fileNameWithoutExtension;
                            }
                            if (tfFN.EndsWith("...")) {
                                TooltipHandler.TipRegion(rect5, fileNameWithoutExtension);
                            }
                            GUI.color = Color.white;
                            Text.Anchor = TextAnchor.UpperLeft;
                            GUI.EndGroup();

                            if (first && Utils.selectedSave == "")
                                Utils.selectedSave = prefixedFileName;
                        }
                        num += vector.y;
                        num2++;
                    }
                    Widgets.EndScrollView();
                    //If mass delete op then reload file list
                    if (needReload)
                    {
                        needReload = false;
                        //Traverse.Create(__instance).Method("ReloadFiles").GetValue();
                        Utils.changeFolder(Settings.curFolder, __instance);
                    }


                    Rect saveFolder = new Rect(0, inRect.height + 15, 32, 32);
                    if (Widgets.ButtonImage(saveFolder, Tex.texFolder2, Color.white, Color.green))
                    {
                        string text = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                        DirectoryInfo directoryInfo = new DirectoryInfo(text);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        //Open the save folder
                        Process.Start(text);
                    }
                    TooltipHandler.TipRegion(saveFolder, "ARS_OpenSaveFolder".Translate());

                    if (Widgets.ButtonImage(new Rect(inRect.width - 168, inRect.height + 5, 167, 40), Tex.texLogo, Color.white, Color.green))
                    {
                        var dialog = new Dialog_ModSettings(Utils.curModRef);

                        Traverse.Create(dialog).Field<Mod>("selMod").Value = Utils.curModRef;
                        Find.WindowStack.Add(dialog);
                    }

                    return false;
                }
                catch(Exception e)
                {
                    Utils.logMsg("Error : " + e.Message+"\nStackTrace : "+e.StackTrace);
                    return true;
                }
            }

            

            static private void DoTypeInField(Rect rect, ref string typingName, ref Dialog_FileList instance)
            {
                Widgets.BeginGroup(rect);
                bool flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
                float y = rect.height - 35f;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.SetNextControlName("MapNameField");
                string str = Widgets.TextField(new Rect(5f, y, 400f, 35f), typingName, Settings.maxSaveCharLength);

                if (!(str.Length > Settings.maxSaveCharLength))
                {
                    typingName = str;
                }

                if (!Utils.focusedNameArea)
                {
                    Utils.focusedNameArea = true;
                    UI.FocusControl("MapNameField", instance);
                }
                if (Widgets.ButtonText(new Rect(420f, y, rect.width - 400f - 20f, 35f), "SaveGameButton".Translate()) || flag)
                {
                    if (typingName.NullOrEmpty())
                    {
                        Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        Traverse.Create(instance).Method("DoFileInteraction", typingName?.Trim()).GetValue();
                        //DoFileInteraction(typingName?.Trim());
                    }
                }
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.EndGroup();
            }
        }
    }
}
