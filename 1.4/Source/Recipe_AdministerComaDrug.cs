using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;
using Verse;

namespace DeathRattle
{
    public class Recipe_AdministerComaDrug : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(
              Pawn pawn,
              RecipeDef recipe) => MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, (Func<BodyPartRecord, bool>)(record => pawn.health.hediffSet.GetNotMissingParts().Contains<BodyPartRecord>(record) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) && !pawn.health.hediffSet.hediffs.Any<Hediff>((Predicate<Hediff>)(x =>
              {
                  if (x.Part != record)
                      return false;
                  return x.def == recipe.addsHediff;
              }))));
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!pawn.RaceProps.IsFlesh)
            {
                return;
            }
            pawn.health.forceDowned = true;

            Hediff_Comatose ailment = HediffMaker.MakeHediff(HediffDefOfComatose.ArtificialComa, pawn) as Hediff_Comatose;
            pawn.health.AddHediff(ailment);

            pawn.health.forceDowned = false;
        }
        /*
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map) {
        }
*/
        public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
        {
            if (pawn.Faction == billDoerFaction)
            {
                return false;
            }
            return true;
        }

    }
}