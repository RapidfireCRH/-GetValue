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
        public struct player_st
        {
            public string name;
            public string api;
            public bool queries;
            public int seconds_query;
            public star_st currloc;
        }
        player_st currcmdr;
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
            currcmdr.currloc = searchbyname("sol");
            textBox1.Text = "Enter Cmdr Name";
            button1.Enabled = false;
            button2.Enabled = false;
            toolStripLabel1.Text = "";
            settings_handle(settings.open);
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
                string result = new System.Net.WebClient().DownloadString("https://www.edsm.net/api-logs-v1/get-position?commanderName=" + textBox1.Text + (textBox4.Text != "" ? "&apiKey=" + textBox4.Text : ""));
                if (result.Length < 13 || result.Contains("\"msgnum\":203"))
                {
                    if (textBox4.Text == "")//api is blank
                    {
                        toolStripLabel1.Text = "No such commander name.";
                        textBox1.BackColor = Color.Red;
                        settings_handle(settings.open);
                        return;
                    }
                    else
                    {
                        toolStripLabel1.Text = "Invalid API Key";
                        textBox1.BackColor = Color.Red;
                        settings_handle(settings.open);
                        return;
                    }
                }
                if (result.Contains("\"system\":null") && textBox4.Text == "")
                {
                    toolStripLabel1.Text = "Non-public profile.";
                    textBox1.BackColor = Color.Yellow;
                    settings_handle(settings.open);
                    return;
                }
                currcmdr.name = textBox1.Text;
                currcmdr.queries = checkBox1.Checked;
                try
                {
                    if (currcmdr.queries)
                    {
                        int temp = Int32.Parse(textBox3.Text);
                        if (temp < 1 || temp > 65535)
                            throw new ArgumentOutOfRangeException();
                        currcmdr.seconds_query = temp;
                        timer1.Interval = currcmdr.seconds_query * 1000;
                    }

                }
                catch
                {
                    currcmdr.seconds_query = 10;
                    toolStripLabel1.Text = "Error reading seconds, make sure its a value between 1 and 65535.";
                    timer1.Interval = currcmdr.seconds_query * 1000;
                }
                timer1.Enabled = currcmdr.queries;
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
            currcmdr.currloc = searchbyname(result.Substring(result.IndexOf(",\"system\":\"") + ",\"system\":\"".Length, result.IndexOf("\",\"firstDiscover") - (result.IndexOf(",\"system\":\"") + ",\"system\":\"".Length)));
            if (currcmdr.currloc.value == -1)
                textBox2.Text = currcmdr.currloc.name + Environment.NewLine + "Error in determining value. API overloaded?";
            else
                textBox2.Text = currcmdr.currloc.name + Environment.NewLine + currcmdr.currloc.value;
            button2.Text = "Open " + currcmdr.currloc.name;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currcmdr.currloc.url != "")
                System.Diagnostics.Process.Start(currcmdr.currloc.url);
        }

        enum settings { open, closed };

        private void button3_Click(object sender, EventArgs e)
        {            
            if (button3.Text == " Settings>")//closed
            {
                settings_handle(settings.open);
                return;
            }
            //Open
            settings_handle(settings.closed);
            if (textBox1.Text != "Enter Cmdr Name") //rerun checks to see if there are any changes
                textBox1_Leave(sender, e);
        }
        private void settings_handle(settings state)
        {
            if (state == settings.open)
            {

                ActiveForm.Size = ActiveForm.MaximumSize = ActiveForm.MinimumSize = new Size(599, 239);
                button3.Location = new Point(504, 172);
                textBox1.Enabled = textBox3.Enabled = textBox4.Enabled = checkBox1.Enabled = true;
                button3.Text = "<Settings ";
            }
            else
            {
                ActiveForm.Size = ActiveForm.MaximumSize = ActiveForm.MinimumSize = new Size(251, 239);
                button3.Location = new Point(152, 172);
                textBox1.Enabled = textBox3.Enabled = textBox4.Enabled = checkBox1.Enabled = false;
                button3.Text = " Settings>";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!checkBox1.Checked)
            {
                timer1.Enabled = false;
                return;
            }
            try
            {
                timer1.Interval = currcmdr.seconds_query * 1000;
            }
            catch
            {
                timer1.Interval = 10000;
            }
            button1_Click(sender, e);
        }
    }
}
