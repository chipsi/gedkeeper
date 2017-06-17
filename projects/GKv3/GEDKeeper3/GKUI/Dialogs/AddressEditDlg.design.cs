﻿using System;
using Eto.Drawing;
using Eto.Forms;
using GKUI.Components;

namespace GKUI.Dialogs
{
    partial class AddressEditDlg
    {
        private Button btnAccept;
        private Button btnCancel;
        private TabControl tabsAddrData;
        private TabPage pagePhones;
        private TabPage pageEmails;
        private TabPage pageCommon;
        private TabPage pageWebPages;
        private Label lblCountry;
        private Label lblState;
        private Label lblCity;
        private Label lblPostalCode;
        private Label lblAddress;
        private TextBox txtCountry;
        private TextBox txtState;
        private TextBox txtCity;
        private TextBox txtPostalCode;
        private TextBox txtAddress;

        private void InitializeComponent()
        {
            SuspendLayout();

            lblCountry = new Label();
            lblCountry.Text = "lblCountry";

            lblState = new Label();
            lblState.Text = "lblState";

            lblCity = new Label();
            lblCity.Text = "lblCity";

            lblPostalCode = new Label();
            lblPostalCode.Text = "lblPostalCode";

            lblAddress = new Label();
            lblAddress.Text = "lblAddress";

            txtCountry = new TextBox();
            txtCountry.Width = 280;

            txtState = new TextBox();

            txtCity = new TextBox();

            txtPostalCode = new TextBox();

            txtAddress = new TextBox();

            pageCommon = new TabPage();
            pageCommon.Text = "pageCommon";
            pageCommon.Content = new DefTableLayout {
                Rows = {
                    new TableRow {
                        Cells = { lblCountry, txtCountry }
                    },
                    new TableRow {
                        Cells = { lblState, txtState }
                    },
                    new TableRow {
                        Cells = { lblCity, txtCity }
                    },
                    new TableRow {
                        Cells = { lblPostalCode, txtPostalCode }
                    },
                    new TableRow {
                        Cells = { lblAddress, txtAddress }
                    }
                }
            };

            //

            pagePhones = new TabPage();
            pagePhones.Text = "pagePhones";

            pageEmails = new TabPage();
            pageEmails.Text = "pageEmails";

            pageWebPages = new TabPage();
            pageWebPages.Text = "pageWebPages";

            tabsAddrData = new TabControl();
            tabsAddrData.Pages.Add(pageCommon);
            tabsAddrData.Pages.Add(pagePhones);
            tabsAddrData.Pages.Add(pageEmails);
            tabsAddrData.Pages.Add(pageWebPages);

            //

            btnAccept = new Button();
            btnAccept.ImagePosition = ButtonImagePosition.Left;
            btnAccept.Size = new Size(130, 26);
            btnAccept.Text = "btnAccept";
            btnAccept.Click += btnAccept_Click;
            btnAccept.Image = Bitmap.FromResource("Resources.btn_accept.gif");

            btnCancel = new Button();
            btnCancel.ImagePosition = ButtonImagePosition.Left;
            btnCancel.Size = new Size(130, 26);
            btnCancel.Text = "btnCancel";
            btnCancel.Click += CancelClickHandler;
            btnCancel.Image = Bitmap.FromResource("Resources.btn_cancel.gif");

            Content = new DefTableLayout {
                Rows = {
                    new TableRow {
                        ScaleHeight = true,
                        Cells = { tabsAddrData }
                    },
                    UIHelper.MakeDialogFooter(null, btnAccept, btnCancel)
                }
            };

            DefaultButton = btnAccept;
            AbortButton = btnCancel;
            //ClientSize = new Size(572, 385);
            Title = "AddressEditDlg";

            UIHelper.SetControlFont(this, "Tahoma", 8.25f);
            ResumeLayout();
        }
    }
}
