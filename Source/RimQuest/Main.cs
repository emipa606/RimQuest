using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using Verse;

namespace RimQuest;

[StaticConstructorOnStartup]
public static class Main
{
    public static Dictionary<IncidentDef, bool> Incidents;
    public static Dictionary<IncidentDef, bool> VanillaIncidentsValues;
    public static Dictionary<QuestScriptDef, bool> Quests;
    public static Dictionary<QuestScriptDef, bool> VanillaQuestsValues;

    static Main()
    {
        UpdateValidQuests(true);
    }

    public static void UpdateValidQuests(bool saveVanilla = false)
    {
        if (saveVanilla)
        {
            VanillaIncidentsValues = new Dictionary<IncidentDef, bool>();
            VanillaQuestsValues = new Dictionary<QuestScriptDef, bool>();
        }

        Incidents = new Dictionary<IncidentDef, bool>();
        Quests = new Dictionary<QuestScriptDef, bool>();
        foreach (var def in DefDatabase<IncidentDef>.AllDefsListForReading.OrderBy(def => def.label).ToList())
        {
            if (!def.targetTags.Contains(IncidentTargetTagDefOf.World) || def.letterDef == LetterDefOf.NegativeEvent ||
                def.defName is "JourneyOffer" or "CultIncident_StarsAreWrong"
                    or "CultIncident_StarsAreRight" or "HPLovecraft_BloodMoon" or "Aurora" ||
                def.defName.Contains("GiveQuest"))
            {
                continue;
            }

            Incidents[def] = IsAcceptableIncident(def);
            if (saveVanilla)
            {
                VanillaIncidentsValues[def] = IsAcceptableIncident(def, true);
            }
        }

        foreach (var def in DefDatabase<QuestScriptDef>.AllDefsListForReading.OrderBy(GetQuestReadableName).ToList())
        {
            if (!def.defName.Contains("OpportunitySite_") &&
                (!def.defName.Contains("Hospitality_") || def.defName.Contains("Util_")))
            {
                continue;
            }

            Quests[def] = IsAcceptableQuest(def);
            if (saveVanilla)
            {
                VanillaQuestsValues[def] = IsAcceptableQuest(def, true);
            }
        }
    }

    private static bool IsAcceptableQuest(QuestScriptDef questScriptDef, bool vanillaCheck = false)
    {
        if (!vanillaCheck && RimQuestMod.instance.Settings.QuestSettings.ContainsKey(questScriptDef))
        {
            return RimQuestMod.instance.Settings.QuestSettings[questScriptDef];
        }

        return questScriptDef.GetModExtension<RimQuest_ModExtension>()?.canBeARimQuest ??
               true; //mod extension value if not null, otherwise assumed true.
    }

    private static bool IsAcceptableIncident(IncidentDef incidentDef, bool vanillaCheck = false)
    {
        if (!vanillaCheck && RimQuestMod.instance.Settings.IncidentSettings.ContainsKey(incidentDef))
        {
            return RimQuestMod.instance.Settings.IncidentSettings[incidentDef];
        }

        return incidentDef.GetModExtension<RimQuest_ModExtension>()?.canBeARimQuest ??
               true; //mod extension value if not null, otherwise assumed true.
    }

    public static string GetQuestReadableName(QuestScriptDef questScriptDef)
    {
        var defname = questScriptDef.defName;
        var keyedName = "RimQuest_" + defname;
        if ((Prefs.DevMode || keyedName.Translate() != keyedName) &&
            (!Prefs.DevMode || !keyedName.Translate().ToString().Contains("RìṁQùèșṭ_")))
        {
            return keyedName.Translate();
        }

        if (defname.Contains("_"))
        {
            defname = questScriptDef.defName.Split('_')[1];
        }

        if (questScriptDef.defName.Contains("Hospitality"))
        {
            defname = questScriptDef.defName.Replace("_", " ");
        }

        return Regex.Replace(defname, "(\\B[A-Z])", " $1");
    }
}