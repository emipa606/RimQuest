using System;
using System.Collections.Generic;
using System.Linq;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimQuest;

[StaticConstructorOnStartup]
internal class RimQuestMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static RimQuestMod instance;

    private static readonly Vector2 buttonSize = new Vector2(120f, 25f);
    private static string currentVersion;

    private static string searchText = "";
    private static readonly Vector2 searchSize = new Vector2(200f, 25f);
    private static readonly Color alternateBackground = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    private static Vector2 scrollPosition;

    /// <summary>
    ///     The private settings
    /// </summary>
    private RimQuestSettings settings;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public RimQuestMod(ModContentPack content) : base(content)
    {
        instance = this;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(
                ModLister.GetActiveModWithIdentifier("Mlie.RimQuest"));
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal RimQuestSettings Settings
    {
        get
        {
            if (settings == null)
            {
                settings = GetSettings<RimQuestSettings>();
            }

            return settings;
        }
        set => settings = value;
    }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "RimQuest";
    }

    private static void DrawButton(Action action, string text, Vector2 pos)
    {
        var rect = new Rect(pos.x, pos.y, buttonSize.x, buttonSize.y);
        if (!Widgets.ButtonText(rect, text, true, false, Color.white))
        {
            return;
        }

        SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
        action();
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        listing_Standard.Label("RimQuest_price".Translate(instance.Settings.QuestPrice.ToStringMoney()), -1f,
            "RimQuest_price_tooltip".Translate());
        instance.Settings.QuestPrice =
            (float)Math.Round(listing_Standard.Slider(instance.Settings.QuestPrice, 1f, 250f), 0);

        var headerLabel = listing_Standard.Label("RimQuest_Hospitality_ValidQuests".Translate());

        if (instance.Settings.IncidentSettings == null)
        {
            instance.Settings.IncidentSettings = new Dictionary<IncidentDef, bool>();
        }

        if (instance.Settings.QuestSettings == null)
        {
            instance.Settings.QuestSettings = new Dictionary<QuestScriptDef, bool>();
        }

        if (instance.Settings.IncidentSettings.Any() || instance.Settings.QuestSettings.Any())
        {
            DrawButton(() =>
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "RimQuest_reset_confirm".Translate(),
                        delegate { instance.Settings.ResetManualValues(); }));
                }, "RimQuest_reset_button".Translate(),
                new Vector2(headerLabel.position.x + headerLabel.width - buttonSize.x,
                    headerLabel.position.y));
        }

        Text.Font = GameFont.Small;

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("RimQuest_version_label".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        searchText =
            Widgets.TextField(
                new Rect(headerLabel.position + new Vector2((rect.width / 2) - (searchSize.x / 2), 0),
                    searchSize),
                searchText);
        TooltipHandler.TipRegion(new Rect(
            headerLabel.position + new Vector2((rect.width / 2) - (searchSize.x / 2), 0),
            searchSize), "RimQuest_search".Translate());

        listing_Standard.End();


        var allQuests = Main.Quests;
        var allIncidents = Main.Incidents;
        if (!string.IsNullOrEmpty(searchText))
        {
            allQuests = Main.Quests.Where(keyValuePair =>
                keyValuePair.Key.defName.ToLower().Contains(searchText.ToLower()) || keyValuePair.Key.modContentPack
                    .Name
                    .ToLower()
                    .Contains(searchText.ToLower())).ToDictionary(pair => pair.Key, pair => pair.Value);
            allIncidents = Main.Incidents.Where(keyValuePair =>
                keyValuePair.Key.label.ToLower().Contains(searchText.ToLower()) || keyValuePair.Key.modContentPack.Name
                    .ToLower()
                    .Contains(searchText.ToLower())).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        var borderRect = rect;
        borderRect.y += headerLabel.y + 90;
        borderRect.height -= headerLabel.y + 90;
        var scrollContentRect = rect;
        scrollContentRect.height = (allQuests.Count + allIncidents.Count) * 61f;
        scrollContentRect.width -= 20;
        scrollContentRect.x = 0;
        scrollContentRect.y = 0;


        var scrollListing = new Listing_Standard();
        Widgets.BeginScrollView(borderRect, ref scrollPosition, scrollContentRect);
        scrollListing.Begin(scrollContentRect);
        var alternate = false;
        foreach (var questScriptDef in allQuests.Keys)
        {
            var modInfo = questScriptDef.modContentPack?.Name;
            var selectorRect = scrollListing.GetRect(60);
            alternate = !alternate;
            if (alternate)
            {
                Widgets.DrawBoxSolid(selectorRect.ExpandedBy(10, 0), alternateBackground);
            }

            var questLabel = $"{Main.GetQuestReadableName(questScriptDef)} ({questScriptDef.defName}) - {modInfo}";
            var selectedValue = Main.VanillaQuestsValues[questScriptDef];
            if (instance.Settings.QuestSettings.ContainsKey(questScriptDef))
            {
                if (instance.Settings.QuestSettings[questScriptDef] == selectedValue)
                {
                    instance.Settings.QuestSettings.Remove(questScriptDef);
                }
                else
                {
                    selectedValue = instance.Settings.QuestSettings[questScriptDef];
                }
            }

            var wasValue = selectedValue;


            Widgets.CheckboxLabeled(selectorRect, questLabel, ref selectedValue);

            if (wasValue != selectedValue)
            {
                instance.Settings.QuestSettings[questScriptDef] = selectedValue;
            }
        }

        foreach (var incidentDef in allIncidents.Keys)
        {
            var modInfo = incidentDef.modContentPack?.Name;
            var selectorRect = scrollListing.GetRect(60);
            alternate = !alternate;
            if (alternate)
            {
                Widgets.DrawBoxSolid(selectorRect.ExpandedBy(10, 0), alternateBackground);
            }

            var incidentLabel = $"{incidentDef.LabelCap} ({incidentDef.defName}) - {modInfo}";
            var selectedValue = Main.VanillaIncidentsValues[incidentDef];
            if (instance.Settings.IncidentSettings.ContainsKey(incidentDef))
            {
                if (instance.Settings.IncidentSettings[incidentDef] == selectedValue)
                {
                    instance.Settings.IncidentSettings.Remove(incidentDef);
                }
                else
                {
                    selectedValue = instance.Settings.IncidentSettings[incidentDef];
                }
            }

            var wasValue = selectedValue;


            Widgets.CheckboxLabeled(selectorRect, incidentLabel, ref selectedValue);

            if (wasValue != selectedValue)
            {
                instance.Settings.IncidentSettings[incidentDef] = selectedValue;
            }
        }

        scrollListing.End();
        Widgets.EndScrollView();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        Main.UpdateValidQuests();
    }
}