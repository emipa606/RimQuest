using System.Reflection;
using HarmonyLib;
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