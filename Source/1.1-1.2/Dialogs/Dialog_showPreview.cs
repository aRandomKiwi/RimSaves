using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace aRandomKiwi.ARS
{
    public class Dialog_ShowPreview : Window
    {
        Texture2D tex;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(UI.screenWidth, UI.screenHeight);
            }
        }

        public Dialog_ShowPreview(Texture2D tex)
        {
            this.forcePause = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
            this.closeOnClickedOutside = true;

            this.tex = tex;

        }

        public override void DoWindowContents(Rect inRect)
        {
            if(Widgets.ButtonImage(new Rect(0, 0, UI.screenWidth, UI.screenHeight), tex, Color.white, Color.white))
            {
                Find.WindowStack.TryRemove(this);
            }
        }
    }
}