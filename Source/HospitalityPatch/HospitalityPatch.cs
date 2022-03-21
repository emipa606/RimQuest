using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hospitality;
using RimQuest;
using Verse;

namespace HospitalityPatch;

[StaticConstructorOnStartup]
internal static class HospitalityPatch
{
    static HospitalityPatch()
    {
        var harmony = new Harmony("Mlie.RimQuest.HospitalityPatch");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [HarmonyPatch(typeof(IncidentWorker_VisitorGroup))]
    [HarmonyPatch("GiveItems")]
    public class Prefix_IncidentWorker_VisitorGroup
    {
        [HarmonyPrefix]
        public static void Prefix(ref IEnumerable<Pawn> visitors)
        {
            if (visitors == null || !visitors.Any())
            {
                return;
            }

            var value = true;
            HarmonyPatches.AddQuestGiver(visitors.ToList(), null, null, ref value);
        }
    }
}