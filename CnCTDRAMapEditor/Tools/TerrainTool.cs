﻿//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed 
// in the hope that it will be useful, but with permitted additional restrictions 
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT 
// distributed with this program. You should have received a copy of the 
// GNU General Public License along with permitted additional restrictions 
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using MobiusEditor.Widgets;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class TerrainTool : ViewTool
    {
        /// <summary> Layers that are important to this tool and need to be drawn last in the PostRenderMap process.</summary>
        protected override MapLayerFlag PriorityLayers => MapLayerFlag.None;
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.TechnoTriggers;

        private readonly TypeListBox terrainTypeListBox;
        private readonly MapPanel terrainTypeMapPanel;
        private readonly TerrainProperties terrainProperties;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        private bool placementMode;

        private readonly Terrain mockTerrain;

        private Terrain selectedTerrain;
        private Point? selectedTerrainLocation;
        private Point selectedTerrainPivot;

        private TerrainType selectedTerrainType;
        private TerrainPropertiesPopup selectedTerrainProperties;
        private TerrainType SelectedTerrainType
        {
            get => selectedTerrainType;
            set
            {
                if (selectedTerrainType != value)
                {
                    if (placementMode && (selectedTerrainType != null))
                    {
                        mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedTerrainType.OverlapBounds.Size));
                    }
                    selectedTerrainType = value;
                    terrainTypeListBox.SelectedValue = selectedTerrainType;
                    if (placementMode && (selectedTerrainType != null))
                    {
                        mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedTerrainType.OverlapBounds.Size));
                    }
                    mockTerrain.Type = selectedTerrainType;
                    mockTerrain.Icon = selectedTerrainType.DisplayIcon;
                    RefreshMapPanel();
                }
            }
        }

        public TerrainTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox terrainTypeComboBox, MapPanel terrainTypeMapPanel, TerrainProperties terrainProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            mockTerrain = new Terrain();
            mockTerrain.PropertyChanged += MockTerrain_PropertyChanged;
            this.terrainTypeListBox = terrainTypeComboBox;
            this.terrainTypeListBox.SelectedIndexChanged += TerrainTypeCombo_SelectedIndexChanged;
            this.terrainTypeMapPanel = terrainTypeMapPanel;
            this.terrainTypeMapPanel.BackColor = Color.White;
            this.terrainTypeMapPanel.MaxZoom = 1;
            this.terrainTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.terrainProperties = terrainProperties;
            this.terrainProperties.Terrain = mockTerrain;
            this.terrainProperties.Visible = plugin.Map.TerrainEventTypes.Count > 0;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            SelectedTerrainType = terrainTypeComboBox.Types.First() as TerrainType;
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
        }

        private void MapPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.None)
            {
                return;
            }
            if (map.Metrics.GetCell(navigationWidget.MouseCell, out int cell))
            {
                if (map.Technos[cell] is Terrain terrain)
                {
                    selectedTerrain = null;
                    selectedTerrainProperties?.Close();
                    // only TD supports triggers ("Attacked" type) on terrain types.
                    if (plugin.GameType == GameType.TiberianDawn)
                    {
                        Terrain preEdit = terrain.Clone();
                        selectedTerrainProperties = new TerrainPropertiesPopup(terrainProperties.Plugin, terrain);
                        selectedTerrainProperties.Closed += (cs, ce) =>
                        {
                            navigationWidget.Refresh();
                            AddUndoRedo(terrain, preEdit);
                        };
                        selectedTerrainProperties.Show(mapPanel, mapPanel.PointToClient(Control.MousePosition));
                    }
                    UpdateStatus();
                }
            }
        }

        private void AddUndoRedo(Terrain terrain, Terrain preEdit)
        {
            // terrain = terrain in its final edited form. Clone for preservation
            Terrain redoUnit = terrain.Clone();
            Terrain undoUnit = preEdit;
            void undoAction(UndoRedoEventArgs ev)
            {
                terrain.CloneDataFrom(undoUnit);
                if (terrain.Trigger == null || (!Trigger.None.Equals(terrain.Trigger, StringComparison.InvariantCultureIgnoreCase)
                    && !ev.Map.FilterTerrainTriggers().Any(tr => tr.Name.Equals(terrain.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    terrain.Trigger = Trigger.None;
                }
                ev.MapPanel.Invalidate(ev.Map, terrain);
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                terrain.CloneDataFrom(redoUnit);
                if (terrain.Trigger == null || (!Trigger.None.Equals(terrain.Trigger, StringComparison.InvariantCultureIgnoreCase)
                    && !ev.Map.FilterTerrainTriggers().Any(tr => tr.Name.Equals(terrain.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    terrain.Trigger = Trigger.None;
                }
                ev.MapPanel.Invalidate(ev.Map, terrain);
            }
            url.Track(undoAction, redoAction);
        }

        private void MockTerrain_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshMapPanel();
        }

        private void TerrainTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedTerrainType = terrainTypeListBox.SelectedValue as TerrainType;
        }

        private void TerrainTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                EnterPlacementMode();
            }
        }

        private void TerrainTool_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                ExitPlacementMode();
            }
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (placementMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    AddTerrain(navigationWidget.MouseCell);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveTerrain(navigationWidget.MouseCell);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                SelectTerrain(navigationWidget.MouseCell);
            }
            else if (e.Button == MouseButtons.Right)
            {
                PickTerrain(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedTerrain != null && selectedTerrainLocation.HasValue)
            {
                AddMoveUndoTracking(selectedTerrain, selectedTerrainLocation.Value);
                selectedTerrain = null;
                selectedTerrainLocation = null;
                selectedTerrainPivot = Point.Empty;
                UpdateStatus();
            }
        }

        private void AddMoveUndoTracking(Terrain toMove, Point startLocation)
        {
            Point? finalLocation = map.Technos[toMove];
            if (finalLocation.HasValue && finalLocation.Value != startLocation)
            {
                Point endLocation = finalLocation.Value;
                void undoAction(UndoRedoEventArgs ev)
                {
                    ev.MapPanel.Invalidate(ev.Map, toMove);
                    ev.Map.Technos.Remove(toMove);
                    ev.Map.Technos.Add(startLocation, toMove);
                    ev.MapPanel.Invalidate(ev.Map, toMove);
                }
                void redoAction(UndoRedoEventArgs ev)
                {
                    ev.MapPanel.Invalidate(ev.Map, toMove);
                    ev.Map.Technos.Remove(toMove);
                    ev.Map.Technos.Add(endLocation, toMove);
                    ev.MapPanel.Invalidate(ev.Map, toMove);
                }
                url.Track(undoAction, redoAction);
            }
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!placementMode && (Control.ModifierKeys == Keys.Shift))
            {
                EnterPlacementMode();
            }
            else if (placementMode && (Control.ModifierKeys == Keys.None))
            {
                ExitPlacementMode();
            }
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (placementMode)
            {
                if (SelectedTerrainType != null)
                {
                    mapPanel.Invalidate(map, new Rectangle(e.OldCell, SelectedTerrainType.OverlapBounds.Size));
                    mapPanel.Invalidate(map, new Rectangle(e.NewCell, SelectedTerrainType.OverlapBounds.Size));
                }
            }
            else if (selectedTerrain != null)
            {
                Terrain toMove = selectedTerrain;
                var oldLocation = map.Technos[toMove].Value;
                var newLocation = new Point(Math.Max(0, e.NewCell.X - selectedTerrainPivot.X), Math.Max(0, e.NewCell.Y - selectedTerrainPivot.Y));
                mapPanel.Invalidate(map, toMove);
                map.Technos.Remove(toMove);
                if (map.Technos.Add(newLocation, toMove))
                {
                    mapPanel.Invalidate(map, toMove);
                    plugin.Dirty = true;
                }
                else
                {
                    map.Technos.Add(oldLocation, toMove);
                }
            }
        }

        private void AddTerrain(Point location)
        {
            if (!map.Metrics.Contains(location))
            {
                return;
            }
            if (SelectedTerrainType != null)
            {
                var terrain = mockTerrain.Clone();
                if (map.Technos.Add(location, terrain))
                {
                    mapPanel.Invalidate(map, terrain);
                    void undoAction(UndoRedoEventArgs e)
                    {
                        e.MapPanel.Invalidate(e.Map, location);
                        e.Map.Technos.Remove(terrain);
                    }
                    void redoAction(UndoRedoEventArgs e)
                    {
                        e.Map.Technos.Add(location, terrain);
                        e.MapPanel.Invalidate(e.Map, location);
                    }
                    url.Track(undoAction, redoAction);
                    plugin.Dirty = true;
                }
            }
        }

        private void RemoveTerrain(Point location)
        {
            if (map.Technos[location] is Terrain terrain)
            {
                mapPanel.Invalidate(map, terrain);
                map.Technos.Remove(location);
                void undoAction(UndoRedoEventArgs e)
                {
                    e.Map.Technos.Add(location, terrain);
                    e.MapPanel.Invalidate(e.Map, terrain);
                }
                void redoAction(UndoRedoEventArgs e)
                {
                    e.MapPanel.Invalidate(e.Map, terrain);
                    e.Map.Technos.Remove(terrain);
                }
                url.Track(undoAction, redoAction);
                plugin.Dirty = true;
            }
        }

        private void EnterPlacementMode()
        {
            if (placementMode)
            {
                return;
            }
            placementMode = true;
            navigationWidget.MouseoverSize = Size.Empty;
            if (SelectedTerrainType != null)
            {
                mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedTerrainType.OverlapBounds.Size));
            }
            UpdateStatus();
        }

        private void ExitPlacementMode()
        {
            if (!placementMode)
            {
                return;
            }
            placementMode = false;
            navigationWidget.MouseoverSize = new Size(1, 1);
            if (SelectedTerrainType != null)
            {
                mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedTerrainType.OverlapBounds.Size));
            }
            UpdateStatus();
        }

        private void PickTerrain(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                if (map.Technos[cell] is Terrain terrain)
                {
                    SelectedTerrainType = terrain.Type;
                    mockTerrain.Trigger = terrain.Trigger;
                }
            }
        }

        private void SelectTerrain(Point location)
        {
            selectedTerrain = null;
            selectedTerrainLocation = null;
            selectedTerrainPivot = Point.Empty;
            if (map.Metrics.GetCell(location, out int cell))
            {
                Terrain selected = map.Technos[cell] as Terrain;
                Point selectedLocation = selected != null ? map.Technos[selected].Value : Point.Empty;
                Point selectedPivot = selected != null ? location - (Size)selectedLocation : Point.Empty;
                selectedTerrain = selected;
                selectedTerrainLocation = selectedLocation;
                selectedTerrainPivot = selectedPivot;
            }
            UpdateStatus();
        }

        private void RefreshMapPanel()
        {
            terrainTypeMapPanel.MapImage = mockTerrain.Type.Thumbnail;
        }

        private void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to place terrain, Right-Click to remove terrain";
            }
            else if (selectedTerrain != null)
            {
                statusLbl.Text = "Drag mouse to move terrain";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click drag to move terrain, "
                    + (plugin.GameType == GameType.TiberianDawn ? "Double-Click to update terrain properties, " : String.Empty)
                    + "Right-Click to pick terrain";
            }
        }

        protected override void PreRenderMap()
        {
            base.PreRenderMap();
            previewMap = map.Clone();
            if (!placementMode)
            {
                return;
            }
            if (SelectedTerrainType == null)
            {
                return;
            }
            var location = navigationWidget.MouseCell;
            if (previewMap.Metrics.Contains(location))
            {
                var terrain = new Terrain
                {
                    Type = SelectedTerrainType,
                    Icon = SelectedTerrainType.DisplayIcon,
                    Tint = Color.FromArgb(128, Color.White)
                };
                previewMap.Technos.Add(location, terrain);
            }
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            float boundsPenSize = Math.Max(1, Globals.MapTileSize.Width / 16.0f);
            float occupyPenSize = Math.Max(0.5f, Globals.MapTileSize.Width / 32.0f);
            if (occupyPenSize == boundsPenSize)
            {
                boundsPenSize++;
            }
            using (var boundsPen = new Pen(Color.Green, boundsPenSize))
            using (var occupyPen = new Pen(Color.Red, occupyPenSize))
            {
                foreach (var (topLeft, terrain) in previewMap.Technos.OfType<Terrain>())
                {
                    var bounds = new Rectangle(new Point(topLeft.X * Globals.MapTileWidth, topLeft.Y * Globals.MapTileHeight), terrain.Type.GetRenderSize(Globals.MapTileSize));
                    graphics.DrawRectangle(boundsPen, bounds);
                }
                foreach (var (topLeft, terrain) in previewMap.Technos.OfType<Terrain>())
                {
                    for (var y = 0; y < terrain.Type.OccupyMask.GetLength(0); ++y)
                    {
                        for (var x = 0; x < terrain.Type.OccupyMask.GetLength(1); ++x)
                        {
                            if (!terrain.Type.OccupyMask[y, x])
                                continue;
                            var occupyBounds = new Rectangle(
                                new Point((topLeft.X + x) * Globals.MapTileWidth, (topLeft.Y + y) * Globals.MapTileHeight),
                                Globals.MapTileSize
                            );
                            graphics.DrawRectangle(occupyPen, occupyBounds);
                        }
                    }
                }
            }
            RenderTechnoTriggers(graphics, map, Globals.MapTileSize, Globals.MapTileScale, Layers);
        }

        public override void Activate()
        {
            base.Activate();
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += TerrainTool_KeyDown;
            (this.mapPanel as Control).KeyUp += TerrainTool_KeyUp;
            UpdateStatus();
        }

        public override void Deactivate()
        {
            ExitPlacementMode();
            base.Deactivate();
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= TerrainTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= TerrainTool_KeyUp;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Deactivate();
                    selectedTerrainProperties?.Close();
                    terrainTypeListBox.SelectedIndexChanged -= TerrainTypeCombo_SelectedIndexChanged;
                    navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
