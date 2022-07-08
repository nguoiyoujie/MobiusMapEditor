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
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class SmudgeProperties : UserControl
    {
        private bool isMockObject;

        public IGamePlugin Plugin { get; private set; }

        private Smudge smudge;
        public Smudge Smudge
        {
            get => smudge;
            set
            {
                if (smudge == value)
                    return;                
                if (smudge != null)
                    smudge.PropertyChanged -= Obj_PropertyChanged;
                smudge = value;
                if (smudge != null)
                    smudge.PropertyChanged += Obj_PropertyChanged;
                Rebind();
            }
        }

        public SmudgeProperties()
        {
            InitializeComponent();
        }

        public void Initialize(IGamePlugin plugin, bool isMockObject)
        {
            this.isMockObject = isMockObject;

            Plugin = plugin;

            UpdateDataSource();

            Disposed += (sender, e) =>
            {
                Smudge = null;
            };
        }

        private void UpdateDataSource()
        {
            int[] data;

            if (smudge != null && smudge.Type.Icons > 1)
                data = Enumerable.Range(0, smudge.Type.Icons).ToArray();
            else
                data = new int[] { 0 };
            stateComboBox.DataSource = data;
        }

        private void Rebind()
        {
            stateComboBox.DataBindings.Clear();

            if (smudge == null)
            {
                return;
            }
            UpdateDataSource();
            stateComboBox.DataBindings.Add("SelectedItem", smudge, "Icon");
            stateComboBox.Enabled = stateComboBox.Items.Count > 1;
        }

        private void Obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Type":
                    {
                        Rebind();
                    }
                    break;
            }
            if (!isMockObject)
            {
                Plugin.Dirty = true;
            }
        }

        private void comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            foreach (Binding binding in (sender as ComboBox).DataBindings)
            {
                binding.WriteValue();
            }
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            foreach (Binding binding in (sender as NumericUpDown).DataBindings)
            {
                binding.WriteValue();
            }
        }
    }

    public class SmudgePropertiesPopup : ToolStripDropDown
    {
        private readonly ToolStripControlHost host;

        public SmudgeProperties SmudgeProperties { get; private set; }

        public SmudgePropertiesPopup(IGamePlugin plugin, Smudge smudge)
        {
            SmudgeProperties = new SmudgeProperties();
            SmudgeProperties.Smudge = smudge;
            SmudgeProperties.Initialize(plugin, false);

            host = new ToolStripControlHost(SmudgeProperties);
            Padding = Margin = host.Padding = host.Margin = Padding.Empty;
            MinimumSize = SmudgeProperties.MinimumSize;
            SmudgeProperties.MinimumSize = SmudgeProperties.Size;
            MaximumSize = SmudgeProperties.MaximumSize;
            SmudgeProperties.MaximumSize = SmudgeProperties.Size;
            Size = SmudgeProperties.Size;
            Items.Add(host);
            SmudgeProperties.Disposed += (sender, e) =>
            {
                SmudgeProperties = null;
                Dispose(true);
            };
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            base.OnClosed(e);

            SmudgeProperties.Smudge = null;
        }
    }
}
