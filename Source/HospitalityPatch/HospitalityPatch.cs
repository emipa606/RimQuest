using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimQuest;
using RimWorld;
using Verse;

namespace HospitalityPatch;

[StaticConstructorOnStartup]
internal static class HospitalityPatch
{
    static HospitalityPatch()
    {
        new Harmony("Mlie.RimQuest.HospitalityPatch").PatchAll(Assembly.GetExecutingAssembly());
    }
}

[HarmonyPatch(typeof(IncidentWorker_VisitorGroup), "GiveItems")]
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