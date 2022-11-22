using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;
using UnityEngine;

namespace More_Darklight_Colours
{
    [StaticConstructorOnStartup]
    public static class More_Darklight_Colours
    {
        static More_Darklight_Colours()
        {
            Log.Message("[More Darklight Colours] Loaded");

            Harmony harmony = new Harmony("rimworld.mod.endoxis.moredarklightcolours");
            harmony.PatchAll();
        }
    }

    public class MDCSettings : ModSettings
    {
        public static float saturationBreakpoint = 50f;
        public static float antiPrimary = 0f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref saturationBreakpoint, "More_Darklight_Colours_saturationBreakpoint", 50f);
            Scribe_Values.Look(ref antiPrimary, "More_Darklight_Colours_antiPrimary", 0f);
            base.ExposeData();
        }
    }

    public class More_Darklight_ColoursConfig : Mod
    {
        public More_Darklight_ColoursConfig(ModContentPack content) : base(content)
        {
            Log.Message("[More Darklight Colours] Inherited Mod Class loaded");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label(label: "Saturation Breakpoint: " + MDCSettings.saturationBreakpoint.ToString(), tooltip: "Colours with a saturation value lower than this will not be a darklight\n\nDefault: 50");
            MDCSettings.saturationBreakpoint = listingStandard.Slider(MDCSettings.saturationBreakpoint, 0f, 100f);
            listingStandard.Label(label: "Anti-Primaryness: " + MDCSettings.antiPrimary.ToString(), tooltip: "0 = All hues can be darklights\n0.5 = 50% of hues can be darklights\n0.99 = Only pure C/Y/M hues can be darklights\n1 = Nothing is a Darklight\n\nDefault: 0");
            MDCSettings.antiPrimary = listingStandard.Slider(MDCSettings.antiPrimary, 0f, 1f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "More Darklight Colours";
        }
    }

    [HarmonyPatch]
    public static class IsDarklightPatch
    {
        [HarmonyPatch(typeof(DarklightUtility), nameof(DarklightUtility.IsDarklight))]
        [HarmonyPrefix]
        static bool Prefix(ref Color color, ref bool __result)
        {
            float[] c = { color.r, color.g, color.b };
            Array.Sort(c);

            bool hueCheck;
            bool saturationCheck;

            hueCheck = (c[2] != 0f) 
                && ((c[1] / c[2]) >= MDCSettings.antiPrimary);

            saturationCheck = (MDCSettings.saturationBreakpoint != 100) 
                && (c[0] <= (c[2] / (-(100 / 
                (MDCSettings.saturationBreakpoint - 100)))));

            __result = hueCheck && saturationCheck;

            return false;
        }


    }
}