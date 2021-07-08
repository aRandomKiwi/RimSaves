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

namespace aRandomKiwi.ARS
{
    internal static class Dialog_FileList_Patch
    {
        [HarmonyPatch(typeof(Dialog_FileList), "get_InitialSize")]
        public class get_InitialSize
        {
            [HarmonyPrefix]
            public static bool Listener(Dialog_FileList __instance, ref Vector2 __result)
            {
                __result = new Vector2(1000f, 700f);
                Utils.initDialog = true;
                Utils.selectedSave = "";
                return false;
            }
        }

        [HarmonyPatch(typeof(Dialog_FileList), "DoWindowContents")]
        public class DoWindowContents
        {
            [HarmonyPrefix]
            public static bool Listener(Dialog_FileList __instance, Rect inRect, 
                ref List<SaveFileInfo> ___files, ref float ___bottomAreaHeight, ref Vector2 ___scrollPosition, ref string ___interactButLabel, ref string ___typingName)
            {
                try
                {
                    //Si scenario on override pas le comportement vanilla
                    if (__instance is Dialog_ScenarioList)
                        return true;

                    //Calcul nb champs matchant pour avoir taille ascenceur
                    int nbRow = 0;
                    foreach (SaveFileInfo current in ___files)
                    {
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(current.FileInfo.Name);
                        if (fileNameWithoutExtension.Contains(Utils.VFOLDERSEP))
                            fileNameWithoutExtension = fileNameWithoutExtension.Substring(fileNameWithoutExtension.IndexOf(Utils.VFOLDERSEP) + 3);

                        if (fileNameWithoutExtension.ToLower().Contains(Utils.filter.ToLower()))
                            nbRow++;
                    }


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

                    //Zone de recherche
                    Rect searchSelRect = new Rect(inRect.AtZero());
                    searchSelRect.width = inRect.width - (340f+32f+15f);
                    searchSelRect.height = 32f;
                    searchSelRect.x = 376f;

                    //Icone de recherche
                    Rect searchSelRectIcon = new Rect(inRect.AtZero());
                    searchSelRectIcon.width = 32f;
                    searchSelRectIcon.height = 32f;
                    searchSelRectIcon.x = 340f;


                    //Dessin du selecteur de folder
                    Rect folderSelRect = new Rect(inRect.AtZero());
                    folderSelRect.width = inRect.width - 340f-150f-8f;
                    folderSelRect.x = 374f;
                    folderSelRect.y += 36f;
                    folderSelRect.height = 32f;

                    Rect folderSelRectBtnAdd = new Rect(folderSelRect);
                    folderSelRectBtnAdd.width = 32f;
                    folderSelRectBtnAdd.x = folderSelRect.x+folderSelRect.width+6;

                    Rect folderSelRectBtnEdit = new Rect(folderSelRectBtnAdd);
                    folderSelRectBtnEdit.x += folderSelRectBtnAdd.width + 6;

                    Rect folderSelRectBtnDel = new Rect(folderSelRectBtnEdit);
                    folderSelRectBtnDel.x += folderSelRectBtnEdit.width + 6;


                    //Image de preview
                    Texture2D preview = Tex.texBGNoPreview;

                    //Check si pas de preview pour la save selected
                    if(Utils.selectedSave != "")
                    {
                        string previewpath=Utils.getBasePathRSPreviews();
                        previewpath = Path.Combine(previewpath, Utils.selectedSave)+".jpg";

                       Texture2D tmp = Utils.loadPreview(previewpath);
                        if (tmp != null)
                            preview = tmp;
                    }

                    if(Widgets.ButtonImage(new Rect(0, 0, 335, 220), preview, Color.white, Color.yellow))
                    {
                        Find.WindowStack.Add(new Dialog_ShowPreview(preview));
                    }

                    //Icon de filtre
                    Widgets.ButtonImage(searchSelRectIcon, Tex.texSearch, Color.white, Color.white);
                    //Filtre
                    GUI.SetNextControlName("ARS_SearchBar");
                    Utils.filter = Widgets.TextField(searchSelRect, Utils.filter);
                    if (Utils.initDialog)
                    {
                        if(___bottomAreaHeight <= 10)
                            UI.FocusControl("ARS_SearchBar", __instance);
                        Utils.initDialog = false;
                    }

                    if (__instance is Dialog_SaveFileList_Save)
                    {
                        //__instance.DoTypeInField(inRect.AtZero());
                        Traverse.Create(__instance).Method("DoTypeInField", inRect.AtZero()).GetValue();
                    }

                    //Selected map meta data
                    Rect metaSaveRect = new Rect(0, 230, 335, 400f);
                    metaSaveRect.height -= ___bottomAreaHeight;
                    Rect metaSaveRectData = new Rect(5, 235, 330, 395f);
                    metaSaveRectData.height -= ___bottomAreaHeight;
                    Widgets.DrawLightHighlight(metaSaveRect);

                    //Affichage des meta data le cas echeant
                    Dictionary<string, string> meta = Utils.getSaveMeta(Utils.selectedSave);
                    
                    float metaHeight = (float)nbRow * y;
                    Rect metaOutRect = new Rect(metaSaveRect);
                    metaOutRect.height += 350f;
                    metaOutRect.width -= 25;

                    if (Utils.selectedSave != "")
                    {
                        Widgets.BeginScrollView(metaSaveRectData, ref Utils.metaScrollPosition, metaOutRect, true);

                        Listing_Standard list = new Listing_Standard() { };
                        list.Begin(metaOutRect);

                        //Affichage de la taille
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
                            //Affichage message expliquant qu'il n'y a pas de meta disponible
                            GUI.color = Color.yellow;
                            list.Label("ARS_SavePreviewMissing".Translate());
                            GUI.color = Color.white;
                        }
                        list.End();

                        Widgets.EndScrollView();
                    }

                    //Icone de dossier
                    Widgets.ButtonImage(new Rect(340f, 36f, 32f, 32f), Tex.texFolder, Color.white, Color.white);

                    if (Widgets.ButtonText(folderSelRect, Settings.curFolder+ " " + "ARS_nbSaves".Translate(Utils.getNbSavesInVF(Settings.curFolder))))
                    {
                        Utils.showFolderList(delegate(string cfolder)
                        {
                            //Changement de dossier courant
                            /*Settings.curFolder = cfolder;
                            Utils.curModRef.WriteSettings();
                            Traverse.Create(__instance).Method("ReloadFiles").GetValue();*/
                            if (Settings.curFolder != cfolder)
                                Utils.selectedSave = "";
                            Utils.changeFolder(cfolder, __instance);
                        },"");
                    }
                    

                    if (Widgets.ButtonImage(folderSelRectBtnAdd, Tex.texAdd, Color.white, GenUI.SubtleMouseoverColor))
                    {
                        Find.WindowStack.Add(new Dialog_Input(delegate(string value)
                        {
                            Settings.folders.Add(value);
                            Settings.curFolder =value;
                            Utils.curModRef.WriteSettings();

                            Utils.changeFolder(Settings.curFolder, __instance);
                        },delegate(string value)
                        {
                            //Routine de validation de la saisie
                            //Check si existe pas déjà nom de dossier
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

                    if (Widgets.ButtonImage(folderSelRectBtnEdit, texEdit, Color.white, GenUI.SubtleMouseoverColor))
                    {
                        if (Settings.curFolder != "Default")
                        {
                            Find.WindowStack.Add(new Dialog_Input(delegate (string value)
                            {
                                Utils.renameCurFolder(value);
                                //Si dossier selectionné == dossier déplacé alors reset
                                Utils.selectedSave = "";

                                //Raffraichissement de la vue
                                Utils.changeFolder(Settings.curFolder, __instance);
                            }, delegate (string value)
                            {
                                //Routine de validation de la saisie
                                //Check si existe pas déjà nom de dossier
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
                    TooltipHandler.TipRegion(folderSelRectBtnAdd, "ARS_ToolTipAddFolder".Translate());


                    if (Widgets.ButtonImage(folderSelRectBtnDel, Tex.texDeleteFolder, Color.white, GenUI.SubtleMouseoverColor))
                    {
                        Find.WindowStack.Add(new Dialog_Msg("ARS_ConfirmRemoveFolderTitle".Translate(), "ARS_ConfirmRemoveFolderDesc".Translate(), delegate
                        {
                            //Confirmation de la suppression du dossier virtuel on supprime toutes les saves qui y sont rattachées
                            foreach (FileInfo current in GenFilePaths.AllSavedGameFiles)
                            {
                                current.Delete();
                            }

                            //Suppression de tous les previews associés
                            //Constitution liste des previews stockant le VF specifié
                            string text2 = Utils.getBasePathRSPreviews();
                            string curPrefix = Settings.curFolder + Utils.VFOLDERSEP;
                            DirectoryInfo directoryInfo = new DirectoryInfo(text2);
                            IOrderedEnumerable<FileInfo> res;
                            res = from f in directoryInfo.GetFiles()
                                  where f.Extension == ".jpg" && f.FullName.Contains(curPrefix)
                                  orderby f.LastWriteTime descending
                                  select f;

                            //Pareil pour les previews
                            foreach (var file in res)
                            {
                                file.Delete();
                            }

                            //Pareil pour les meta
                            text2 = Utils.getBasePathRSMeta();
                            curPrefix = Settings.curFolder + Utils.VFOLDERSEP;
                            directoryInfo = new DirectoryInfo(text2);
                            res = from f in directoryInfo.GetFiles()
                                  where f.Extension == ".dat" && f.FullName.Contains(curPrefix)
                                  orderby f.LastWriteTime descending
                                  select f;

                            //Pareil pour les previews
                            foreach (var file in res)
                            {
                                file.Delete();
                            }

                            //Suppression reférence dossier supprimé
                            if (Settings.curFolder != "Default")
                                Settings.folders.Remove(Settings.curFolder);

                            Utils.selectedSave = "";

                            Utils.changeFolder("Default", __instance);
                        }, false, Color.red));
                    }

                    TooltipHandler.TipRegion(folderSelRectBtnDel, "ARS_ToolTipDelFolder".Translate());

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

                            if(prefixedFileName == Utils.selectedSave)
                            {
                                Widgets.DrawHighlightSelected(rect);
                            }

                            GUI.BeginGroup(rect);
                            Rect rect2 = new Rect(rect.width - 36f, (rect.height - 36f) / 2f, 36f, 36f);
                            if (Widgets.ButtonImage(rect2,Tex.texDeleteX2, Color.white, GenUI.SubtleMouseoverColor))
                            {
                                FileInfo localFile = current.FileInfo;
                                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(localFile.Name), delegate
                                {
                                    localFile.Delete();

                                    string prefix = "";
                                    if (Settings.curFolder != "Default")
                                        prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                    //Si preview associée
                                    string pathPreview = Utils.getBasePathRSPreviews();
                                    pathPreview = Path.Combine(pathPreview, prefixedFileName + ".jpg");

                                    if (File.Exists(pathPreview))
                                    {
                                        System.IO.File.Delete(pathPreview);
                                    }

                                    //Si meta associée
                                    string pathMeta = Utils.getBasePathRSMeta();
                                    pathMeta = Path.Combine(pathMeta, prefixedFileName + ".dat");

                                    if (File.Exists(pathMeta))
                                    {
                                        System.IO.File.Delete(pathMeta);
                                    }

                                    //Si dossier selectionné == dossier supprimé alors reset
                                    if (Utils.selectedSave == prefixedFileName)
                                        Utils.selectedSave = "";

                                    Traverse.Create(__instance).Method("ReloadFiles").GetValue();
                                    //__instance.ReloadFiles();
                                }, true, null));
                            }
                            TooltipHandler.TipRegion(rect2, "DeleteThisSavegame".Translate());

                            Rect rect20 = new Rect(rect.width - 72f, (rect.height - 36f) / 2f, 36f, 36f);
                            if (Widgets.ButtonImage(rect20, Tex.texMove, Color.white, GenUI.SubtleMouseoverColor))
                            {
                                Utils.showFolderList(delegate(string cfolder)
                                {
                                    string newFile;
                                    //Fichier stocké dans le Default ?
                                    if (!current.FileInfo.FullName.Contains(Utils.VFOLDERSEP))
                                    {
                                        string dir = Path.GetDirectoryName(current.FileInfo.FullName);
                                        newFile = dir + Path.DirectorySeparatorChar + cfolder + Utils.VFOLDERSEP + current.FileInfo.Name;
                                    }
                                    else
                                    {
                                        string tmpPath = current.FileInfo.FullName;
                                        //Recherche et suppression /xxxx§§§
                                        int pos1;
                                        int pos2;

                                        Utils.getVFPosFromPath(tmpPath, out pos1, out pos2);
                                        
                                        //Si dossier de destination est le default alors on sueeze la signature de VFOLDER
                                        if (cfolder == "Default")
                                            pos2 += 3;

                                        //Log.Message("toSubstitute : " + pos2 + " " + pos1);
                                        string toSubstitute = tmpPath.Substring(pos1, pos2 - pos1);
                                        //Log.Message("toSubstitute : " + toSubstitute);
                                        if (cfolder == "Default")
                                            newFile = tmpPath.Replace(toSubstitute, "");
                                        else
                                            newFile = tmpPath.Replace(toSubstitute, cfolder );
                                    }

                                    //Check existence fichier meme nom dans la destination, le cas echeant on rajoute une particule au fichier a déplacer
                                    if (File.Exists(newFile))
                                    {
                                        string tmp;
                                        for (int j=1; j!=100 ; j++)
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

                                    //Deplacement de la sauvegarde dans le nouveau dossier
                                    //Log.Message(newFile);
                                    System.IO.File.Move(current.FileInfo.FullName,newFile);

                                    //ON MOVE aussi le preview si preview il y a
                                    string pathPreview = Utils.getBasePathRSPreviews();
                                    pathPreview = Path.Combine(pathPreview, prefixedFileName + ".jpg");

                                    if (File.Exists(pathPreview))
                                    {
                                        string newPathPreview = Utils.getBasePathRSPreviews();

                                        string lastPart = newFile.Split(Path.DirectorySeparatorChar).Last();

                                        newPathPreview = Path.Combine(newPathPreview, lastPart);
                                        newPathPreview = Utils.replaceLastOccurrence(newPathPreview,".rws", ".jpg");

                                        System.IO.File.Move(pathPreview, newPathPreview);
                                        //Mise a jour du cache des previews
                                        Utils.updateCachedPreviewPath(pathPreview, newPathPreview);
                                    }

                                    //ON MOVE aussi le meta si preview il y a
                                    string pathMeta = Utils.getBasePathRSMeta();
                                    pathMeta = Path.Combine(pathMeta, prefixedFileName + ".dat");

                                    if (File.Exists(pathMeta))
                                    {
                                        string newPathMeta= Utils.getBasePathRSMeta();

                                        string lastPart = newFile.Split(Path.DirectorySeparatorChar).Last();

                                        newPathMeta = Path.Combine(newPathMeta, lastPart);
                                        newPathMeta = Utils.replaceLastOccurrence(newPathMeta, ".rws", ".dat");

                                        System.IO.File.Move(pathMeta, newPathMeta);
                                        //Mise a jour du cache des previews
                                        Utils.updateCachedMetaPath(pathMeta, newPathMeta);
                                    }

                                    //Si dossier selectionné == dossier déplacé alors reset
                                    if (Utils.selectedSave == prefixedFileName)
                                        Utils.selectedSave = "";

                                    //Rafraichissement de la fenetre
                                    Utils.changeFolder(Settings.curFolder, __instance);

                                },Settings.curFolder);
                            }
                            TooltipHandler.TipRegion(rect20, "ARS_ToolTipMoveSave".Translate());

                            Rect rect21 = new Rect(rect.width - 108f, (rect.height - 36f) / 2f, 36f, 36f);
                            if (Widgets.ButtonImage(rect21, Tex.texMore, Color.white, GenUI.SubtleMouseoverColor))
                            {
                                List<FloatMenuOption> listFO = new List<FloatMenuOption>();
                                listFO.Add(new FloatMenuOption(Utils.OPTNSTART + "ARS_OP_RENAME".Translate(), delegate
                                {
                                    Find.WindowStack.Add(new Dialog_Input(delegate (string value)
                                    {
                                        string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
                                        string prefix = "";
                                        if (Settings.curFolder != "Default")
                                            prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                        path = Path.Combine(path, prefix + value + ".rws");

                                        //Changement de nom effectif
                                        System.IO.File.Move(current.FileInfo.FullName, path);

                                        string pathPreview = Utils.getBasePathRSPreviews();
                                        pathPreview = Path.Combine(pathPreview, prefixedFileName + ".jpg");

                                        if (File.Exists(pathPreview))
                                        {
                                            string newPathPreview = Utils.getBasePathRSPreviews();

                                            if (Settings.curFolder != "Default")
                                                prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                            newPathPreview = Path.Combine(newPathPreview, prefix + value + ".jpg");

                                            System.IO.File.Move(pathPreview, newPathPreview);
                                            //Mise a jour du cache des previews
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
                                            //Mise a jour du cache des previews
                                            Utils.updateCachedMetaPath(pathMeta, newPathMeta);
                                        }

                                        //Si dossier selectionné == dossier déplacé alors reset
                                        if (Utils.selectedSave == prefixedFileName)
                                            Utils.selectedSave = "";

                                        //Rechargement liste des fichiers
                                        Utils.changeFolder(Settings.curFolder, __instance);
                                    }, delegate (string value)
                                    {
                                        //Routine de validation de la saisie
                                        //Check si existe une save de meme nom
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

                                        //Changement de nom effectif
                                        System.IO.File.Copy(current.FileInfo.FullName, path);

                                        //Si preview associée
                                        string pathPreview = Utils.getBasePathRSPreviews();
                                        pathPreview = Path.Combine(pathPreview, prefixedFileName + ".jpg");

                                        if (File.Exists(pathPreview))
                                        {
                                            string newPathPreview = Utils.getBasePathRSPreviews();

                                            if (Settings.curFolder != "Default")
                                                prefix = Settings.curFolder + Utils.VFOLDERSEP;

                                            newPathPreview = Path.Combine(newPathPreview, prefix + value + ".jpg");

                                            System.IO.File.Copy(pathPreview, newPathPreview);
                                        }

                                        //Si meta associée
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

                                        //Rechargement liste des fichiers
                                        Utils.changeFolder(Settings.curFolder, __instance);
                                    }, delegate (string value)
                                    {
                                        //Routine de validation de la saisie
                                        //Check si existe une save de meme nom
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
                            if (Widgets.ButtonText(rect3, ___interactButLabel, true, false, true))
                            {
                                Utils.loadedSave = prefixedFileName;
                                //__instance.DoFileInteraction(Path.GetFileNameWithoutExtension(current.FileInfo.Name));
                                Traverse.Create(__instance).Method("DoFileInteraction", Path.GetFileNameWithoutExtension(current.FileInfo.Name)).GetValue();
                            }
                            Rect rect4 = new Rect(rect3.x - 94f, 0f, 94f, rect.height);
                            Dialog_FileList.DrawDateAndVersion(current, rect4);
                            GUI.color = Color.white;
                            Text.Anchor = TextAnchor.UpperLeft;
                            //GUI.color = __instance.FileNameColor(current);
                            GUI.color = (Color)Traverse.Create(__instance).Method("FileNameColor", current).GetValue();

                            Rect rect5 = new Rect(308f, 0f, rect4.x - 8f - 4f, rect.height);
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Text.Font = GameFont.Small;
                            

                            //Widgets.Label(rect5, fileNameWithoutExtension.Truncate(rect5.width * 1.8f, null));
                            if(Widgets.ButtonText(rect5, fileNameWithoutExtension.Truncate(rect5.width * 1.8f, null),false, false))
                            {
                                Utils.selectedSave = prefixedFileName;
                                ___typingName = fileNameWithoutExtension;
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
                        var dialog = new Dialog_ModSettings();
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
        }
    }
}
