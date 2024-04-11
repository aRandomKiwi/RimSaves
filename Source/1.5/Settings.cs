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
        public static bool enableQuicksavesRotations = false;
        public static int maxQuicksaves = 3;
        public static int nextQuicksaves = 1;



        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard() { };

            list.Begin(inRect);
            list.ButtonImage(Tex.texLogoVer, 750, 128);
            list.GapLine();
            list.Label("ARS_SettingsGeneralSection".Translate());
            list.GapLine();

            list.CheckboxLabeled("ARS_SettingsQuicksaveOnNegativeIncident".Translate(), ref saveOnNegativeIncident);
            list.CheckboxLabeled("ARS_SettingsQuicksaveOnPositiveIncident".Translate(), ref saveOnPositiveIncident);
            list.CheckboxLabeled("ARS_SettingsQuicksaveOnIncidentLabelSuffix".Translate(), ref addEventLabelSuffix);
            list.CheckboxLabeled("ARS_SettingsUniqueQuicksaveName".Translate(), ref uniqueQuicksaveName);
            list.CheckboxLabeled("ARS_SettingsUniqueSavenameOnSave".Translate(), ref uniqueSaveName);

            list.CheckboxLabeled("ARS_SettingsDisableAutosaves".Translate(), ref disableAutosave);
            if (keyBinding != 3)
                list.CheckboxLabeled("ARS_SettingsDisableQuicksavesNotifs".Translate(), ref disableQuicksavesNotifs);

            list.CheckboxLabeled("ARS_SettingsQuicksaveRotation".Translate(), ref enableQuicksavesRotations);
            if(enableQuicksavesRotations)
                uniqueQuicksaveName = false;
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

            list.GapLine();
            list.Label("ARS_SettingsQSSection".Translate());
            list.GapLine();

            if (list.RadioButton("ARS_SettingsBindingHomeEnd".Translate(), (keyBinding == 1)))
                keyBinding = 1;
            if (list.RadioButton("ARS_SettingsBindingF5F9".Translate(), (keyBinding == 2)))
                keyBinding = 2;
            if (list.RadioButton("ARS_SettingsBindingNo".Translate(), (keyBinding == 3)))
                keyBinding = 3;

            list.End();
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
            Scribe_Values.Look<bool>(ref enableQuicksavesRotations, "enableQuicksavesRotations", false);
            Scribe_Values.Look<int>(ref maxQuicksaves, "maxQuicksaves", 3);
            Scribe_Values.Look<int>(ref nextQuicksaves, "nextQuicksaves", 1);
        } 
    }
}