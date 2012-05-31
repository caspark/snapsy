using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Snapsy
{
    public partial class FPDFSave : Form
    {
        public FPDFSave()
        {
            InitializeComponent();
        }

        public void SetStatus(int count, int total)
        {
            lblStatus.Text = count.ToString() + " of " + total.ToString();
            Application.DoEvents();
        }
    }

}
