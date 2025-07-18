using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace aRandomKiwi.ARS
{
    public class Dialog_Input : Window
    {
        protected string curName;

        private bool focusedFolderNameField;
        private Action<string> onOkCb;
        private Func<string, bool> validationCb;

        protected virtual int MaxNameLength
        {
            get
            {
                return Settings.maxSaveCharLength;
            }
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(280f, 175f);
            }
        }

        public Dialog_Input(Action<string> eventOnOk, Func<string, bool> valCb,string defVal="")
        {
            this.forcePause = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
            this.closeOnClickedOutside = true;

            validationCb = valCb;
            onOkCb = eventOnOk;
            curName = defVal;
        }

        protected virtual AcceptanceReport NameIsValid(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }
            return true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                flag = true;
                Event.current.Use();
            }
            GUI.SetNextControlName("FolderNameField");
            string text = Widgets.TextField(new Rect(0f, 15f, inRect.width, 35f), this.curName);
            if (text.Length < this.MaxNameLength)
            {
                this.curName = text;
            }
            if (!this.focusedFolderNameField)
            {
                UI.FocusControl("FolderNameField", this);
                this.focusedFolderNameField = true;
            }
            if (Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 15f, inRect.width - 15f - 15f, 35f), "OK", true, false, true) || flag)
            {
                AcceptanceReport acceptanceReport = this.NameIsValid(this.curName);
                if (!acceptanceReport.Accepted || !Utils.isValidFilename(this.curName))
                {
                    if (acceptanceReport.Reason.NullOrEmpty())
                    {
                        Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                    }
                    else
                    {
                        Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
                    }
                }
                else
                {
                    if (validationCb(this.curName))
                    {
                        if (this.onOkCb != null)
                            this.onOkCb(this.curName);

                        Find.WindowStack.TryRemove(this, true);
                    }
                }
            }
        }
    }
}