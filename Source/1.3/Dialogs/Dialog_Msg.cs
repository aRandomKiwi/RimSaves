using System;
using UnityEngine;
using Verse;

namespace aRandomKiwi.ARS
{
    public class Dialog_Msg : Dialog_MessageBox
    {
        private const float TitleHeight = 42f;
        private const float DialogWidth = 500f;
        private const float DialogHeight = 300f;
        private Color winTextColor = Color.white;

        public override Vector2 InitialSize
        {
            get
            {
                float height = DialogHeight;
                if (title != null) height += TitleHeight;
                return new Vector2(DialogWidth, height);
            }
        }

        public Dialog_Msg(string title, string text, Action confirmedAct = null, bool destructive = false, Color textColor = default(Color))
            : base(text, "Confirm".Translate(), confirmedAct, "GoBack".Translate(), null, title, destructive)
        {
            if (textColor != default(Color))
                winTextColor = textColor;

            closeOnCancel = false;
            closeOnAccept = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Color prev = GUI.color;
            GUI.color = winTextColor;
            base.DoWindowContents(inRect);
            GUI.color = prev;

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    if (buttonAAction != null) buttonAAction();
                    Close();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    if (buttonBAction != null) buttonBAction();
                    Close();
                }
            }
        }
    }
}