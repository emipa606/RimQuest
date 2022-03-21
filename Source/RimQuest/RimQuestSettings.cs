using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimQuest;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class RimQuestSettings : ModSettings
{
    public Dictionary<IncidentDef, bool> IncidentSettings = new Dictionary<IncidentDef, bool>();
    private List<IncidentDef> incidentSettingsKeys;
    private List<bool> incidentSettingsValues;
    public Dictionary<QuestScriptDef, bool> QuestSettings = new Dictionary<QuestScriptDef, bool>();
    private List<QuestScriptDef> questSettingsKeys;
    private List<bool> questSettingsValues;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref QuestSettings, "QuestSettings", LookMode.Def,
            LookMode.Value,
            ref questSettingsKeys, ref questSettingsValues);
        Scribe_Collections.Look(ref IncidentSettings, "IncidentSettings", LookMode.Def,
            LookMode.Value,
            ref incidentSettingsKeys, ref incidentSettingsValues);
    }

    public void ResetManualValues()
    {
        questSettingsKeys = new List<QuestScriptDef>();
        questSettingsValues = new List<bool>();
        QuestSettings = new Dictionary<QuestScriptDef, bool>();
        incidentSettingsKeys = new List<IncidentDef>();
        incidentSettingsValues = new List<bool>();
        IncidentSettings = new Dictionary<IncidentDef, bool>();
        Main.UpdateValidQuests();
    }
}