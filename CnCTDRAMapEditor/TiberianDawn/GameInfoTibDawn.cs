//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.TiberianDawn
{
    public class GameInfoTibDawn : GameInfo
    {
        public override GameType GameType => GameType.TiberianDawn;
        public override string Name => "Tiberian Dawn";
        public override string ShortName => "TD";
        public override string IniName => "TiberianDawn";
        public override string DefaultSaveDirectory => Path.Combine(Globals.RootSaveDirectory, "Tiberian_Dawn");
        public override string OpenFilter => Constants.FileFilter;
        public override string SaveFilter => Constants.FileFilter;
        public override string DefaultExtension => ".ini";
        public override string DefaultExtensionFromMix => ".ini";
        public override string ModFolder => Path.Combine(Globals.ModDirectory, "Tiberian_Dawn");
        public override string ModIdentifier => "TD";
        public override string ModsToLoad => Properties.Settings.Default.ModsToLoadTD;
        public override string ModsToLoadSetting => "ModsToLoadTD";
        public override string WorkshopTypeId => "TD";
        public override string ClassicFolder => Properties.Settings.Default.ClassicPathTD;
        public override string ClassicFolderRemaster => "CNCDATA\\TIBERIAN_DAWN";
        public override string ClassicFolderRemasterData => ClassicFolderRemaster + "\\CD1";
        public override string ClassicFolderDefault => "Classic\\TD\\";
        public override string ClassicFolderSetting => "ClassicPathTD";
        public override string ClassicStringsFile => "conquer.eng";
        public override Size MapSize => Constants.MaxSize;
        public override Size MapSizeMega => Constants.MaxSizeMega;
        public override TheaterType[] AllTheaters => TheaterTypes.GetAllTypes().ToArray();
        public override TheaterType[] AvailableTheaters => TheaterTypes.GetTypes().ToArray();
        public override bool MegamapIsSupported => true;
        public override bool MegamapIsOptional => true;
        public override bool MegamapIsDefault => false;
        public override bool MegamapIsOfficial => false;
        public override bool HasSinglePlayer => true;
        public override bool CanUseNewMixFormat => false;
        public override int MaxTriggers => Constants.MaxTriggers;
        public override int MaxTeams => Constants.MaxTeams;
        public override int HitPointsGreenMinimum => 127;
        public override int HitPointsYellowMinimum => 63;
        public override OverlayTypeFlag OverlayIconType => OverlayTypeFlag.Crate;

        public string ModFile => "mod.ini";

        public override IGamePlugin CreatePlugin(Boolean mapImage, Boolean megaMap) => new GamePluginTD(mapImage, megaMap);

        public override void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster)
        {
            // This function is used by Sole Survivor too, so it references the local GameType and ShortName.
            string prefix = ShortName + ": ";
            mfm.Reset(GameType.None, null);
            // Contains cursors / strings file
            mfm.LoadArchive(GameType, "local.mix", false);
            mfm.LoadArchive(GameType, "cclocal.mix", false);
            // Mod addons
            mfm.LoadArchives(GameType, "sc*.mix", false);
            mfm.LoadArchive(GameType, "conquer.mix", false);
            // Theaters
            foreach (TheaterType tdTheater in AllTheaters)
            {
                StartupLoader.LoadClassicTheater(mfm, GameType, tdTheater, false);
            }
            // Check files.
            mfm.Reset(GameType, null);
            List<string> loadedFiles = mfm.ToList();
            // Check required files.
            if (!forRemaster)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "local.mix", "cclocal.mix");
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "conquer.mix");
            }
            foreach (TheaterType tdTheater in AllTheaters)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, tdTheater, !tdTheater.IsModTheater);
            }
            if (!forRemaster)
            {
                StartupLoader.TestFileExists(mfm, fileLoadErrors, prefix, "conquer.eng");
            }
        }

        public override void InitModFiles(List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster)
        {
            System.Threading.Thread.Sleep(3000);
            string folder = Path.GetFullPath(Path.Combine(Program.ApplicationPath, forRemaster ? ClassicFolderRemaster : ClassicFolder, ModFile));
            if (File.Exists(folder))
            {
                INI ini = GeneralUtils.GetIniContents(folder, FileType.INI);
                if (ini != null)
                {
                    // [BuildingTypes]
                    if (ini.Sections["BuildingTypes"] is INISection buildingSec)
                    {
                        foreach (var line in buildingSec)
                        {
                            string[] tokens = line.Value.Split(',');
                            string text = tokens.Length > 0 ? tokens[0] : null;
                            string spowerProd = tokens.Length > 1 ? tokens[1] : string.Empty;
                            string spowerUse = tokens.Length > 2 ? tokens[2] : string.Empty;
                            string sstorage = tokens.Length > 3 ? tokens[3] : string.Empty;
                            string scapturable = tokens.Length > 4 ? tokens[4] : string.Empty;
                            string swidth = tokens.Length > 5 ? tokens[5] : string.Empty;
                            string sheight = tokens.Length > 6 ? tokens[6] : string.Empty;
                            string occupyMask = tokens.Length > 7 ? tokens[7] : null;
                            string ownerHouse = tokens.Length > 8 ? tokens[8] : null;
                            string factoryOverlay = tokens.Length > 9 ? tokens[9] : null;
                            string sframeOffset = tokens.Length > 10 ? tokens[10] : string.Empty;
                            string graphicsSource = tokens.Length > 11 ? tokens[11] : null;
                            string sflag = tokens.Length > 12 ? tokens[12] : string.Empty;
                            string szOrder = tokens.Length > 13 ? tokens[13] : string.Empty;

                            if ("NONE".Equals(factoryOverlay, StringComparison.OrdinalIgnoreCase)) { factoryOverlay = null; }
                            if ("NONE".Equals(graphicsSource, StringComparison.OrdinalIgnoreCase)) { graphicsSource = null; }

                            int.TryParse(spowerProd, out int powerProd);
                            int.TryParse(spowerUse, out int powerUse);
                            int.TryParse(sstorage, out int storage);
                            bool.TryParse(scapturable, out bool capturable);
                            int.TryParse(swidth, out int width);
                            int.TryParse(sheight, out int height);
                            int.TryParse(sframeOffset, out int frameOffset);
                            BuildingTypeFlag.TryParse(sflag, out BuildingTypeFlag flag);
                            int.TryParse(szOrder, out int zOrder);
                            if (text != null)
                            {
                                BuildingTypes.ModifyOrAdd(line.Key, text, powerProd, powerUse, storage, capturable, width, height, occupyMask, ownerHouse, factoryOverlay, frameOffset, graphicsSource, flag, zOrder);
                            }
                        }
                    }

                    // [InfantryTypes]
                    if (ini.Sections["InfantryTypes"] is INISection infantrySec)
                    {
                        foreach (var line in infantrySec)
                        {
                            string[] tokens = line.Value.Split(',');
                            string text = tokens.Length > 0 ? tokens[0] : null;
                            string ownerHouse = tokens.Length > 1 ? tokens[1] : null;
                            string sflags = tokens.Length > 2 ? tokens[2] : string.Empty;

                            UnitTypeFlag.TryParse(sflags, out UnitTypeFlag flags);
                            if (text != null)
                            {
                                InfantryTypes.ModifyOrAdd(line.Key, text, ownerHouse, flags);
                            }
                        }
                    }

                    // [VehicleTypes]
                    if (ini.Sections["VehicleTypes"] is INISection vehicleSec)
                    {
                        foreach (var line in vehicleSec)
                        {
                            string[] tokens = line.Value.Split(',');
                            string text = tokens.Length > 0 ? tokens[0] : null;
                            string ownerHouse = tokens.Length > 1 ? tokens[1] : null;
                            string sbodyFrameUsage = tokens.Length > 2 ? tokens[2] : string.Empty;
                            string sturrFrameUsage = tokens.Length > 3 ? tokens[3] : string.Empty;
                            string sturrOffset = tokens.Length > 4 ? tokens[4] : string.Empty;
                            string sturrY = tokens.Length > 5 ? tokens[5] : string.Empty;
                            string sflags = tokens.Length > 6 ? tokens[6] : string.Empty;

                            FrameUsage.TryParse(sbodyFrameUsage, out FrameUsage bodyFrameUsage);
                            FrameUsage.TryParse(sturrFrameUsage, out FrameUsage turrFrameUsage);
                            int.TryParse(sturrOffset, out int turrOffset);
                            int.TryParse(sturrY, out int turrY);
                            UnitTypeFlag.TryParse(sflags, out UnitTypeFlag flags);
                            if (text != null)
                            {
                                UnitTypes.ModifyOrAddVehicle(line.Key, text, ownerHouse, bodyFrameUsage, turrFrameUsage, turrOffset, turrY, flags);
                            }
                        }
                    }

                    // [AircraftTypes]
                    if (ini.Sections["AircraftTypes"] is INISection aircraftSec)
                    {
                        foreach (var line in aircraftSec)
                        {
                            string[] tokens = line.Value.Split(',');
                            string text = tokens.Length > 0 ? tokens[0] : null;
                            string ownerHouse = tokens.Length > 1 ? tokens[1] : null;
                            string sbodyFrameUsage = tokens.Length > 2 ? tokens[2] : string.Empty;
                            string sturrFrameUsage = tokens.Length > 3 ? tokens[3] : string.Empty;
                            string turret = tokens.Length > 4 ? tokens[4] : null;
                            string turret2 = tokens.Length > 5 ? tokens[5] : null;
                            string sturrOffset = tokens.Length > 6 ? tokens[6] : string.Empty;
                            string sturrY = tokens.Length > 7 ? tokens[7] : string.Empty;
                            string sflags = tokens.Length > 8 ? tokens[8] : string.Empty;

                            if ("NONE".Equals(turret, StringComparison.OrdinalIgnoreCase)) { turret = null; }
                            if ("NONE".Equals(turret2, StringComparison.OrdinalIgnoreCase)) { turret2 = null; }

                            FrameUsage.TryParse(sbodyFrameUsage, out FrameUsage bodyFrameUsage);
                            FrameUsage.TryParse(sturrFrameUsage, out FrameUsage turrFrameUsage);
                            int.TryParse(sturrOffset, out int turrOffset);
                            int.TryParse(sturrY, out int turrY);
                            UnitTypeFlag.TryParse(sflags, out UnitTypeFlag flags);
                            if (text != null)
                            {
                                UnitTypes.ModifyOrAddAircraft(line.Key, text, ownerHouse, bodyFrameUsage, turrFrameUsage, turret, turret2, turrOffset, turrY, flags);
                            }
                        }
                    }

                    // [TerrainTypes]
                    if (ini.Sections["TerrainTypes"] is INISection terrainSec)
                    {
                        foreach (var line in terrainSec)
                        {
                            string[] tokens = line.Value.Split(',');
                            string text = tokens.Length > 0 ? tokens[0] : null;
                            string swidth = tokens.Length > 1 ? tokens[1] : string.Empty;
                            string sheight = tokens.Length > 2 ? tokens[2] : string.Empty;
                            string scenterX = tokens.Length > 3 ? tokens[3] : string.Empty;
                            string scenterY = tokens.Length > 4 ? tokens[4] : string.Empty;
                            string occupyMask = tokens.Length > 5 ? tokens[5] : null;

                            int.TryParse(swidth, out int width);
                            int.TryParse(sheight, out int height);
                            int.TryParse(scenterX, out int centerX);
                            int.TryParse(scenterY, out int centerY);
                            if (text != null)
                            {
                                TerrainTypes.ModifyOrAdd(line.Key, text,  width, height, centerX, centerY, occupyMask);
                            }
                        }
                    }

                    // [TheaterTypes]
                    if (ini.Sections["TheaterTypes"] is INISection theaterSec)
                    {
                        foreach (var line in theaterSec)
                        {
                            string[] tokens = line.Value.Split(',');
                            string tileset = tokens.Length > 0 ? tokens[0] : null;
                            string ext = tokens.Length > 1 ? tokens[1] : null;
                            string smodTheater = tokens.Length > 2 ? tokens[2] : string.Empty;
                            string remasteredTileset = tokens.Length > 3 ? tokens[3] : null;

                            bool.TryParse(smodTheater, out bool modTheater);
                            if (tileset != null && ext != null)
                            {
                                TheaterTypes.ModifyOrAdd(line.Key, tileset, ext, modTheater, remasteredTileset);
                            }
                        }
                    }
                }
            }
        }

        public override string GetClassicOpposingPlayer(string player) => HouseTypes.GetClassicOpposingPlayer(player);

        public override bool SupportsMapLayer(MapLayerFlag mlf)
        {
            MapLayerFlag badLayers = MapLayerFlag.BuildingFakes | MapLayerFlag.EffectRadius | MapLayerFlag.FootballArea;
            return mlf == (mlf & ~badLayers);
        }

        public override Bitmap GetWaypointIcon()
        {
            return Globals.TheTilesetManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_SELECT_FRIENDLY_X2_00.DDS", "mouse", 12, true);
        }

        public override Bitmap GetCellTriggerIcon()
        {
            return Globals.TheTilesetManager.GetTile("mine", 3, "mine.shp", 3, null);
        }

        public override Bitmap GetSelectIcon()
        {
            // Remaster: Chronosphere cursor from TEXTURES_SRGB.MEG
            // Alt: @"DATA\ART\TEXTURES\SRGB\ICON_IONCANNON_15.DDS"
            // Classic: Ion Cannon cursor
            return Globals.TheTilesetManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_SELECT_GREEN_04.DDS", "mouse", 118, true);
        }

        public override Bitmap GetCaptureIcon()
        {
            return Globals.TheTilesetManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_MOUNT_UNIT_X2_02.DDS", "mouse", 121, true);
        }

        public override string EvaluateBriefing(string briefing)
        {
            if (!Globals.WriteClassicBriefing)
            {
                return null;
            }
            string briefText = (briefing ?? String.Empty).Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n");
            // Remove duplicate spaces
            briefText = Regex.Replace(briefText, " +", " ");
            if (briefText.Length > Constants.MaxBriefLengthClassic)
            {
                return "Classic Tiberian Dawn briefings cannot exceed " + Constants.MaxBriefLengthClassic + " characters. This includes line breaks.\n\nThis will not affect the mission when playing in the Remaster, but the briefing will be truncated when playing in the original game.";
            }
            return null;
        }

        public override bool MapNameIsEmpty(string name)
        {
            return String.IsNullOrEmpty(name) || Constants.EmptyMapName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        public override string GetClassicFontInfo(ClassicFont font, TilesetManagerClassic tsmc, TeamRemapManager trm, Color textColor, out bool crop, out TeamRemap remap)
        {
            crop = false;
            remap = null;
            string fontName = null;
            int[] toClear;
            switch (font)
            {
                case ClassicFont.Waypoints:
                    crop = true;
                    fontName = "8point.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, 2, 3);
                    break;
                case ClassicFont.WaypointsLong: // The DOS 6point.fnt would be ideal for this, but they replaced it with a much larger one in C&C95.
                    crop = true;
                    fontName = "6ptdos.fnt";
                    toClear = new int[] { 2, 3 };
                    if (!tsmc.TileExists(fontName))
                    {
                        fontName = "scorefnt.fnt";
                        toClear = new int[0];
                    }
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, toClear);
                    break;
                case ClassicFont.CellTriggers:
                    crop = true;
                    fontName = "scorefnt.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor);
                    break;
                case ClassicFont.RebuildPriority:
                    crop = true;
                    fontName = "scorefnt.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor);
                    break;
                case ClassicFont.TechnoTriggers:
                    crop = true;
                    fontName = "6ptdos.fnt";
                    toClear = new int[] { 2, 3 };
                    if (!tsmc.TileExists(fontName))
                    {
                        fontName = "scorefnt.fnt";
                        toClear = new int[0];
                    }
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, toClear);
                    break;
                case ClassicFont.TechnoTriggersSmall:
                    crop = true;
                    fontName = "5pntthin.fnt";
                    if (!tsmc.TileExists(fontName))
                    {
                        fontName = "3point.fnt";
                    }
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor);
                    break;
                case ClassicFont.FakeLabels:
                    break;
            }
            if (!tsmc.TileExists(fontName))
            {
                fontName = null;
            }
            return fontName;
        }

    }
}
