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
    public float QuestChance = 1f;
    public float QuestPrice = 50f;
    public Dictionary<QuestScriptDef, bool> QuestSettings = new Dictionary<QuestScriptDef, bool>();
    private List<QuestScriptDef> questSettingsKeys;
    private List<bool> questSettingsValues;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref QuestPrice, "QuestPrice", 50f);
        Scribe_Values.Look(ref QuestChance, "QuestChance", 1f);
        Scribe_Collections.Look(ref QuestSettings, "QuestSettings", LookMode.Def,
            LookMode.Value,
            ref questSettingsKeys, ref questSettingsValues);
        Scribe_Collections.Look(ref IncidentSettings, "IncidentSettings", LookMode.Def,
            LookMode.Value,
            ref incidentSettingsKeys, ref incidentSettingsValues);
    }

    public void ResetManualValues()
    {
        questSettingsKeys = [];
        questSettingsValues = [];
        QuestSettings = new Dictionary<QuestScriptDef, bool>();
        incidentSettingsKeys = [];
        incidentSettingsValues = [];
        IncidentSettings = new Dictionary<IncidentDef, bool>();
        Main.UpdateValidQuests();
    }
}