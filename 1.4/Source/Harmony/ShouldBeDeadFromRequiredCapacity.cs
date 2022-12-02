using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Verse;
using HarmonyLib;

namespace DeathRattle.Harmony
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDeadFromRequiredCapacity")]
    public static class ShouldBeDeadFromRequiredCapacityPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DeathRattleException(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            bool trigger = false;
            foreach (CodeInstruction itr in instrs)
            {
                yield return itr;
                if (trigger)
                {
                    trigger = false;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ShouldBeDeadFromRequiredCapacityPatch), "AddCustomHediffs", new Type[] { typeof(Pawn_HealthTracker), typeof(Pawn), typeof(PawnCapacityDef) }));
                    yield return itr;
                }
                if (itr.opcode == OpCodes.Callvirt && itr.operand.Equals(AccessTools.Method(typeof(PawnCapacitiesHandler), "CapableOf", new Type[] { typeof(PawnCapacityDef) })))
                {
                    trigger = true;
                }
            }
        }

        public static bool AddCustomHediffs(Pawn_HealthTracker tracker, Pawn pawn, PawnCapacityDef pawnCapacityDef)
        {
            if (pawn.health.hediffSet.GetBrain() == null)
            {
                return false;
            }
            if (pawn.RaceProps.IsFlesh && pawnCapacityDef.lethalFlesh && !tracker.capacities.CapableOf(pawnCapacityDef) && ailmentDictionary.ContainsKey(pawnCapacityDef.defName))
            {
                HediffDef def = ailmentDictionary[pawnCapacityDef.defName];
                if (def == null)
                {
                    if ((pawn.health.hediffSet.GetNotMissingParts(depth: BodyPartDepth.Inside).FirstOrDefault(p => p.def.defName == "Liver") == null) && !pawn.health.hediffSet.HasHediff(HediffDefOfDeathRattle.LiverFailure))
                    {
                        def = HediffDefOfDeathRattle.LiverFailure;
                    }
                    else if (pawn.health.hediffSet.GetNotMissingParts(depth: BodyPartDepth.Inside).FirstOrDefault(p => p.def.defName.Contains("Kidney")) == null)
                    {
                        def = HediffDefOfDeathRattle.KidneyFailure;
                    }
                }
                if (def != null && !pawn.health.hediffSet.HasHediff(def))
                {
                    Hediff_DeathRattle ailment = HediffMaker.MakeHediff(def, pawn) as Hediff_DeathRattle;
                    ailment.cause = pawnCapacityDef;
                    pawn.health.AddHediff(ailment);
                }
                return true;
            }
            return false;
        }

        public static Dictionary<string, HediffDef> ailmentDictionary = new Dictionary<string, HediffDef>{ { "Metabolism", HediffDefOfDeathRattle.IntestinalFailure },
                                                                                                           { "BloodFiltration", null },
                                                                                                           { "BloodPumping", HediffDefOfDeathRattle.ClinicalDeathNoHeartbeat },
                                                                                                           { "Breathing", HediffDefOfDeathRattle.ClinicalDeathAsphyxiation },
                                                                                                           { "Consciousness", HediffDefOfDeathRattle.Coma }};
    }
}