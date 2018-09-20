using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalSystem
{
    public partial class Form1 : Form
    {
        star_st curr;
        public struct star_st
        {
            public int edsm_id;
            public string name;
            public long value;
            public string url;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            curr = searchbyname("sol");
            textBox1.Text = "Enter Cmdr Name";
            button1.Enabled = false;
            button2.Enabled = false;
            toolStripLabel1.Text = "";
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "Enter Cmdr Name";
                textBox1.ForeColor = Color.DarkGray;
            }
            else//Check to make sure that the commander name is correct
            {
                string result = new System.Net.WebClient().DownloadString("https://www.edsm.net/api-logs-v1/get-position?commanderName=" + textBox1.Text);
                if (result.Length < 13 || result.Contains("\"msgnum\":203"))
                {
                    toolStripLabel1.Text = "No such commander name.";
                    textBox1.BackColor = Color.Red;
                    return;
                }
                if (result.Contains("\"system\":null"))
                {
                    toolStripLabel1.Text = "Non-public profile.";
                    textBox1.BackColor = Color.Yellow;
                    return;
                }
                textBox1.BackColor = Color.Green;
                button1.Enabled = true;
            }
        }
        private void textBox1_Enter(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            textBox1.ForeColor = DefaultForeColor;
            textBox1.BackColor = Color.White;
            if (textBox1.Text == "Enter Cmdr Name")
            {
                textBox1.Text = "";
            }
        }
        public star_st searchbyname(string starname)
        {
            string result = new System.Net.WebClient().DownloadString("https://www.edsm.net/api-system-v1/estimated-value?systemName=" + starname);
            star_st ret = new star_st();
            ret.name = starname;
            if (result.IndexOf(",\"estimatedValue\":") == -1)
            {
                ret.value = -1;
                ret.url = "";
                return ret;
            }
            ret.value = Int64.Parse(result.Substring(result.IndexOf(",\"estimatedValue\":") + ",\"estimatedValue\":".Length, result.Length - (result.IndexOf(",\"estimatedValue\":") + ",\"estimatedValue\":".Length) - 1));
            ret.url = result.Substring(result.IndexOf(",\"url\":") + ",\"url\":".Length, (result.IndexOf(",\"estimatedValue\":") - (result.IndexOf(",\"url\":") + ",\"url\":".Length))).Replace("\\", "");
            ret.url = ret.url.Substring(1, ret.url.Length - 2);
            return ret;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string result = new System.Net.WebClient().DownloadString("https://www.edsm.net/api-logs-v1/get-position?commanderName=" + textBox1.Text);
            curr = searchbyname(result.Substring(result.IndexOf(",\"system\":\"") + ",\"system\":\"".Length, result.IndexOf("\",\"firstDiscover") - (result.IndexOf(",\"system\":\"") + ",\"system\":\"".Length)));
            textBox2.Text = curr.name + Environment.NewLine + curr.value;
            button2.Text = "Open " + curr.name;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (curr.url != "")
                System.Diagnostics.Process.Start(curr.url);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "Enter Cmdr Name")
                textBox1_Leave(sender, e);
        }
    }
}