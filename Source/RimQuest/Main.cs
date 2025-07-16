using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimQuest;

[StaticConstructorOnStartup]
public static class Main
{
    public static Dictionary<IncidentDef, bool> Incidents;
    public static Dictionary<IncidentDef, bool> VanillaIncidentsValues;
    public static Dictionary<QuestScriptDef, bool> Quests;
    public static Dictionary<QuestScriptDef, bool> VanillaQuestsValues;

    public static readonly Material exclamationPointMat =
        MaterialPool.MatFrom("UI/Overlays/RQ_ExclamationPoint", ShaderDatabase.MetaOverlay);

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

            Incidents[def] = isAcceptableIncident(def);
            if (saveVanilla)
            {
                VanillaIncidentsValues[def] = isAcceptableIncident(def, true);
            }
        }

        foreach (var def in DefDatabase<QuestScriptDef>.AllDefsListForReading.OrderBy(GetQuestReadableName).ToList())
        {
            if (!def.defName.Contains("OpportunitySite_") &&
                (!def.defName.Contains("Hospitality_") || def.defName.Contains("Util_")))
            {
                continue;
            }

            Quests[def] = isAcceptableQuest(def);
            if (saveVanilla)
            {
                VanillaQuestsValues[def] = isAcceptableQuest(def, true);
            }
        }
    }

    private static bool isAcceptableQuest(QuestScriptDef questScriptDef, bool vanillaCheck = false)
    {
        if (!vanillaCheck && RimQuestMod.instance.Settings.questSettings.TryGetValue(questScriptDef, out var quest))
        {
            return quest;
        }

        return questScriptDef.GetModExtension<RimQuest_ModExtension>()?.canBeARimQuest ??
               true; //mod extension value if not null, otherwise assumed true.
    }

    private static bool isAcceptableIncident(IncidentDef incidentDef, bool vanillaCheck = false)
    {
        if (!vanillaCheck && RimQuestMod.instance.Settings.incidentSettings.TryGetValue(incidentDef, out var incident))
        {
            return incident;
        }

        return incidentDef.GetModExtension<RimQuest_ModExtension>()?.canBeARimQuest ??
               true; //mod extension value if not null, otherwise assumed true.
    }

    public static string GetQuestReadableName(QuestScriptDef questScriptDef)
    {
        var defName = questScriptDef.defName;
        var keyedName = $"RimQuest_{defName}";
        if ((Prefs.DevMode || keyedName.Translate() != keyedName) &&
            (!Prefs.DevMode || !keyedName.Translate().ToString().Contains("RìṁQùèșṭ_")))
        {
            return keyedName.Translate();
        }

        if (defName.Contains("_"))
        {
            defName = questScriptDef.defName.Split('_')[1];
        }

        if (questScriptDef.defName.Contains("Hospitality"))
        {
            defName = questScriptDef.defName.Replace("_", " ");
        }

        return Regex.Replace(defName, "(\\B[A-Z])", " $1");
    }
}