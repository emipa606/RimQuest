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
    public const float MaxCost = 1000f;

    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static RimQuestMod instance;

    private static readonly Vector2 buttonSize = new(120f, 25f);
    private static string currentVersion;

    private static string searchText = "";
    private static readonly Vector2 searchSize = new(200f, 25f);
    private static readonly Color alternateBackground = new(0.2f, 0.2f, 0.2f, 0.5f);
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
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal RimQuestSettings Settings
    {
        get
        {
            settings ??= GetSettings<RimQuestSettings>();

            return settings;
        }
    }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "RimQuest";
    }

    private static void drawButton(Action action, string text, Vector2 pos)
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
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        listingStandard.Label("RimQuest_price".Translate(instance.Settings.questPrice.ToStringMoney()), -1f,
            "RimQuest_price_tooltip".Translate());
        instance.Settings.questPrice =
            (float)Math.Round(listingStandard.Slider(instance.Settings.questPrice, 1f, MaxCost), 0);
        instance.Settings.questChance = listingStandard.SliderLabeled(
            "RimQuest_chance".Translate(instance.Settings.questChance.ToStringPercent()),
            instance.Settings.questChance, 0.01f, 1f, tooltip: "RimQuest_chance_tooltip".Translate());

        instance.Settings.amount = (int)listingStandard.SliderLabeled(
            "RimQuest_amount".Translate(instance.Settings.amount), instance.Settings.amount, 1, 10,
            tooltip: "RimQuest_amount_tooltip".Translate());

        var headerLabel = listingStandard.Label("RimQuest_Hospitality_ValidQuests".Translate());

        instance.Settings.incidentSettings ??= new Dictionary<IncidentDef, bool>();

        instance.Settings.questSettings ??= new Dictionary<QuestScriptDef, bool>();

        if (instance.Settings.incidentSettings.Any() || instance.Settings.questSettings.Any())
        {
            drawButton(() =>
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
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("RimQuest_version_label".Translate(currentVersion));
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

        listingStandard.End();


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
            if (instance.Settings.questSettings.ContainsKey(questScriptDef))
            {
                if (instance.Settings.questSettings[questScriptDef] == selectedValue)
                {
                    instance.Settings.questSettings.Remove(questScriptDef);
                }
                else
                {
                    selectedValue = instance.Settings.questSettings[questScriptDef];
                }
            }

            var wasValue = selectedValue;


            Widgets.CheckboxLabeled(selectorRect, questLabel, ref selectedValue);

            if (wasValue != selectedValue)
            {
                instance.Settings.questSettings[questScriptDef] = selectedValue;
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
            if (instance.Settings.incidentSettings.ContainsKey(incidentDef))
            {
                if (instance.Settings.incidentSettings[incidentDef] == selectedValue)
                {
                    instance.Settings.incidentSettings.Remove(incidentDef);
                }
                else
                {
                    selectedValue = instance.Settings.incidentSettings[incidentDef];
                }
            }

            var wasValue = selectedValue;


            Widgets.CheckboxLabeled(selectorRect, incidentLabel, ref selectedValue);

            if (wasValue != selectedValue)
            {
                instance.Settings.incidentSettings[incidentDef] = selectedValue;
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