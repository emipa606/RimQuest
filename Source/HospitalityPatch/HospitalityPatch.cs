using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace HospitalityPatch
{

    [StaticConstructorOnStartup]
    static class HospitalityPatch
    {
        static HospitalityPatch()
        {
            var harmony = new Harmony("Mlie.RimQuest.HospitalityPatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(Hospitality.IncidentWorker_VisitorGroup))]
        [HarmonyPatch("GiveItems")]
        public class Prefix_IncidentWorker_VisitorGroup
        {
            [HarmonyPrefix]
            public static void Prefix(ref IEnumerable<Pawn> visitors)
            {
                if(visitors == null || visitors.Count() == 0)
                {
                    return;
                }
                var value = true;
                RimQuest.HarmonyPatches.AddQuestGiver(visitors.ToList(), null, null, ref value);
            }
        }
    }
}
