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
using System;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class NewMapDialog : Form
    {
        private GameType gameType = GameType.TiberianDawn;
        public GameType GameType
        {
            get => gameType;
            set
            {
                if (gameType != value)
                {
                    gameType = value;
                    switch (value)
                    {
                        case GameType.TiberianDawn:
                            radioTD.Checked = true;
                            chkMegamap.Checked = false;
                            break;
                        case GameType.RedAlert:
                            radioRA.Checked = true;
                            break;
                        case GameType.SoleSurvivor:
                            radioSS.Checked = true;
                            chkMegamap.Checked = true;
                            break;
                    }
                    UpdateGameType();
                }
            }
        }

        public bool MegaMap
        {
            get
            {
                return chkMegamap.Checked;
            }
        }

        public string TheaterName
        {
            get
            {
                if (radioTheater1.Checked) return radioTheater1.Text;
                if (radioTheater2.Checked) return radioTheater2.Text;
                if (radioTheater3.Checked) return radioTheater3.Text;
                return null;
            }
        }

        public NewMapDialog(bool fromImage)
        {
            InitializeComponent();
            if (fromImage)
            {
                this.Text = "New Map From Image";
            }
        }

        private void UpdateGameType()
        {
            switch(GameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    {
                        radioTheater1.Text = "Desert";
                        radioTheater2.Text = "Temperate";
                        radioTheater3.Text = "Winter";
                    } break;
                case GameType.RedAlert:
                    {
                        radioTheater1.Text = "Temperate";
                        radioTheater2.Text = "Snow";
                        radioTheater3.Text = "Interior";
                    }
                    break;
            }
            chkMegamap.Visible = GameType == GameType.TiberianDawn || GameType == GameType.SoleSurvivor;
            lblWarning.Visible = (chkMegamap.Visible && chkMegamap.Checked) || GameType == GameType.SoleSurvivor;
        }

        private void radioGameType_CheckedChanged(object sender, EventArgs e)
        {
            if (radioTD.Checked)
            {
                GameType = GameType.TiberianDawn;
            }
            else if (radioRA.Checked)
            {
                GameType = GameType.RedAlert;
            }
            else if (radioSS.Checked)
            {
                GameType = GameType.SoleSurvivor;
            }
        }

        private void chkMegamap_CheckedChanged(Object sender, EventArgs e)
        {
            UpdateGameType();
        }
    }
}
