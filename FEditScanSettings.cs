using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NAPS.wia;
using NAPS.twain;

namespace NAPS
{
    public partial class FEditScanSettings : Form
    {
        private CScanSettings scanSettigs;

        private string currentDeviceID;
        private bool result = false;

        public bool Result
        {
            get { return result; }
        }

        public CScanSettings ScanSettings
        {
            get { return scanSettigs; }
            set { scanSettigs = value; }
        }

        public FEditScanSettings()
        {
            InitializeComponent();

            cmbPage.Items.AddRange(CPageSizes.GetPageSizeList());
        }

        private void chooseWIA()
        {
            string devID;
            try
            {
                devID = CWIAAPI.SelectDeviceUI();
            }
            catch (Exceptions.ENoScannerFound)
            {
                MessageBox.Show("No device found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            CWIAAPI api = new CWIAAPI(devID);
            txtDevice.Text = api.DeviceName;
            currentDeviceID = devID;

        }

        private void chooseTWAIN()
        {
            Twain tw = new Twain();
            if (!tw.Init(this.Handle))
            {
                MessageBox.Show("No device found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            tw.Select();
            txtDevice.Text = tw.GetCurrentName();
            currentDeviceID = tw.GetCurrentName();
        }

        private void btnChooseDevice_Click(object sender, EventArgs e)
        {
            if (rdWIA.Checked == true)
                chooseWIA();
            else
                chooseTWAIN();
        }

        private void saveSettings()
        {
            scanSettigs.DeviceID = currentDeviceID;
            scanSettigs.Source = (CScanSettings.ScanSource)cmbSource.SelectedIndex;
            scanSettigs.ShowScanUI = rdbNativeWIA.Checked;
            scanSettigs.Depth = (CScanSettings.BitDepth)cmbDepth.SelectedIndex;
            scanSettigs.Resolution = (CScanSettings.DPI)cmbResolution.SelectedIndex;
            scanSettigs.Brightnes = trBrightnes.Value;
            scanSettigs.Contrast = trContrast.Value;
            scanSettigs.DisplayName = txtName.Text;
            scanSettigs.PageSize = (CPageSizes.PageSize)cmbPage.SelectedIndex;
            scanSettigs.AfterScanScale = (CScanSettings.Scale)cmbScale.SelectedIndex;
            scanSettigs.PageAlign = (CScanSettings.HorizontalAlign)cmbAlign.SelectedIndex;
            scanSettigs.DeviceDriver = rdWIA.Checked ? CScanSettings.Driver.WIA : CScanSettings.Driver.TWAIN;

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (currentDeviceID == null || currentDeviceID == "")
            {
                MessageBox.Show("No device selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtName.Text == "")
            {
                MessageBox.Show("Name missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            result = true;
            saveSettings();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rdbNativeWIA_CheckedChanged(object sender, EventArgs e)
        {
            cmbSource.Enabled = rdbConfig.Checked;
            cmbResolution.Enabled = rdbConfig.Checked;
            cmbPage.Enabled = rdbConfig.Checked;
            cmbDepth.Enabled = rdbConfig.Checked;
            cmbAlign.Enabled = rdbConfig.Checked;
            cmbScale.Enabled = rdbConfig.Checked;
            trBrightnes.Enabled = rdbConfig.Checked;
            trContrast.Enabled = rdbConfig.Checked;
        }

        private void loadWIA()
        {
            currentDeviceID = ScanSettings.DeviceID;
            CWIAAPI api;
            try
            {
                api = new CWIAAPI(ScanSettings.DeviceID);
            }
            catch (Exceptions.EScannerNotFound)
            {
                MessageBox.Show("Device not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ScanSettings.DeviceID = "";
                currentDeviceID = "";
                txtDevice.Text = "";
                return;
            }
            txtDevice.Text = api.DeviceName;
        }

        private void loadTWAIN()
        {
            Twain tw = new Twain();
            if (!tw.Init(this.Handle))
            {
                MessageBox.Show("No device found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ScanSettings.DeviceID = "";
                currentDeviceID = "";
                txtDevice.Text = "";
                return;
            }
            if (!tw.SelectByName(ScanSettings.DeviceID))
            {
                MessageBox.Show("Device not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ScanSettings.DeviceID = "";
                currentDeviceID = "";
                txtDevice.Text = "";
                return;
            }
            txtDevice.Text = tw.GetCurrentName();
            currentDeviceID = tw.GetCurrentName();
        }

        private void FEditScanSettings_Load(object sender, EventArgs e)
        {
            pctIcon.Image = ilProfileIcons.IconsList.Images[ScanSettings.IconID];
            txtName.Text = ScanSettings.DisplayName;
            cmbSource.SelectedIndex = (int)ScanSettings.Source;
            

            cmbDepth.SelectedIndex = (int)ScanSettings.Depth;
            cmbResolution.SelectedIndex = (int)ScanSettings.Resolution;
            trContrast.Value = ScanSettings.Contrast;
            trBrightnes.Value = ScanSettings.Brightnes;
            cmbPage.SelectedIndex = (int)ScanSettings.PageSize;
            cmbScale.SelectedIndex = (int)ScanSettings.AfterScanScale;
            cmbAlign.SelectedIndex = (int)ScanSettings.PageAlign;

            if (ScanSettings.DeviceDriver == CScanSettings.Driver.WIA)
            {
                rdWIA.Checked = true;
                if (ScanSettings.ShowScanUI)
                    rdbNativeWIA.Checked = true;
                else
                    rdbConfig.Checked = true;
            }
            else
            {
                rdTWAIN.Checked = true;
                rdbNativeWIA.Checked = true;
                rdbNativeWIA.Enabled = rdWIA.Checked;
                rdbConfig.Enabled = rdWIA.Checked;
            }


            if (ScanSettings.DeviceID != "")
            {
                if (ScanSettings.DeviceDriver == CScanSettings.Driver.WIA)
                    loadWIA();
                else
                    loadTWAIN();
            }
        }

        private void pctIcon_DoubleClick(object sender, EventArgs e)
        {
            FChooseIcon fic = new FChooseIcon();
            fic.ShowDialog();
            if (fic.IconID > -1)
            {
                pctIcon.Image = ilProfileIcons.IconsList.Images[fic.IconID];
                ScanSettings.IconID = fic.IconID;
            }
        }

        private void rdWIA_CheckedChanged(object sender, EventArgs e)
        {
            rdbNativeWIA.Checked = true;
            rdbNativeWIA.Enabled = rdWIA.Checked;
            rdbConfig.Enabled = rdWIA.Checked;
            ScanSettings.DeviceID = "";
            currentDeviceID = "";
            txtDevice.Text = "";
        }
    }
}