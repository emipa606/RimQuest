using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hospitality;
using RimQuest;
using Verse;

namespace HospitalityPatch;

[HarmonyPatch(typeof(IncidentWorker_VisitorGroup), "GiveItems")]
public class IncidentWorker_VisitorGroup_GiveItems
{
    public static void Prefix(ref IEnumerable<Pawn> visitors)
    {
        var enumerable = visitors as Pawn[] ?? visitors.ToArray();
        if (visitors == null || !enumerable.Any())
        {
            return;
        }

        var value = true;
        IncidentWorker_VisitorGroup_TryConvertOnePawnToSmallTrader.Postfix(enumerable.ToList(), ref value);
    }
}