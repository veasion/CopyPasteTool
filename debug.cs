using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CopyPasteTool
{
    public partial class DebugForm : Form
    {

        private MainWindow main;

        public DebugForm(MainWindow main)
        {
            this.main = main;
            InitializeComponent();
        }

        public void ShowAndInit(string param2)
        {
            this.textBox_param2.Text = param2;
            this.Show();
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            main.changeParam2(null);
            this.Hide();
        }

        private void But_run_Click(object sender, EventArgs e)
        {
            this.textBox_result.Text = main.invokeJsMethod(this.textBox_text.Text, true);
        }

        private void TextBox_param2_TextChanged(object sender, EventArgs e)
        {
            main.changeParam2(this.textBox_param2.Text);
        }

        private void TextBox_text_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\x1')
            {
                ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
        }

        private void TextBox_param2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\x1')
            {
                ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
        }

        private void TextBox_result_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\x1')
            {
                ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
        }
    }
}
