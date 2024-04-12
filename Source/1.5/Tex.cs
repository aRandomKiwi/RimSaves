using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace aRandomKiwi.ARS
{
    [StaticConstructorOnStartup]
    static class Tex
    {
        public static readonly Texture2D texDeleteFolder = ContentFinder<Texture2D>.Get("UI/DeleteFolder2", true);
        public static readonly Texture2D texDeleteSelected = ContentFinder<Texture2D>.Get("UI/DeleteSelected", true);
        public static readonly Texture2D texDeleteX = ContentFinder<Texture2D>.Get("UI/DeleteX", true);
        public static readonly Texture2D texDeleteX2 = ContentFinder<Texture2D>.Get("UI/DeleteX2", true);
        public static readonly Texture2D texAdd = ContentFinder<Texture2D>.Get("UI/Add", true);
        public static readonly Texture2D texMove = ContentFinder<Texture2D>.Get("UI/Move2", true);
        public static readonly Texture2D texMoveDisabled = ContentFinder<Texture2D>.Get("UI/MoveDisabled", true);
        public static readonly Texture2D texMore = ContentFinder<Texture2D>.Get("UI/More", true);
        public static readonly Texture2D texLogo = ContentFinder<Texture2D>.Get("UI/Logo", true);
        public static readonly Texture2D texEdit = ContentFinder<Texture2D>.Get("UI/Edit2", true);
        public static readonly Texture2D texSearch = ContentFinder<Texture2D>.Get("UI/Search", true);
        public static readonly Texture2D texFolder = ContentFinder<Texture2D>.Get("UI/Folder", true);
        public static readonly Texture2D texFolder2 = ContentFinder<Texture2D>.Get("UI/Folder2", true);
        public static readonly Texture2D texEditDisabled = ContentFinder<Texture2D>.Get("UI/EditDisabled2", true);
        public static readonly Texture2D texBGNoPreview = ContentFinder<Texture2D>.Get("BG/NoPreview", true);
        public static readonly Texture2D texLogoVer = ContentFinder<Texture2D>.Get("UI/LogoVer", true);
        public static readonly Texture2D texSettings = ContentFinder<Texture2D>.Get("UI/RimSavesSettings", true);
        public static readonly Texture2D texSel = ContentFinder<Texture2D>.Get("UI/Sel", true);
        public static readonly Texture2D texUnSel = ContentFinder<Texture2D>.Get("UI/UnSel", true);
    }
}