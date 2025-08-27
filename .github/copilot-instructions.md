# Copilot Instructions for RimQuest (Continued)

## Mod Overview and Purpose

RimQuest (Continued) is an enhancement of Jecrell's original RimWorld mod, designed to introduce additional narrative elements and quest opportunities into the game. This mod aims to enrich the player's experience by adding quest givers to trader groups and caravans. It provides a sense of direction and purpose, encouraging the player to engage with quests that can be accessed through characters marked with a green exclamation mark during caravan and trading events.

## Key Features and Systems

- **Support for New Quest Types**: This mod includes integration for the newer `QuestScriptDef` types in addition to the traditional `IncidentDef`, expanding the variety of quests available.
- **Hospitality Mod Integration**: Includes guest group support from Orion's Hospitality mod, offering more interaction options.
- **Localization**: Offers localization support with Russian and French translations, improving accessibility for non-English-speaking players.
- **Dynamic Quest-Givers**: Quest-givers are added to traveling groups and their availability can be customized through settings.
- **Customizable Quest System**: Players can modify which quests and incidents can be purchased and adjust the base price and spawn rate of quests.
- **Performance Optimizations**: Various enhancements for improved performance and smoother gameplay.

## Coding Patterns and Conventions

- **Code Structure**: The code is organized into descriptive classes and methods that adhere to C# naming conventions such as PascalCase for method names and camelCase for local variables.
- **Modularity**: Functionality is broken into specialized classes such as `IncidentWorker_VisitorGroup_GiveItems`, and `QuestPawn` to ensure single-responsibility.
- **Use of DefModExtension**: Includes `RimQuest_ModExtension` for expanding mod capabilities through XML.

## XML Integration

- XML files define the various game elements such as quests and incidents.
- Custom XML loading functionality is implemented in classes such as `IncidentGenOption` and `QuestGenOption` using methods like `LoadDataFromXmlCustom(XmlNode xmlRoot)` to enable dynamic XML configuration.

## Harmony Patching

- **Harmony Patches**: Implemented in the `HarmonyPatches` static class, allowing for non-intrusive modification of core game functionality without directly editing the game’s assemblies.
- **Patch Types**: Includes methods that patch specific game methods through Harmony to ensure compatibility with game updates and other mods.

## Suggestions for Copilot

- **Automate Quest Creation**: Suggest automating the generation of new quests through templates in XML and C#.
- **Advanced Localization**: Assist in introducing translations for newly added content by suggesting placeholder text with appropriate keys.
- **Performance Optimization**: Recommend refactoring methods to optimize performance, especially in frequently called code paths, by analyzing current implementations.
- **Harmony Patch Expansion**: Propose new harmony patches to enhance or tweak additional game mechanics related to quests or trader interactions.
- **User-Friendly Configuration**: Offer tips for expanding the in-game settings menu to include new mod options and intuitive controls for players.
- **Test Coverage**: Encourage the development of unit tests for key systems like quest generation to ensure reliability and correctness as the mod evolves.

Contributions and collaboration are encouraged to continue improving RimQuest (Continued). For code examples or issues, please refer to the specific classes and methods in the project structure.
