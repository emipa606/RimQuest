using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimQuest;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class RimQuestSettings : ModSettings
{
    public Dictionary<IncidentDef, bool> incidentSettings = new();
    private List<IncidentDef> incidentSettingsKeys;
    private List<bool> incidentSettingsValues;
    public float questChance = 1f;
    public float questPrice = 50f;
    public Dictionary<QuestScriptDef, bool> questSettings = new();
    private List<QuestScriptDef> questSettingsKeys;
    private List<bool> questSettingsValues;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref questPrice, "QuestPrice", 50f);
        Scribe_Values.Look(ref questChance, "QuestChance", 1f);
        Scribe_Collections.Look(ref questSettings, "QuestSettings", LookMode.Def,
            LookMode.Value,
            ref questSettingsKeys, ref questSettingsValues);
        Scribe_Collections.Look(ref incidentSettings, "IncidentSettings", LookMode.Def,
            LookMode.Value,
            ref incidentSettingsKeys, ref incidentSettingsValues);
    }

    public void ResetManualValues()
    {
        questSettingsKeys = [];
        questSettingsValues = [];
        questSettings = new Dictionary<QuestScriptDef, bool>();
        incidentSettingsKeys = [];
        incidentSettingsValues = [];
        incidentSettings = new Dictionary<IncidentDef, bool>();
        Main.UpdateValidQuests();
    }
}