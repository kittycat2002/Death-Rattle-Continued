using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DeathRattle
{
    public class DeathRattleMod : Mod
    {
        public DeathRattleMod(ModContentPack content) : base(content)
        {
            GetSettings<DeathRattleSettings>();
        }
        public override string SettingsCategory()
        {
            return "Death Rattle";
        }
        public override void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("Colony Only", ref DeathRattleSettings.ColonyOnly);
            listingStandard.CheckboxLabeled("Affects Animals", ref DeathRattleSettings.AffectsAnimals);
            listingStandard.CheckboxLabeled("Affects Shamblers", ref DeathRattleSettings.AffectsShamblers);
            listingStandard.End();
            base.DoSettingsWindowContents(rect);
        }
    }
    class DeathRattleSettings : ModSettings
    {
        public static bool ColonyOnly = false;
        public static bool AffectsAnimals = true;
        public static bool AffectsShamblers = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ColonyOnly, "ColonyOnly", false, false);
            Scribe_Values.Look(ref AffectsAnimals, "AffectsAnimals", true, false);
            Scribe_Values.Look(ref AffectsShamblers, "AffectsShamblers", true, false);
        }
    }
}
