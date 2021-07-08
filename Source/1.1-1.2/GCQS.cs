using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using UnityEngine;
using Verse.Profile;
using System.IO;

namespace aRandomKiwi.ARS
{
    public class GCQS : GameComponent
    {

        public GCQS(Game game)
        {
            this.game = game;
            Utils.GCQSI = this;

            Utils.negativeIncidents = new List<string>
            {
                LetterDefOf.NegativeEvent.defName, LetterDefOf.ThreatBig.defName, LetterDefOf.ThreatSmall.defName
            };
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
        }

        public override void GameComponentUpdate()
        {
            if (Utils.loadedSave != null && Utils.loadedSave != "")
            {
                string loadedSaveLocal = Utils.loadedSave;

                //Initial screenshot at game loading
                ScreenRecorder.screenshotSaved = false;
                ScreenRecorder.saveName = Utils.addPrefix(Utils.loadedSave, false);
                ScreenRecorder.wantScreenShot = true;

                Utils.loadedSave = null;
                try
                {
                    Utils.updateMeta(loadedSaveLocal);
                }
                catch(Exception e)
                {
                    Log.Message("[RimSaves Error] : " + e.Message);
                }
            }

            bool anyKeyDown = Input.anyKeyDown;
            bool flag = anyKeyDown;
            if (flag)
            {
                this.kpQS = Utils.HomeIsHeld;
                this.kpQL = Utils.EndIsHeld;
            }
            else
            {
                bool flag2 = this.kpQS;
                bool flag3 = flag2;
                if (flag3 && (Settings.keyBinding == 2 || Utils.ControlIsHeld))
                {
                    this.quicksave();
                }
                bool flag4 = this.kpQL;
                bool flag5 = flag4;
                if (flag5 && (Settings.keyBinding == 2 || Utils.ControlIsHeld))
                {
                    if (Current.Game.Info.permadeathMode)
                    {
                        return;
                    }
                    this.quickload();
                }
                this.kpQS = false;
                this.kpQL = false;
            }
        }

        public void quicksave(string baseName = "Quicksave")
        {
            if (Current.Game.Info.permadeathMode)
            {
                string mapName = Current.Game.Info.permadeathModeUniqueName;
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    GameDataSaveLoader.SaveGame(mapName);
                }, "SavingLongEvent", false, null);

                Messages.Message("SavedAs".Translate(Utils.getFilenameWithoutVF(mapName)), MessageTypeDefOf.SilentInput);
                PlayerKnowledgeDatabase.Save();
            }
            else
            {
                string mapName = baseName;
                if (Settings.uniqueQuicksaveName)
                {
                    mapName += Utils.getUniqueSuffix();
                }

                LongEventHandler.QueueLongEvent(delegate ()
                {
                    GameDataSaveLoader.SaveGame(mapName);
                }, "SavingLongEvent", false, null);
                Messages.Message("SavedAs".Translate(mapName), MessageTypeDefOf.SilentInput);
                PlayerKnowledgeDatabase.Save();
            }
        }

        private void quickload()
        {
            //Check existence quicksave file
            string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves");
            string prefix = "";
            if (Settings.curFolder != "Default")
                prefix = Settings.curFolder + Utils.VFOLDERSEP;

            path = Path.Combine(path, prefix + "Quicksave.rws");

            if (!File.Exists(path))
            {
                Messages.Message("ARS_NoSQToLoad".Translate(), MessageTypeDefOf.NegativeEvent);
                return;
            }
            Action preLoadLevelAction = delegate
            {
                string save = "Quicksave";


                //prefixage le cas echeant
                if (Settings.curFolder != "Default")
                    save = Settings.curFolder + Utils.VFOLDERSEP + save;

                MemoryUtility.ClearAllMapsAndWorld();
                Current.Game = new Game();
                Current.Game.InitData = new GameInitData();
                Current.Game.InitData.gameToLoad = save;

            };
            LongEventHandler.QueueLongEvent(preLoadLevelAction, "Play", "LoadingLongEvent", true, null);
        }

        private bool kpQS;
        private bool kpQL;

        private Game game;
    }
}