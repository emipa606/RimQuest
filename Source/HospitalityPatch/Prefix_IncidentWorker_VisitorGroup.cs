using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hospitality;
using RimQuest;
using Verse;

namespace HospitalityPatch;

[HarmonyPatch(typeof(IncidentWorker_VisitorGroup), "GiveItems")]
public class Prefix_IncidentWorker_VisitorGroup
{
    public static void Prefix(ref IEnumerable<Pawn> visitors)
    {
        if (visitors == null || !visitors.Any())
        {
            return;
        }

        var value = true;
        HarmonyPatches.AddQuestGiver(visitors.ToList(), ref value);
    }
}