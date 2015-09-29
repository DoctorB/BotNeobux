using BotNeobux;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppBotNeobux
{
  public partial class FormMain : Form
  {
    private Neobux bot;

    public FormMain()
    {
      InitializeComponent();
      bot = new Neobux(string.Empty, string.Empty, string.Empty);
      bot.OnProgress += bot_OnProgress;
      bot.OnFinish += bot_OnFinish;

      txtUsername.Text = ConfigurationManager.AppSettings["username"];
      txtPassword.Text = ConfigurationManager.AppSettings["password"];
      txtExtraPassword.Text = ConfigurationManager.AppSettings["extrapassword"];
      frequency.Value = decimal.Parse(ConfigurationManager.AppSettings["frequency"]);

    }

    void bot_OnFinish(object sender, EventArgs e)
    {
      statusLabel.Text = "Status: Waiting...";
      timer1.Interval = (int)frequency.Value * 1000 * 60;
      timer1.Enabled = true;
    }

    private void btnStart_Click(object sender, EventArgs e)
    {
      btnStart.Enabled = false;
      btnStop.Enabled = true;
      SetEnabledGui(false);
      statusLabel.Text = "Status: Starting";

      bot.Username = txtUsername.Text;
      bot.Password = txtPassword.Text;
      bot.ExtraPassword = txtExtraPassword.Text;

      bot.Start();
    }

    void bot_OnProgress(object sender, ProgressEventArgs e)
    {
      statusLabel.Text = string.Format("Status: {0}", e.Progress);
    }
    
    private void btnStop_Click(object sender, EventArgs e)
    {
      timer1.Enabled = false;
      bot.Stop();
      btnStart.Enabled = true;
      btnStop.Enabled = false;
      SetEnabledGui(true);
      statusLabel.Text = "Status:";
    }

    private void SetEnabledGui(bool value)
    {
      txtUsername.Enabled = value;
      txtPassword.Enabled = value;
      txtExtraPassword.Enabled = value;
      frequency.Enabled = value;
    }

    private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
    {
      UpdateSettings();
      if (timer1 != null)
      {
        timer1.Enabled = false;
        timer1.Dispose();
      }
      if (bot != null)
      {
        bot.OnFinish -= bot_OnFinish;
        bot.OnProgress -= bot_OnProgress;
        bot.Dispose();
        bot = null;
      }
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      timer1.Enabled = false;
      if (bot != null) bot.Start();
    }

    private void UpdateSettings()
    {
      Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      configuration.AppSettings.Settings["username"].Value = txtUsername.Text;
      configuration.AppSettings.Settings["password"].Value = txtPassword.Text;
      configuration.AppSettings.Settings["extrapassword"].Value = txtExtraPassword.Text;
      configuration.AppSettings.Settings["frequency"].Value = frequency.Value.ToString();

      //Save only the modified section of the config
      configuration.Save(ConfigurationSaveMode.Modified);

      //Refresh the appSettings section to reflect updated configurations
      ConfigurationManager.RefreshSection("appSettings");
    }

  }
}
