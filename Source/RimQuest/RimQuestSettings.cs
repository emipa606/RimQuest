using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimQuest;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class RimQuestSettings : ModSettings
{
    public int amount = 3;
    public Dictionary<IncidentDef, bool> incidentSettings = new Dictionary<IncidentDef, bool>();
    private List<IncidentDef> incidentSettingsKeys;
    private List<bool> incidentSettingsValues;
    public float questChance = 1f;
    public float questPrice = 50f;
    public Dictionary<QuestScriptDef, bool> questSettings = new Dictionary<QuestScriptDef, bool>();
    private List<QuestScriptDef> questSettingsKeys;
    private List<bool> questSettingsValues;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref questPrice, "QuestPrice", 50f);
        Scribe_Values.Look(ref questChance, "QuestChance", 1f);
        Scribe_Values.Look(ref amount, "QuestAmount", 3);
        Scribe_Collections.Look(ref questSettings, "QuestSettings", LookMode.Def,
            LookMode.Value,
            ref questSettingsKeys, ref questSettingsValues);
        Scribe_Collections.Look(ref incidentSettings, "IncidentSettings", LookMode.Def,
            LookMode.Value,
            ref incidentSettingsKeys, ref incidentSettingsValues);
    }

    public void ResetManualValues()
    {
        amount = 3;
        questSettingsKeys = [];
        questSettingsValues = [];
        questSettings = new Dictionary<QuestScriptDef, bool>();
        incidentSettingsKeys = [];
        incidentSettingsValues = [];
        incidentSettings = new Dictionary<IncidentDef, bool>();
        Main.UpdateValidQuests();
    }
}