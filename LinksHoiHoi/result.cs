using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LinksHoiHoi
{
    public partial class result : Form
    {
        public result(int[] SorF)
        {
            InitializeComponent();
            label1.Text = SorF[0]+"";
            label2.Text = SorF[1] + "";
            label3.Text = SorF[2] + "";
            label4.Text = SorF[3] + "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void result_Load(object sender, EventArgs e)
        {

        }
    }
}
