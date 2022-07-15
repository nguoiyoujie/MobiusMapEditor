## C&C Tiberian Dawn and Red Alert Map Editor

An enhanced version of the C&C Tiberian Dawn and Red Alert Map Editor based on the source code released by Electronic Arts.
The goal of the project is simply to improve the usability and convenience of the map editor, fix bugs, improve and clean its code-base,
enhance compatibility with different kinds of systems and enhance the editor's support for mods.

### Contributing

Right now, I'm not really looking into making this a joint project. Specific bug reports and suggestions are always welcome though, but post them as issues.

### Upgrading

The settings of the program are apparently automatically saved under

%localappdata%\Nyerguds\

So if you install a new version, and your settings are gone, just go there and copy the settings file to the newest folder.

---

## Change log

### Features added by Rampastring:

* Downsized menu graphics by a user-configurable factor so you can see more placeable object types at once on sub-4K monitors.
* Improved zoom levels.
* Fixed a couple of crashes.
* Made tool windows remember their previous position, size and other settings upon closing and re-opening them.
* Replaced drop-downs with list boxes in object type selection dialogs to allow switching between objects with fewer clicks.

### Features and fixes by Nyerguds (so far):

v1.4.0.0:

* Fixed Overlay height overflow bug in Rampa's new UI.
* Fixed tiles list duplicating every time the "Map" tool window is opened in Rampa's version.
* Split off internal Overlay type "decoration", used for pavements and civilian buildings.
* Added CONC and ROAD pavement. They have no graphics, but at least now they are accepted by the editor and not discarded as errors.
* Sorted all items in the lists (except map tiles) by key, which is usually a lot more straightforward.
* Split off specific separate list for techno types usable in teamtypes.
* Removed the Aircraft from the placeable units in TD.
* Removed irrelevant orders from the unit missions list (Selling, Missile, etc.)
* Fixed case sensitivity related crashes in TD teamtypes.
* TD Triggers without a Teamtype will now automatically get "None" filled in as Teamtype, fixing the malfunctioning of their repeat status.
* Added Ctrl-N, Ctrl+O, Ctrl+S etc shortcuts for the File menu.
* Fixed double indicator on map tile selection window.
* Fixed smudge reading in TD to allow 5 crater stages.
* Added tool window to adjust crater stage.
* Fixed Terrain objects not saving their trigger. Note that only "Attacked" triggers work on them.
* RA "Spied by..." trigger event now shows the House to select.
* Added "Add" buttons in triggers and teamtypes dialogs.
* Fixed tab order in triggers and teamtypes dialogs.
* Fixed crash in "already exists" messages for triggers and teams.
* Randomised tiberium on save, like the original WW editor does. (this is purely cosmetic; the game re-randomises it on map load.)
* [EXPERIMENTAL] Added ability to place bibs as Smudge type. They won't show their full size in the editor at the moment, though.

v1.4.0.1:

* Added "All supported types (\*.ini;\*.bin;\*.mpr)" as default filter when opening files.
* Added Drag & Drop support for opening map files.
* Added command line file argument support, which allows setting the editor as application for opening ini/mpr files.
* House Edge reading now corrects values with case differences so they show up in the dropdown.
* Centralised the House Edge array on the House class, and changed its order to a more logical North, East, South, West.
* Fixed order of the Multi-House colours. It seems the error is not in the editor, but in bizarre mixed-up team color names in the remastered game itself.
* Remapped Neutral (TD only) and Special as yellow, as they are in the game.
* All tool windows will now save their position.
* Tool windows for which no position was previously set will center themselves on the right edge of the editor.
* Some things, like crates, were missing names. This has been fixed.
* All objects except map tilesets will now show a real name and their internal code.
* Added ASCII restriction to trigger and teamtype names, since the map formats don't support UTF-8. (Except on the Briefing, apparently, since the GlyphX part handles that.)
* Made "Already exists" check on trigger and teamtype names case insensitive, since that is how the game handles them.
* Triggers and teamtypes dialogs have a new logic for generating names for new entries that should never run out.
* Triggers and teamtypes dialogs support the delete key for deleting an entry in the list.
* Triggers and teamtypes dialogs have "Rename" added to the context menu when right-clicking an item.
* Triggers and teamtypes dialogs now warn when cancelling if changes were made.
* "Add" button in triggers and teamtypes dialogs gets disabled when the internal maximum amount of items for the type is reached.
* Changed the default build level in TD maps from 99 to 98. Level 99 allows building illegal objects that can break the game.
* The Briefing text area will now accept [Enter] for adding line breaks without this closing the window. Previously, [Ctrl]+[Enter] had to be used for this, which is pretty awkward.
* The Briefing text area now has a scrollbar.
* Fixed placement of illegal tiles caused by incorrect filtering on which tiles from a template should be included. This is the problem which caused tiles that showed as black blocks in classic graphics. It is also the problem that made Red Alert maps contain indestructible bridges.
* Map tile placement can now be dragged, allowing easily filling an area with water or other tiles. This also works for removing tiles.
* Removing tiles will now obey the actual occupied cells of the selected tile, rather than just clearing the bounding box, making it more intuitive.
* Creating an RA trigger with Action "Text Trigger" will no longer cause an error to be shown.
* Trigger controls no longer jump around slightly when selecting different options.
* Using the mouse wheel will now change the tiberium field size per 2, like a normal arrow click would.

v1.4.0.2:

* Fixed the bug that cleared all map templates on save in v1.4.0.1 (whoops).
* Fixed the bug in the Teamtypes list that showed the wrong context menu options on right click.
* Fixed the bug that the status bar did not show the map placement shortcuts hints on initial load.
* The editor no longer exits if it cannot connect to Steam. Instead, workshop publishing will simply be disabled if the Steamworks interface can't be initialised.
* The texture manager will now properly dispose all loaded image objects when a different map is loaded.
* Added \*.ini to the list of possible extensions for opening RA maps, to support opening pre-Remaster missions.
* If a building has no direction to set and shows no dropdown for it, the "Direction" label is now also now removed.
* Structure graphics are now correctly centered on their full building size.
* Damaged state is now correctly shown at strength 128/256, and not below it.
* Damaged states now work correctly on all buildings.
* Using the mouse wheel will now change the strength of objects in increments of 4.
* IQ of all Houses in Red Alert now defaults to 0.
* Fixed gunboat facing and damage states logic.
* Fixed bug causing bad refresh when previewing the placement of a single cell selected from a template with an empty top right corner cell.
* The "clear1" tile is now explicitly shown in the tiles list.
* Teamtype "Priority" value (recruit priority) is now capped at 15.

v1.4.0.3: [WIP]

* Removed limitation on placing resources on the top and bottom row of the map.
* The 'clamping' logic that prevented tool windows from being dragged outside usable screen bounds had a bug that this prevented it from being dragged onto a different monitor. This is now fixed.
* "Theme" has been added to the map settings. Do note this has no effect on the Remaster.
* All videos available in the Remaster are now shown in the video lists in the "Map settings" dialog.
* Added missing entries (videos not included in the Remaster) to the RA and TD video lists, with a 'Classic only' indicator.
* Added tooltips for the team type options.
* Fixed tab order of the Temtype options.
