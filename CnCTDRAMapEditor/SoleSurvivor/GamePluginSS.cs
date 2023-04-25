﻿using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.TiberianDawn;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace MobiusEditor.SoleSurvivor
{
    public class GamePluginSS : GamePluginTD
    {

        protected const int cratePoints = 4;
        protected const int teamStartPoints = 8;

        protected static readonly IEnumerable<string> movieTypesSole = new string[]
        {
            "WESTLOGO",
        };
        
        protected static readonly IEnumerable<string> themeTypesSole = new string[]
        {
            "No Theme",
            "WORKREMX",
            "CRSHNVOX",
            "DEPTHCHG",
            "DRILL",
            "HELLNVOX",
            "IRONFIST",
            "MERCY98",
            "MUDREMX",
            "CREEPING",
            "MAP1",
        };

        public override string Name => "Sole Survivor";
        public override GameType GameType => GameType.SoleSurvivor;
        public override bool IsMegaMap => true;

        public static bool CheckForSSmap(INI iniContents)
        {
            return INITools.CheckForIniInfo(iniContents, "Crates");
        }

        protected CratesSection cratesSection;
        public CratesSection CratesSection => cratesSection;

        public GamePluginSS(bool megaMap)
            : this(true, megaMap)
        {
        }

        public GamePluginSS(bool mapImage, bool megaMap)
            : base()
        {
            this.isMegaMap = megaMap;
            IEnumerable<Waypoint> crateWaypoints = Enumerable.Range(0, cratePoints).Select(i => new Waypoint(string.Format("CR{0}", i), WaypointFlag.CrateSpawn));
            IEnumerable<Waypoint> teamWaypoints = Enumerable.Range(cratePoints, teamStartPoints).Select(i => new Waypoint(string.Format("TM{0}", i - cratePoints), Waypoint.GetFlagForMpId(i - cratePoints)));
            IEnumerable<Waypoint> generalWaypoints = Enumerable.Range(cratePoints + teamStartPoints, 25 - cratePoints - teamStartPoints).Select(i => new Waypoint(i.ToString()));
            Waypoint[] specialWaypoints = new Waypoint[] { new Waypoint("Flare", WaypointFlag.Flare), new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", WaypointFlag.Reinforce) };
            Waypoint[] waypoints = crateWaypoints.Concat(teamWaypoints).Concat(generalWaypoints).Concat(specialWaypoints).ToArray();
            TiberianDawn.BasicSection basicSection = new TiberianDawn.BasicSection();
            basicSection.SetDefault();
            List<HouseType> houseTypes = HouseTypes.GetTypes().ToList();
            basicSection.Player = HouseTypes.Admin.Name;
            // Irrelevant for Sole. Rebuilding options will be disabled in the editor.
            basicSection.BasePlayer = HouseTypes.GetBasePlayer(basicSection.Player);
            // Specific to Sole.
            cratesSection = new CratesSection();
            cratesSection.SetDefault();
            // I guess we leave these to the TD defaults.
            string[] cellEventTypes = new[]
            {
                EventTypes.EVENT_PLAYER_ENTERED,
                EventTypes.EVENT_NONE
            };
            string[] unitEventTypes =
            {
                EventTypes.EVENT_DISCOVERED,
                EventTypes.EVENT_ATTACKED,
                EventTypes.EVENT_DESTROYED,
                EventTypes.EVENT_ANY,
                EventTypes.EVENT_NONE
            };
            string[] structureEventTypes = (new[] { EventTypes.EVENT_PLAYER_ENTERED }).Concat(unitEventTypes).ToArray();
            string[] terrainEventTypes =
            {
                EventTypes.EVENT_ATTACKED,
                EventTypes.EVENT_ANY,
                EventTypes.EVENT_NONE
            };
            string[] cellActionTypes = { };
            string[] unitActionTypes = { };
            string[] structureActionTypes = { };
            string[] terrainActionTypes = { };
            BuildingType[] buildings = Globals.NoOwnedObjectsInSole ? new BuildingType[0] : BuildingTypes.GetTypes().ToArray();
            UnitType[] units = Globals.NoOwnedObjectsInSole ? new UnitType[0] : UnitTypes.GetTypes(Globals.DisableAirUnits).ToArray();
            InfantryType[] infantry = Globals.NoOwnedObjectsInSole ? new InfantryType[0] : InfantryTypes.GetTypes().ToArray();
            foreach (BuildingType bld in buildings)
            {
                // Power is irrelevant in SS.
                bld.PowerUsage = 0;
                bld.PowerProduction = 0;
            }
            ITeamColor[] flagColors = new ITeamColor[8];
            foreach (HouseType house in houseTypes)
            {
                int mpId = Waypoint.GetMpIdFromFlag(house.MultiplayIdentifier);
                if (mpId == -1)
                {
                    continue;
                }
                flagColors[mpId] = Globals.TheTeamColorManager[house.UnitTeamColor];
            }
            // Multi7: the dark blue that's unused in SS because Multi4 uses BAD_UNITS instead.
            flagColors[6] = Globals.TheTeamColorManager["MULTI2"];
            // Multi8: RA Purple
            flagColors[7] = Globals.TheTeamColorManager["PURPLE"];
            List<string> movies = movieTypesTD.Concat(movieTypesSole).ToList();
            ExplorerComparer sorter = new ExplorerComparer();
            movies.Sort(sorter);
            Size mapSize = !megaMap ? Constants.MaxSize : Constants.MaxSizeMega;
            Map = new Map(basicSection, null, mapSize, typeof(TiberianDawn.House), houseTypes,
                flagColors, TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(Globals.ConvertCraters),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), MissionTypes.MISSION_GUARD, MissionTypes.MISSION_STOP,
                MissionTypes.MISSION_HARVEST, MissionTypes.MISSION_UNLOAD, DirectionTypes.GetMainTypes(),
                DirectionTypes.GetAllTypes(), infantry, units, buildings, TeamMissionTypes.GetTypes(), fullTechnoTypes,
                waypoints, 4, 0, 0, movies, movieEmpty, themeTypesSole, themeEmpty, tiberiumValue, 0);
            Map.MapSection.PropertyChanged += MapSection_PropertyChanged;
            // Clean up this mess.
            foreach (Model.House house in Map.Houses)
            {
                if (house.Type.ID >= HouseTypes.Multi1.ID)
                {
                    house.Enabled = false;
                }
            }
            if (mapImage)
            {
                Bitmap mapImg = new Bitmap(Map.Metrics.Width * Globals.MapTileWidth, Map.Metrics.Height * Globals.MapTileHeight);
                mapImg.SetResolution(96, 96);
                MapImage = mapImg;
            }
        }

        public override IEnumerable<string> Load(string path, FileType fileType)
        {
            return Load(path, fileType, true);
        }

        protected override List<string> LoadINI(INI ini, bool forceSoloMission, ref bool modified)
        {
            List<string> errors = LoadINI(ini, forceSoloMission, true, ref modified);
            INISection cratesIniSection = extraSections.Extract("Crates");
            if (cratesIniSection != null)
            {
                try
                {
                    INI.ParseSection(new MapContext(Map, false), cratesIniSection, this.cratesSection);
                }
                catch (Exception ex)
                {
                    errors.Add("Parsing of [Crates] section failed: " + ex.Message);
                }
            }
            return errors;
        }

        public override void New(string theater)
        {
            // Nothing changed here.
            base.New(theater);
        }

        public override bool Save(string path, FileType fileType)
        {
            return Save(path, fileType, null, false);
        }

        public override bool Save(string path, FileType fileType, Bitmap customPreview, bool dontResavePreview)
        {
            return Save(path, fileType, true, customPreview, dontResavePreview);
        }

        protected override void SaveINI(INI ini, FileType fileType, string fileName)
        {
            INISection waypointBackup = null;
            INISection overlayBackup = null;
            if (extraSections != null)
            {
                // Commonly found in official maps as backups of beta versions of the map.
                // If found, place them under their respective original sections.
                waypointBackup = extraSections.Extract("OldWaypoints");
                overlayBackup = extraSections.Extract("OldOverlay");
                ini.Sections.AddRange(extraSections);
            }
            INISection basicSection = SaveIniBasic(ini, fileName);
            // Not used for SS; not Remaster, and no single play in it.
            basicSection.Keys.Remove("SoloMission");
            SaveIniMap(ini);
            INISection cratesIniSection = ini.Sections.Add("Crates");
            SaveIniWaypoints(ini);
            if (waypointBackup != null)
            {
                ini.Sections.Add(waypointBackup);
            }
            INI.WriteSection(new MapContext(Map, false), cratesIniSection, this.cratesSection);
            SaveIniCellTriggers(ini, true);
            SaveIniTeamTypes(ini, true);
            SaveIniTriggers(ini, true);
            SaveIniHouses(ini);
            //SaveIniBriefing(ini);
            if (!Globals.NoOwnedObjectsInSole)
            {
                SaveIniBase(ini, true);
                SaveIniInfantry(ini);
                SaveIniUnits(ini);
                SaveIniStructures(ini);
                SaveIniAircraft(ini);
            }
            SaveINITerrain(ini);
            SaveIniOverlay(ini);
            if (overlayBackup != null)
            {
                ini.Sections.Add(overlayBackup);
            }
            SaveIniSmudge(ini);
        }

        public override string Validate()
        {
            return Validate(true);
        }

        public override HashSet<string> GetHousesWithProduction()
        {
            // Not applicable. Return empty set.
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public override IEnumerable<string> AssessMapItems()
        {
            ExplorerComparer cmp = new ExplorerComparer();
            List<string> info = new List<string>();
            int numAircraft = Globals.DisableAirUnits ? 0 : Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsGroundUnit).Count();
            const string maximums = "Number of {0}: {1}. Maximum: {2}.";
            if (!Globals.NoOwnedObjectsInSole)
            {
                if (!Globals.DisableAirUnits)
                {
                    info.Add(string.Format(maximums, "aircraft", numAircraft, Globals.RestrictSoleLimits ? Constants.MaxAircraftClassic : Constants.MaxAircraft));
                }
                info.Add(string.Format(maximums, "structures", numBuildings, Globals.RestrictSoleLimits ? Constants.MaxBuildingsClassic : Constants.MaxBuildings));
                info.Add(string.Format(maximums, "infantry", numInfantry, Globals.RestrictSoleLimits ? Constants.MaxInfantryClassic : Constants.MaxInfantry));
            }
            info.Add(string.Format(maximums, "terrain objects", numTerrain, Globals.RestrictSoleLimits ? Constants.MaxTerrainClassic : Constants.MaxTerrain));
            if (!Globals.NoOwnedObjectsInSole)
            {
                info.Add(string.Format(maximums, "units", numUnits, Globals.RestrictSoleLimits ? Constants.MaxUnitsClassic : Constants.MaxUnits));
            }
            //info.Add(string.Format(maximums, "team types", Map.TeamTypes.Count, Globals.ExpandSoleLimits ? Constants.MaxTeams : Constants.MaxTeamsClassic));
            //info.Add(string.Format(maximums, "triggers", Map.Triggers.Count, Globals.ExpandSoleLimits ? Constants.MaxTriggers : Constants.MaxTriggersClassic));
            int startPoints = Map.Waypoints.Count(w => w.Cell.HasValue && (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart);
            info.Add(string.Format("Number of set starting points: {0}.", startPoints));
            return info;
        }

        public override Boolean EvaluateBriefing(string briefing, out string message)
        {
            message = null;
            return true;
        }
    }
}
