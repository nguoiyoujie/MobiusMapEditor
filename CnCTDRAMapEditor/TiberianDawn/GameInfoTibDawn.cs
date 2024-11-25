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

        public override void InitModFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster, Dictionary<string, int> mappedText, Dictionary<string, string> additionalText)
        {
            string folder = Path.GetFullPath(Path.Combine(Program.ApplicationPath, forRemaster ? ClassicFolderRemaster : ClassicFolder, ModFile));
            if (File.Exists(folder))
            {
                INI ini = GeneralUtils.GetIniContents(folder, FileType.INI);
                if (ini != null)
                {
                    // Existing [BuildingTypes]
                    List<BuildingType> buildingtypes = BuildingTypes.GetTypes().ToList();
                    foreach (BuildingType type in buildingtypes)
                    {
                        if (ini.Sections[type.Name] is INISection buildingSec)
                        {
                            InitModBuildingType(buildingSec, mappedText, additionalText);
                        }
                    }

                    // New [BuildingTypes]
                    if (ini.Sections["BuildingTypes"] is INISection buildinglistSec)
                    {
                        foreach (KeyValuePair<string, string> line in buildinglistSec)
                        {
                            if (ini.Sections[line.Value] is INISection buildingSec)
                            {
                                InitModBuildingType(buildingSec, mappedText, additionalText);
                            }
                        }
                    }

                    // Existing [InfantryTypes]
                    List<InfantryType> infantrytypes = InfantryTypes.GetTypes().ToList();
                    foreach (InfantryType type in infantrytypes)
                    {
                        if (ini.Sections[type.Name] is INISection infantrySec)
                        {
                            InitModInfantryType(infantrySec, mappedText, additionalText);
                        }
                    }

                    // New [InfantryTypes]
                    if (ini.Sections["InfantryTypes"] is INISection infantrylistSec)
                    {
                        foreach (KeyValuePair<string, string> line in infantrylistSec)
                        {
                            if (ini.Sections[line.Value] is INISection infantrySec)
                            {
                                InitModInfantryType(infantrySec, mappedText, additionalText);
                            }
                        }
                    }

                    // Existing [VehicleTypes] and [AircraftTypes]
                    List<UnitType> unittypes = UnitTypes.GetTypes(false).ToList();
                    foreach (UnitType type in unittypes)
                    {
                        if (ini.Sections[type.Name] is INISection vehiclelSec)
                        {
                            if (type.IsAircraft)
                            {
                                InitModAircraftType(vehiclelSec, mappedText, additionalText);
                            }
                            else
                            {
                                InitModVehicleType(vehiclelSec, mappedText, additionalText);
                            }
                        }
                    }

                    // New [VehicleTypes]
                    if (ini.Sections["VehicleTypes"] is INISection vehiclelistSec)
                    {
                        foreach (KeyValuePair<string, string> line in vehiclelistSec)
                        {
                            if (ini.Sections[line.Value] is INISection vehiclelSec)
                            {
                                InitModVehicleType(vehiclelSec, mappedText, additionalText);
                            }
                        }
                    }

                    // New [AircraftTypes]
                    if (ini.Sections["AircraftTypes"] is INISection aircraftlistSec)
                    {
                        foreach (KeyValuePair<string, string> line in aircraftlistSec)
                        {
                            if (ini.Sections[line.Value] is INISection aircraftSec)
                            {
                                InitModAircraftType(aircraftSec, mappedText, additionalText);
                            }
                        }
                    }

                    // Existing [TerrainTypes]
                    List<TerrainType> terraintypes = TerrainTypes.GetTypes().ToList();
                    foreach (TerrainType type in terraintypes)
                    {
                        if (ini.Sections[type.Name] is INISection terrainSec)
                        {
                            InitModTerrainType(terrainSec, mappedText, additionalText);
                        }
                    }

                    // New [TerrainTypes]
                    if (ini.Sections["TerrainTypes"] is INISection terrainlistSec)
                    {
                        foreach (KeyValuePair<string, string> line in terrainlistSec)
                        {
                            if (ini.Sections[line.Value] is INISection terrainSec)
                            {
                                InitModTerrainType(terrainSec, mappedText, additionalText);
                            }
                        }
                    }

                    // [TheaterTypes]
                    // parse single line
                    if (ini.Sections["TheaterTypes"] is INISection theaterSec)
                    {
                        string prefix = ShortName + ": ";
                        foreach (KeyValuePair<string, string> line in theaterSec)
                        {
                            string[] tokens = line.Value.Split(',');
                            string tileset = tokens.Length > 0 ? tokens[0] : null;
                            string ext = tokens.Length > 1 ? tokens[1] : null;
                            string smodTheater = tokens.Length > 2 ? tokens[2] : string.Empty;
                            string remasteredTileset = tokens.Length > 3 ? tokens[3] : null;

                            bool.TryParse(smodTheater, out bool modTheater);
                            if (tileset != null && ext != null)
                            {
                                TheaterType tdTheater = TheaterTypes.ModifyOrAdd(line.Key, tileset, ext, modTheater, remasteredTileset);
                                mfm.LoadArchive(GameType, tdTheater.ClassicTileset + ".mix", false);
                            }
                        }

                        // check
                        mfm.Reset(GameType, null);
                        List<string> loadedFiles = mfm.ToList();
                        foreach (var theater in TheaterTypes.GetAllTypes())
                        {
                            StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, theater, !theater.IsModTheater);
                        }
                    }
                }
            }
        }

        private void InitModBuildingType(INISection section, Dictionary<string, int> mappedText, Dictionary<string, string> additionalText)
        {
            string id = section.Name;
            string sTextID = section.TryGetValue("TextID");
            string text = section.TryGetValue("Name");
            string sPowerProd = section.TryGetValue("PowerProd");
            string sPowerUse = section.TryGetValue("PowerUse");
            string sStorage = section.TryGetValue("Storage");
            string sCapturable = section.TryGetValue("Capturable");
            string sWidth = section.TryGetValue("Width");
            string sHeight = section.TryGetValue("Height");
            string occupyMask = section.TryGetValue("OccupyMask");
            string owner = section.TryGetValue("Owner");
            string factoryOverlay = section.TryGetValue("FactoryOverlay");
            string sFrameOffset = section.TryGetValue("FrameOffset");
            string graphicsSource = section.TryGetValue("Image");
            string isConstructionYard = section.TryGetValue("IsConstructionYard");
            string bib = section.TryGetValue("Bib");
            string isTheaterDependent = section.TryGetValue("IsTheaterDependent");
            string isTurret = section.TryGetValue("IsTurret");
            string isSingleFrame = section.TryGetValue("IsSingleFrame");
            string noRemap = section.TryGetValue("NoRemap");
            string isWall = section.TryGetValue("IsWall");
            string sZOrder = section.TryGetValue("ZOrder");

            // parse and apply defaults (int)
            int.TryParse(sTextID, out int textID);
            int.TryParse(sPowerProd, out int powerProd);
            int.TryParse(sPowerUse, out int powerUse);
            int.TryParse(sStorage, out int storage);
            bool capturable = YesNoBooleanTypeConverter.Parse(sCapturable);
            if (!int.TryParse(sWidth, out int width)) { width = 1; }
            if (!int.TryParse(sHeight, out int height)) { height = 1; }
            int.TryParse(sFrameOffset, out int frameOffset);
            BuildingTypeFlag flags = BuildingTypeFlag.None;
            if (YesNoBooleanTypeConverter.Parse(isConstructionYard)) { flags |= BuildingTypeFlag.Factory; }
            if (YesNoBooleanTypeConverter.Parse(bib)) { flags |= BuildingTypeFlag.Bib; }
            if (YesNoBooleanTypeConverter.Parse(isTheaterDependent)) { flags |= BuildingTypeFlag.TheaterDependent; }
            if (YesNoBooleanTypeConverter.Parse(isTurret)) { flags |= BuildingTypeFlag.Turret; }
            if (YesNoBooleanTypeConverter.Parse(isSingleFrame)) { flags |= BuildingTypeFlag.SingleFrame; }
            if (YesNoBooleanTypeConverter.Parse(noRemap)) { flags |= BuildingTypeFlag.NoRemap; }
            if (YesNoBooleanTypeConverter.Parse(isWall)) { flags |= BuildingTypeFlag.Wall; }
            if (!int.TryParse(sZOrder, out int zorder)) { zorder = 10; }

            // apply defaults (string)
            if ("NONE".Equals(factoryOverlay, StringComparison.OrdinalIgnoreCase)) { factoryOverlay = null; }
            if ("NONE".Equals(graphicsSource, StringComparison.OrdinalIgnoreCase)) { graphicsSource = null; }
            if (string.IsNullOrEmpty(occupyMask) || "NONE".Equals(occupyMask, StringComparison.OrdinalIgnoreCase)) { occupyMask = null; }
            if (string.IsNullOrEmpty(owner) || "NONE".Equals(owner, StringComparison.OrdinalIgnoreCase)) { owner = "Neutral"; }

            string stringText;
            if (!string.IsNullOrEmpty(text)) 
            {
                stringText = "TEXT_STRUCTURE_" + text;
                if (additionalText != null)
                {
                    additionalText[stringText] = text;
                }
            }
            else
            {
                stringText = "TEXT_STRUCTURE_" + textID;
                if (mappedText != null)
                {
                    mappedText[stringText] = textID;
                }
            }

            BuildingTypes.ModifyOrAdd(id, stringText, powerProd, powerUse, storage, capturable, width, height, occupyMask, owner, factoryOverlay, frameOffset, graphicsSource, flags, zorder);
        }

        private void InitModInfantryType(INISection section, Dictionary<string, int> mappedText, Dictionary<string, string> additionalText)
        {
            string id = section.Name;
            string sTextID = section.TryGetValue("TextID");
            string text = section.TryGetValue("Name");
            string owner = section.TryGetValue("Owner");
            string isArmed = section.TryGetValue("IsArmed");
            string noRemap = section.TryGetValue("NoRemap");

            // parse and apply defaults (int)
            int.TryParse(sTextID, out int textID);
            UnitTypeFlag flags = UnitTypeFlag.None;
            if (YesNoBooleanTypeConverter.Parse(isArmed)) { flags |= UnitTypeFlag.IsArmed; }
            if (YesNoBooleanTypeConverter.Parse(noRemap)) { flags |= UnitTypeFlag.NoRemap; }

            // apply defaults (string)
            if (string.IsNullOrEmpty(owner) || "NONE".Equals(owner, StringComparison.OrdinalIgnoreCase)) { owner = "Neutral"; }

            string stringText;
            if (!string.IsNullOrEmpty(text))
            {
                stringText = "TEXT_INFANTRY_" + text;
                if (additionalText != null)
                {
                    additionalText[stringText] = text;
                }
            }
            else
            {
                stringText = "TEXT_INFANTRY_" + textID;
                if (mappedText != null)
                {
                    mappedText[stringText] = textID;
                }
            }

            InfantryTypes.ModifyOrAdd(id, stringText, owner, flags);
        }

        private void InitModVehicleType(INISection section, Dictionary<string, int> mappedText, Dictionary<string, string> additionalText)
        {
            string id = section.Name;
            string sTextID = section.TryGetValue("TextID");
            string text = section.TryGetValue("Name");
            string owner = section.TryGetValue("Owner");
            string sFacings = section.TryGetValue("Facings");
            string sTurretOffset = section.TryGetValue("TurretOffset");
            string sTurretY = section.TryGetValue("TurretY");
            string hasUnloadFrames = section.TryGetValue("HasUnloadFrames");
            string hasTurret = section.TryGetValue("HasTurret");
            string isArmed = section.TryGetValue("IsArmed");
            string noRemap = section.TryGetValue("NoRemap");

            // parse and apply defaults (int)
            int.TryParse(sTextID, out int textID);
            int.TryParse(sTurretOffset, out int turrOffset);
            int.TryParse(sTurretY, out int turrY);
            int.TryParse(sFacings, out int facings);
            UnitTypeFlag flags = UnitTypeFlag.None;
            bool turret = YesNoBooleanTypeConverter.Parse(hasTurret);
            if (turret) { flags |= UnitTypeFlag.HasTurret; }
            if (YesNoBooleanTypeConverter.Parse(isArmed)) { flags |= UnitTypeFlag.IsArmed; }
            if (YesNoBooleanTypeConverter.Parse(noRemap)) { flags |= UnitTypeFlag.NoRemap; }
            FrameUsage bodyFrameUsage = FrameUsage.None;
            FrameUsage turrFrameUsage = FrameUsage.None;
            switch (facings)
            {
                case 32:
                    bodyFrameUsage |= FrameUsage.Frames32Full;
                    if (turret) { turrFrameUsage |= FrameUsage.Frames32Full; }
                    break;
                case 8:
                    bodyFrameUsage |= FrameUsage.Frames08Cardinal;
                    break;
                default:
                    bodyFrameUsage |= FrameUsage.Frames01Single;
                    break;
            }
            if (YesNoBooleanTypeConverter.Parse(hasUnloadFrames)) { bodyFrameUsage |= FrameUsage.HasUnloadFrames; }

            // apply defaults (string)
            if (string.IsNullOrEmpty(owner) || "NONE".Equals(owner, StringComparison.OrdinalIgnoreCase)) { owner = "Neutral"; }

            string stringText;
            if (!string.IsNullOrEmpty(text))
            {
                stringText = "TEXT_VEHICLE_" + text;
                if (additionalText != null)
                {
                    additionalText[stringText] = text;
                }
            }
            else
            {
                stringText = "TEXT_VEHICLE_" + textID;
                if (mappedText != null)
                {
                    mappedText[stringText] = textID;
                }
            }

            UnitTypes.ModifyOrAddVehicle(id, stringText, owner, bodyFrameUsage, turrFrameUsage, turrOffset, turrY, flags);
        }

        private void InitModAircraftType(INISection section, Dictionary<string, int> mappedText, Dictionary<string, string> additionalText)
        {
            string id = section.Name;
            string sTextID = section.TryGetValue("TextID");
            string text = section.TryGetValue("Name");
            string owner = section.TryGetValue("Owner");
            string sTurretOffset = section.TryGetValue("TurretOffset");
            string sTurretY = section.TryGetValue("TurretY");
            string isFixedWing = section.TryGetValue("IsFixedWing");
            string hasUnloadFrames = section.TryGetValue("HasUnloadFrames");
            string hasRotor = section.TryGetValue("HasRotor");
            string hasDoubleRotor = section.TryGetValue("hasDoubleRotor");
            string isArmed = section.TryGetValue("IsArmed");
            string noRemap = section.TryGetValue("NoRemap");

            // parse and apply defaults (int)
            int.TryParse(sTextID, out int textID);
            int.TryParse(sTurretOffset, out int turrOffset);
            int.TryParse(sTurretY, out int turrY);
            UnitTypeFlag flags = UnitTypeFlag.None;
            FrameUsage bodyFrameUsage = FrameUsage.Frames32Full;
            FrameUsage turrFrameUsage = FrameUsage.None;
            string turret = null;
            string turret2 = null;
            if (YesNoBooleanTypeConverter.Parse(hasRotor)) 
            { 
                flags |= UnitTypeFlag.HasTurret;
                turrFrameUsage = FrameUsage.Rotor;
                turret = "LROTOR";
                if (YesNoBooleanTypeConverter.Parse(hasDoubleRotor))
                {
                    flags |= UnitTypeFlag.HasDoubleTurret;
                    turret = "RROTOR";
                }
            }
            if (YesNoBooleanTypeConverter.Parse(isArmed)) { flags |= UnitTypeFlag.IsArmed; }
            if (YesNoBooleanTypeConverter.Parse(noRemap)) { flags |= UnitTypeFlag.NoRemap; }
            if (YesNoBooleanTypeConverter.Parse(hasUnloadFrames)) { bodyFrameUsage |= FrameUsage.HasUnloadFrames; }

            // apply defaults (string)
            if (string.IsNullOrEmpty(owner) || "NONE".Equals(owner, StringComparison.OrdinalIgnoreCase)) { owner = "Neutral"; }

            string stringText;
            if (!string.IsNullOrEmpty(text))
            {
                stringText = "TEXT_AIRCRAFT_" + text;
                if (additionalText != null)
                {
                    additionalText[stringText] = text;
                }
            }
            else
            {
                stringText = "TEXT_AIRCRAFT_" + textID;
                if (mappedText != null)
                {
                    mappedText[stringText] = textID;
                }
            }

            UnitTypes.ModifyOrAddAircraft(id, stringText, owner, bodyFrameUsage, turrFrameUsage, turret, turret2, turrOffset, turrY, flags);
        }

        private void InitModTerrainType(INISection section, Dictionary<string, int> mappedText, Dictionary<string, string> additionalText)
        {
            string id = section.Name;
            string sTextID = section.TryGetValue("TextID");
            string text = section.TryGetValue("Name");
            string sCenterX = section.TryGetValue("CenterX");
            string sCenterY = section.TryGetValue("CenterY");
            string sWidth = section.TryGetValue("Width");
            string sHeight = section.TryGetValue("Height");
            string occupyMask = section.TryGetValue("OccupyMask");

            // parse and apply defaults (int)
            int.TryParse(sTextID, out int textID);
            if (!int.TryParse(sWidth, out int width)) { width = 1; }
            if (!int.TryParse(sHeight, out int height)) { height = 1; }
            if (!int.TryParse(sCenterX, out int centerX)) { centerX = 12; }
            if (!int.TryParse(sCenterY, out int centerY)) { centerY = 12; }

            // apply defaults (string)
            if (string.IsNullOrEmpty(occupyMask) || "NONE".Equals(occupyMask, StringComparison.OrdinalIgnoreCase)) { occupyMask = null; }

            string stringText;
            if (!string.IsNullOrEmpty(text))
            {
                stringText = "TEXT_TERRAIN_" + text;
                if (additionalText != null)
                {
                    additionalText[stringText] = text;
                }
            }
            else
            {
                stringText = "TEXT_TERRAIN_" + textID;
                if (mappedText != null)
                {
                    mappedText[stringText] = textID;
                }
            }

            TerrainTypes.ModifyOrAdd(id, stringText, width, height, centerX, centerY, occupyMask);
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
