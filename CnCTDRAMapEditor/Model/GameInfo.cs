﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using MobiusEditor.Interface;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MobiusEditor.Model
{
    /// <summary>
    /// <para>The GameInfo class is meant to get rid of most of the logic checking the
    /// <see cref="Model.GameType"/> enum and instead centralise it all in a set of game-specific
    /// classes implementating this one. The main principle of this class, as opposed to an
    /// <see cref="IGamePlugin"/> implementation, is that <b>it contains no variable data</b>.
    /// Its creation does not initialise anything.
    /// </para><para>
    /// It has properties and methods that fetch game-specific information or execute
    /// game-specific logic, but returns results based either on hardcoded data, or
    /// on unchanging data initialised on program startup. This means its methods
    /// should <i>never</i> use data from an <see cref="IGamePlugin"/> as argument.</para>
    /// </summary>
    public abstract class GameInfo
    {
        public const string TRIG_ARG_FORMAT = "{0}: {1}";
        public string[] PERSISTENCE_NAMES = { "first triggered", "all triggered", "each triggering" };
        #region properties
        /// <summary>GqmeType enup for this game.</summary>
        public abstract GameType GameType { get; }
        /// <summary>Name used for this game.</summary>
        public abstract string Name { get; }
        /// <summary>Short name used for this game.</summary>
        public abstract string ShortName { get; }
        /// <summary>Name used for this game in the mixcontent.ini definition.</summary>
        public abstract string IniName { get; }
        /// <summary>Default remaster folder for saving maps of this type.</summary>
        public abstract string DefaultSaveDirectory { get; }
        /// <summary>"Save File" filter for maps for this game.</summary>
        public abstract string SaveFilter { get; }
        /// <summary>"Open File" filter for maps for this game.</summary>
        public abstract string OpenFilter { get; }
        /// <summary>Default extension used for maps of this game.</summary>
        public abstract string DefaultExtension { get; }
        /// <summary>Default extension used for maps of this game when they were loaded from inside a .mix file.</summary>
        public abstract string DefaultExtensionFromMix { get; }
        /// <summary>Location to look for mods for this game in the user's Documents folder.</summary>
        public abstract string ModFolder { get; }
        /// <summary>Identifier for this game in mod definition json files.</summary>
        public abstract string ModIdentifier { get; }
        /// <summary>Mods to load for this game.</summary>
        public abstract string ModsToLoad { get; }
        /// <summary>Name of the setting that configures this game's mods to load.</summary>
        public abstract string ModsToLoadSetting { get; }
        /// <summary>Workshop identifier for maps for this game. If empty, workshop is not supported for this game.</summary>
        public abstract string WorkshopTypeId { get; }
        /// <summary>Configured folder for this this game's Classic files</summary>
        public abstract string ClassicFolder { get; }
        /// <summary>Root folder of this game's Classic files under in the Remastered Collection's "Data" folder.</summary>
        public abstract string ClassicFolderRemaster { get; }
        /// <summary>Location of this game's Classic files to use under in the Remastered Collection's "Data" folder.</summary>
        public abstract string ClassicFolderRemasterData { get; }
        /// <summary>Default for this this game's Classic folder</summary>
        public abstract string ClassicFolderDefault { get; }
        /// <summary>Name of the setting that configures this game's Classic folder</summary>
        public abstract string ClassicFolderSetting { get; }
        /// <summary>File name of the classic strings file for this game.</summary>
        public abstract string ClassicStringsFile { get; }
        /// <summary>Map size for normal maps of this type</summary>
        public abstract Size MapSize { get; }
        /// <summary>Map size for megamaps of this type</summary>
        public abstract Size MapSizeMega { get; }
        /// <summary>Lists all theaters theoretically supported by this type.</summary>
        public abstract TheaterType[] AllTheaters { get; }
        /// <summary>Lists all theaters supported by this type which are actually found.</summary>
        public abstract TheaterType[] AvailableTheaters { get; }
        /// <summary>Whether megamaps are supported for this game.</summary>
        public abstract bool MegamapIsSupported { get; }
        /// <summary>Whether megamaps are optional for this game.</summary>
        public abstract bool MegamapIsOptional { get; }
        /// <summary>Whether maps of this game default to being megamaps.</summary>
        public abstract bool MegamapIsDefault { get; }
        /// <summary>Whether megamap support for this game is official, or only supported through mods.</summary>
        public abstract bool MegamapIsOfficial { get; }
        /// <summary>Whether single player scenarios exist for this game.</summary>
        public abstract bool HasSinglePlayer { get; }
        /// <summary>Whether mix files of this game can be in the new mix format</summary>
        public abstract bool CanUseNewMixFormat { get; }
        /// <summary>Maximum amount of triggers that can be added into a map of this game.</summary>
        public abstract int MaxTriggers { get; }
        /// <summary>Maximum amount of teams that can be added into a map of this game.</summary>
        public abstract int MaxTeams { get; }
        /// <summary>Threshold (1-256) at which the health bar colour changes from yellow to green in this game.</summary>
        public abstract int HitPointsGreenMinimum { get; }
        /// <summary>Threshold (1-256) at which the health bar colour changes from red to yellow in this game.</summary>
        public abstract int HitPointsYellowMinimum { get; }
        /// <summary>Preferred type of overlay to use as UI icon</summary>
        public abstract OverlayTypeFlag OverlayIconType { get; }
        #endregion

        #region functions
        /// <summary>
        /// Create game plugin for this game.
        /// </summary>
        /// <param name="mapImage">True if a map image should be created.</param>
        /// <param name="megaMap">True if this plugin will be handling a megamap.</param>
        /// <returns></returns>
        public abstract IGamePlugin CreatePlugin(bool mapImage, bool megaMap);
        /// <summary>
        /// Initialises the classic files for this game. If <paramref name="forRemaster"/> is false, this should do more detailed checks on which assets are required.
        /// </summary>
        /// <param name="mfm">MixfileManager to load the archives into.</param>
        /// <param name="loadErrors">List of load errors. </param>
        /// <param name="fileLoadErrors">A list to collect errors into.</param>
        /// <param name="forRemaster">Indicates if this init is for remastered mode.</param>
        public abstract void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster);
        /// <summary>
        /// Initialises modifications for this game. Useful for redefining internal object lists
        /// </summary>
        /// <param name="loadErrors">List of load errors. </param>
        /// <param name="fileLoadErrors">A list to collect errors into.</param>
        /// <param name="forRemaster">Indicates if this init is for remastered mode.</param>
        public abstract void InitModFiles(List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster);
        /// <summary>Retrieves the typical opposing player for the given House name, e.g. for TD, GoodGuy will give BadGuy.</summary>
        /// <param name="player">The player to get the opposing player for.</param>
        /// <returns>The typical opposing player for the given House.</returns>
        public abstract string GetClassicOpposingPlayer(string player);
        /// <summary>Checks if the given map layer is relevant for this game type.</summary>
        /// <param name="mlf">The map layer flag to check.</param>
        /// <returns>True if the given layer is used for this game.</returns>
        public abstract bool SupportsMapLayer(MapLayerFlag mlf);
        /// <summary>Fetches the Waypoints-mode icon for the UI. This returns a new image that needs to be disposed afterwards.</summary>
        /// <returns>The waypoints UI icon.</returns>
        public abstract Bitmap GetWaypointIcon();
        /// <summary>Fetches the Celltriggers-mode icon for the UI. This returns a new image that needs to be disposed afterwards.</summary>
        /// <returns>The celltriggers UI icon.</returns>
        public abstract Bitmap GetCellTriggerIcon();
        /// <summary>Fetches the Select-mode icon for the UI. This returns a new image that needs to be disposed afterwards.</summary>
        /// <returns>The select mode UI icon.</returns>
        public abstract Bitmap GetSelectIcon();
        /// <summary>Fetches the Capture icon for the building properties UI. This returns a new image that needs to be disposed afterwards.</summary>
        /// <returns>An icon indicating this object is capturable.</returns>
        public abstract Bitmap GetCaptureIcon();
        /// <summary>Checks whether the briefing has any kind of issues concerning length or supported characters.</summary>
        /// <param name="briefing">The briefing to check</param>
        /// <returns>Null if everything is okay, otherwise any issues to show on the user interface.</returns>
        public abstract string EvaluateBriefing(string briefing);
        /// <summary>Checks whether the map has a default map name that is considered empty by this game type.</summary>
        /// <param name="name">Map name to check.</param>
        /// <returns>True if the given name is considered empty by this game type.</returns>
        public abstract bool MapNameIsEmpty(string name);
        /// <summary>Retrieves classic font info from this game to use for the requested role.</summary>
        public abstract string GetClassicFontInfo(ClassicFont font, TilesetManagerClassic tsmc, TeamRemapManager trm, Color textColor, out bool crop, out TeamRemap remap);
        #endregion

        #region protected functions

        /// <summary>
        /// Creates a remap object for a specific font, by remapping all indices to the closest color on te palette.
        /// A list of indices to clear can be given, which remaps those to index 0 on the palette.
        /// </summary>
        /// <param name="fontName">font name, to use in the remap name.</param>
        /// <param name="tsmc">Classic tileset manager, to get the color info from.</param>
        /// <param name="textColor">Requested color for the text. Probably won't match exactly since it is looked up in the palette.</param>
        /// <param name="clearIndices">Indices on the graphics that need to be cleared to transparent (index 0).</param>
        /// <returns>A TeamRemap object for the given color.</returns>
        /// <remarks>The generated remap is cached in the TeamRemapManager.</remarks>
        protected TeamRemap GetClassicFontRemapSimple(string fontName, TilesetManagerClassic tsmc, TeamRemapManager trm, Color textColor, params int[] clearIndices)
        {
            if (fontName == null)
            {
                return null;
            }
            List<int> indicesFiltered = (clearIndices ?? new int[0]).Where(x => x > 0 && x < 16).ToList();
            indicesFiltered.Sort();
            string cleared = String.Join("-", indicesFiltered.Select(i => i.ToString("X")));
            string remapName = "FontRemap_" + fontName + "_" + textColor.ToArgb().ToString("X4") + (cleared.Length > 0 ? "_" : string.Empty) + cleared;
            TeamRemap fontRemap = trm.GetItem(remapName);
            if (fontRemap != null)
            {
                return fontRemap;
            }
            int color = tsmc.GetClosestColorIndex(textColor, true);
            // Extremely simple: all indices except 0 remap to the given colour.
            byte[] remapIndices = 0.Yield().Concat(Enumerable.Repeat(color, 15)).Select(b => (byte)b).ToArray();
            if (indicesFiltered.Count > 0)
            {
                foreach (int index in indicesFiltered)
                {
                    remapIndices[index] = 0;
                }
            }
            fontRemap = new TeamRemap(remapName, 0, 0, 0, remapIndices);
            trm.AddTeamColor(fontRemap);
            return fontRemap;
        }

        protected static string GetMissionName(char side, int number, string suffix)
        {
            return GetMissionName(side, number, suffix, '\0');
        }

        protected static string GetMissionName(char side, int number, string suffix, char aftermathLetter)
        {
            const string formatNormal = "sc{0}{1:00}{2}";
            const string formatAm = "sc{0}{3}{1}{2}";
            return String.Format(aftermathLetter == '\0' ? formatNormal : formatAm, side, number, suffix, aftermathLetter);
        }
        #endregion

        #region overrides
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }

    public enum ClassicFont
    {
        /// <summary>Font used for Waypoints.</summary>
        Waypoints,
        /// <summary>Font used for waypoints with longer names. Separate because it needs a smaller font to fit inside one cell.</summary>
        WaypointsLong,
        /// <summary>Font used for cell triggers.</summary>
        CellTriggers,
        /// <summary>Font used for techno triggers on multi-cell objects.</summary>
        TechnoTriggers,
        /// <summary>Font used for one-cell techno triggers. Separate because it might need to be smaller.</summary>
        TechnoTriggersSmall,
        /// <summary>Font used for rebuild priority numbers on buildings.</summary>
        RebuildPriority,
        /// <summary>Font used for "FAKE" labels on buildings.</summary>
        FakeLabels
    }
}
