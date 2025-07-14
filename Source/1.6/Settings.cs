using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;

namespace aRandomKiwi.ARS
{
    public class Settings : ModSettings
    {
        public static List<string> folders = new List<string>() {"Default"}; 
        public static string curFolder = "Default";
        public static int nbAutosave = 5;
        public static int keyBinding = 1;
        public static bool disableAutosave = false;
        public static bool uniqueQuicksaveName = false;
        public static bool uniqueSaveName = false;
        public static bool saveOnNegativeIncident = false;
        public static bool saveOnPositiveIncident = false;
        public static bool addEventLabelSuffix = true;
        public static bool disableQuicksavesNotifs = false;
        public static bool enableQuicksavesRotations = true;
        public static int maxQuicksaves = 3;
        public static int nextQuicksaves = 1;
        public static int nbMinSecBetweenIncidents = 5;
        public static long nbMinSecBetweenIncidentsTsNegative = 0;
        public static long nbMinSecBetweenIncidentsTsPositive = 0;
        public static bool enableLiteMode = false;


        public static bool SectionGeneralExpanded = false;
        public static bool SectionIncidentsExpanded = false;
        public static bool SectionQSKeysBindingExpanded = false;

        public static Vector2 scrollPosition = Vector2.zero;


        public static void DoSettingsWindowContents(Rect inRect)
        {
            inRect.yMin += 15f;
            inRect.yMax -= 15f;

            var defaultColumnWidth = (inRect.width - 50);
            Listing_Standard list = new Listing_Standard() { };

            Widgets.ButtonImage(new Rect((inRect.width / 2) - 90, inRect.y, 180, 144), Tex.texSettings, Color.white, Color.green);

            var outRect = new Rect(inRect.x, inRect.y + 150, inRect.width, inRect.height - 150);
            var scrollRect = new Rect(0f, 150f, inRect.width - 16f, inRect.height * 3f + 50);
            Widgets.BeginScrollView(outRect, ref scrollPosition, scrollRect, true);

            list.Begin(scrollRect);
            //list.ButtonImage(Tex.texLogoVer, 750, 128);

            list.Gap(10);
            if (SectionGeneralExpanded)
                GUI.color = Color.gray;
            else
                GUI.color = Color.green;

            if (list.ButtonText("ARS_SettingsGeneralSection".Translate()))
            {
                SectionGeneralExpanded = !SectionGeneralExpanded;
            }
            GUI.color = Color.white;

            if (SectionGeneralExpanded)
            {
                list.CheckboxLabeled("ARS_SettingsLiteMode".Translate(), ref enableLiteMode);
                list.CheckboxLabeled("ARS_SettingsUniqueQuicksaveName".Translate(), ref uniqueQuicksaveName);
                list.CheckboxLabeled("ARS_SettingsUniqueSavenameOnSave".Translate(), ref uniqueSaveName);

                list.CheckboxLabeled("ARS_SettingsDisableAutosaves".Translate(), ref disableAutosave);
                if (keyBinding != 3)
                    list.CheckboxLabeled("ARS_SettingsDisableQuicksavesNotifs".Translate(), ref disableQuicksavesNotifs);

                list.CheckboxLabeled("ARS_SettingsQuicksaveRotation".Translate(), ref enableQuicksavesRotations);
                if (enableQuicksavesRotations)
                {
                    int prevMaxQuicksaves = maxQuicksaves;
                    list.Label("ARS_SettingsNbQuicksaves".Translate(Settings.maxQuicksaves));
                    maxQuicksaves = (int)list.Slider(maxQuicksaves, 2, 100);
                    if (prevMaxQuicksaves != maxQuicksaves && maxQuicksaves < nextQuicksaves)
                        nextQuicksaves = 1;
                }

                list.Label("ARS_SettingsNbAutosave".Translate(Settings.nbAutosave));
                nbAutosave = (int)list.Slider(nbAutosave, 2, 150);
            }
            //QuickSaves on incidents
            if (SectionIncidentsExpanded)
                GUI.color = Color.gray;
            else
                GUI.color = Color.green;
            if (list.ButtonText("ARS_SettingsIncidentsSection".Translate()))
            {
                SectionIncidentsExpanded = !SectionIncidentsExpanded;
            }
            GUI.color = Color.white;

            if (SectionIncidentsExpanded)
            {
                list.CheckboxLabeled("ARS_SettingsQuicksaveOnNegativeIncident".Translate(), ref saveOnNegativeIncident);
                list.CheckboxLabeled("ARS_SettingsQuicksaveOnPositiveIncident".Translate(), ref saveOnPositiveIncident);
                if (saveOnNegativeIncident || saveOnPositiveIncident)
                {
                    list.Label("ARS_SettingsSecsMinBetweenIncidents".Translate(Settings.nbMinSecBetweenIncidents));
                    nbMinSecBetweenIncidents = (int)list.Slider(nbMinSecBetweenIncidents, 1, 200);
                }
                list.CheckboxLabeled("ARS_SettingsQuicksaveOnIncidentLabelSuffix".Translate(), ref addEventLabelSuffix);
            }

            //Quicksaves key binding
            if (SectionQSKeysBindingExpanded)
                GUI.color = Color.gray;
            else
                GUI.color = Color.green;
            if (list.ButtonText("ARS_SettingsQSSection".Translate()))
            {
                SectionQSKeysBindingExpanded = !SectionQSKeysBindingExpanded;
            }
            GUI.color = Color.white;

            if (SectionQSKeysBindingExpanded)
            {
                if (list.RadioButton("ARS_SettingsBindingHomeEnd".Translate(), (keyBinding == 1)))
                    keyBinding = 1;
                if (list.RadioButton("ARS_SettingsBindingF5F9".Translate(), (keyBinding == 2)))
                    keyBinding = 2;
                if (list.RadioButton("ARS_SettingsBindingNo".Translate(), (keyBinding == 3)))
                    keyBinding = 3;
            }
            list.End();
            Widgets.EndScrollView();
        }

        public override void ExposeData()
        {
            base.ExposeData();                             

            Scribe_Values.Look<string>(ref curFolder, "curFolder", "Default");
            Scribe_Collections.Look<string>(ref folders, "folders", LookMode.Value);
            Scribe_Values.Look<int>(ref nbAutosave, "nbAutosave", 5);
            Scribe_Values.Look<int>(ref keyBinding, "keyBinding", 1);
            Scribe_Values.Look<bool>(ref disableAutosave, "disableAutosave", false);
            Scribe_Values.Look<bool>(ref uniqueQuicksaveName, "uniqueQuicksaveName", false);
            Scribe_Values.Look<bool>(ref uniqueSaveName, "uniqueSaveName", false);
            Scribe_Values.Look<bool>(ref saveOnNegativeIncident, "saveOnNegativeIncident", false);
            Scribe_Values.Look<bool>(ref saveOnPositiveIncident, "saveOnPositiveIncident", false);
            Scribe_Values.Look<bool>(ref addEventLabelSuffix, "addEventLabelSuffix", true);
            Scribe_Values.Look<bool>(ref disableQuicksavesNotifs, "disableQuicksavesNotifs", false);
            Scribe_Values.Look<bool>(ref enableQuicksavesRotations, "enableQuicksavesRotations", true);
            Scribe_Values.Look<int>(ref maxQuicksaves, "maxQuicksaves", 3);
            Scribe_Values.Look<int>(ref nextQuicksaves, "nextQuicksaves", 1);
            Scribe_Values.Look<int>(ref nbMinSecBetweenIncidents, "nbMinSecBetweenIncidents", 5);
            Scribe_Values.Look<long>(ref nbMinSecBetweenIncidentsTsNegative, "nbMinSecBetweenIncidentsTsNegative", 0);
            Scribe_Values.Look<long>(ref nbMinSecBetweenIncidentsTsPositive, "nbMinSecBetweenIncidentsTsPositive", 0);
            Scribe_Values.Look<bool>(ref enableLiteMode, "enableLiteMode", false);
        } 
    }
}