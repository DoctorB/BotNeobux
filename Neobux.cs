using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace BotNeobux
{
  public class Neobux : IDisposable
  {
    #region Constant

    //private static string RED_DOT_URL_IMAGE = "http://img.neobux.com/imagens/badge_a1.gif";
    private static string RED_DOT_ID_PREFIX = "l";
    private static string TABLE_ID_PREFIX = "l0l";

    private static string DIV_RED_DOT_ID_PREFIX = "tg_";

    private static string TO_CLICK_ADV_DIV_CLASS_PURPLE = "adfu";
    private static string TO_CLICK_ADV_DIV_CLASS_ORANGE = "ad30";
    private static string TO_CLICK_ADV_DIV_CLASS_GRAY = "adf";

    #endregion

    private ChromeDriver driver;
    private bool IsWorking;

    private int purpleAdv;
    private int orangeAdv;
    private int grayAdv;
    private int advSize;


    private string handle;

    private IWebElement purpleAnchor;
    private IWebElement orangeAnchor;
    private IWebElement grayAnchor;
    private IWebElement advertiseAnchor;

    private IWebElement advertisementsAnchor;

    public event EventHandler<ProgressEventArgs> OnProgress;
    public event EventHandler<EventArgs> OnFinish;

    public Neobux(string userName, string password)
      : this(userName, password, string.Empty, "https://www.neobux.com/m/l/")
    {
    }

    public Neobux(string userName, string password, string extraPassword)
      : this(userName, password, extraPassword, "https://www.neobux.com/m/l/")
    {
    }

    public Neobux(string userName, string password, string extraPassword, string loginUrl)
    {
      driver = null;
      IsWorking = false;
      Username = userName;
      Password = password;
      ExtraPassword = extraPassword;
      LoginUrl = loginUrl;

      purpleAdv = 0;
      orangeAdv = 0;
      grayAdv = 0;
      advSize = 0;

      handle = string.Empty;

      purpleAnchor = null;
      orangeAnchor = null;
      grayAnchor = null;
      advertiseAnchor = null;

      advertisementsAnchor = null;
    }

    #region Bot Methods

    private ChromeDriver Build()
    {
      var chromeDriverService = ChromeDriverService.CreateDefaultService();
      chromeDriverService.HideCommandPromptWindow = true;
      return new ChromeDriver(chromeDriverService, new ChromeOptions());
    }

    private bool Login(out string message)
    {
      Progress("Login...");

      bool result = true;
      message = string.Empty;
      driver.Navigate().GoToUrl(LoginUrl);

      IWebElement userField = driver.FindElementById("Kf1");
      if (userField == null)
      {
        result = false;
        message = "(Login) - Element not found: Kf1" + Environment.NewLine;
      }

      IWebElement passField = driver.FindElementById("Kf2");
      if (passField == null)
      {
        result = false;
        message += "(Login) - Element not found: Kf2" + Environment.NewLine;
      }

      IWebElement extraField = driver.FindElementById("Kf4");
      if (extraField == null)
      {
        result = false;
        message += "(Login) - Element not found: Kf4" + Environment.NewLine;
      }

      IWebElement loginBtn = driver.FindElementById("botao_login");
      if (loginBtn == null)
      {
        result = false;
        message += "(Login) - Element not found: botao_login" + Environment.NewLine;
      }

      if (!result) return result;
      if (!string.IsNullOrEmpty(Username))
        userField.SendKeys(Username);
      if (!string.IsNullOrEmpty(Password))
        passField.SendKeys(Password);
      if (!string.IsNullOrEmpty(ExtraPassword))
        extraField.SendKeys(ExtraPassword);
      loginBtn.Click();

      WaitingFor("NeoBux - Your account");

      return result;
    }

    public void Stop()
    {
      IsWorking = false;
      if (driver != null)
      {
        driver.Quit();
        driver.Dispose();
        driver = null;
      }
    }

    public void Start()
    {
      if (driver == null)
      {
        driver = Build();
      }

      IsWorking = true;
      string error;
      if (!Login(out error))
      {
        Stop();
        return;
      }
      RefreshAnchors();
      if (IsWorking && advertisementsAnchor != null)
      {
        advertisementsAnchor.Click();
        WaitingFor("NeoBux - View Advertisements");
        RefreshAdvertisementsAndClick();
        driver.Navigate().Refresh();
        RefreshAnchors();
        Adprize();
      }

      if (OnFinish != null) OnFinish(this, null);
      Stop();
    }

    private void Progress(string message)
    {
      if (OnProgress != null)
      {
        OnProgress(null, new ProgressEventArgs(message));
      }
    }

    private void WaitingFor(string Title)
    {
      Progress(string.Format("Waiting for: {0}", Title));
      while (IsWorking && driver != null && driver.Title != Title)
      {
        Application.DoEvents();
        Thread.Sleep(1000);
      }
    }

    private void Adprize()
    {
      if (!IsWorking) return;
      if (advertiseAnchor == null) return;

      IWebElement element = driver.FindElementById("ap_h");
      if (element == null) return;

      string str_value = element.Text;
      if (string.IsNullOrEmpty(str_value)) return;
      int value;
      if (!int.TryParse(str_value, out value)) return;

      handle = driver.CurrentWindowHandle;
      for (int click = 1; click <= value; click++)
      {
        IWebElement jsEle = driver.FindElementById("ap_h");
        if (jsEle == null) break;
        if (jsEle.Displayed)
        {
          jsEle.Click();
          Progress("Click on adprize...");
          WaitRandomRange(15, 20);
          SwitchToWindow(f => f.CurrentWindowHandle != handle);
          CloseCurrentWindow();
          SwitchToWindow(f => f.CurrentWindowHandle == handle);
        } 
      }
    }

    private void RefreshAnchors()
    {
      if (!IsWorking) return;

      Progress("Refresh...");
      purpleAdv = 0;
      purpleAnchor = null;
      orangeAdv = 0;
      orangeAnchor = null;
      grayAdv = 0;
      grayAnchor = null;

      advertiseAnchor = null;
      advertisementsAnchor = null;

      ReadOnlyCollection<IWebElement> items = driver.FindElementsByTagName("a");
      foreach (var item in items)
      {
        int temp;
        if (item.GetAttribute("class") == "button small2 purple")
        {
          if (!string.IsNullOrEmpty(item.Text))
          {
            if (int.TryParse(item.Text, out temp))
            {
              purpleAnchor = item;
              purpleAdv = temp;
            }
          }
        }
        if (item.GetAttribute("class") == "button small2 orange")
        {
          if (!string.IsNullOrEmpty(item.Text))
          {
            if (int.TryParse(item.Text, out temp))
            {
              orangeAdv = temp;
              orangeAnchor = item;
            }
          }
        }
        if (item.GetAttribute("class") == "button small2 gray2")
        {
          if (!string.IsNullOrEmpty(item.Text))
          {
            if (int.TryParse(item.Text, out temp))
            {
              grayAdv = temp;
              grayAnchor = item;
            }
          }
        }
        if (item.GetAttribute("class") == "button green" && item.GetAttribute("href").StartsWith("http://www.neobux.com/m/v/?vl="))
        {
          advertisementsAnchor = item;
        }
        if (item.GetAttribute("class") == "button small2 blue" && item.GetAttribute("href").EndsWith("#adprize"))
        {
          advertiseAnchor = item;
        }
      }
      advSize = purpleAdv + orangeAdv + grayAdv;
      Progress(string.Format("Purple: {0} - Orange: {1} - Gray: {2}", purpleAdv, orangeAdv, grayAdv));
    }

    private void RefreshAdvertisementsAndClick()
    {
      if (!IsWorking) return;

      ReadOnlyCollection<IWebElement> items = driver.FindElementsByTagName("div");
      handle = driver.CurrentWindowHandle;

      // Possible stale elements
      List<string> list = new List<string>();
      foreach (var item in items)
      {
        if (item.GetAttribute("id").StartsWith(DIV_RED_DOT_ID_PREFIX) && CheckInClass(item))
        {
          list.Add(item.GetAttribute("id"));
        }
      }

      foreach (var str_id in list)
      {
        IWebElement item = driver.FindElementById(str_id);
        if (item == null) continue;
        if (!CheckInClass(item)) continue;
        IWebElement element_table = driver.FindElementById(TABLE_ID_PREFIX + str_id.Replace(DIV_RED_DOT_ID_PREFIX, string.Empty));
        IWebElement element_dot = driver.FindElementById(RED_DOT_ID_PREFIX + str_id.Replace(DIV_RED_DOT_ID_PREFIX, string.Empty));
        if (element_table == null || element_dot == null) continue;
        element_table.Click();
        if (element_dot.Displayed)
        {
          element_dot.Click();
          Progress("Click on advertisement...");
          WaitRandomRange(15, 20);
          SwitchToWindow(f => f.CurrentWindowHandle != handle);
          CloseCurrentWindow();
          SwitchToWindow(f => f.CurrentWindowHandle == handle);
        }
      }
    }

    private bool CheckInClass(IWebElement element)
    {
      return element.GetAttribute("class") == TO_CLICK_ADV_DIV_CLASS_PURPLE
        || element.GetAttribute("class") == TO_CLICK_ADV_DIV_CLASS_ORANGE
        || element.GetAttribute("class") == TO_CLICK_ADV_DIV_CLASS_GRAY;
    }

    private void WaitRandomRange(int from, int to)
    {
      Random rnd = new Random();
      int number = rnd.Next(from, to);
      for (int i = 1; i <= number; i ++)
      {
        try
        {
          driver.SwitchTo().Alert().Accept();
        }
        catch
        {

        }
        Application.DoEvents();
        Thread.Sleep(1000);
        if (!IsWorking) break;
      }        
    }

    private void CloseCurrentWindow()
    {
      if (!IsWorking) return;
      if (IsWorking && driver != null && driver.CurrentWindowHandle != handle)
      {
        driver.FindElementByTagName("body").SendKeys(OpenQA.Selenium.Keys.Control + "w");
      }
    }

    private void SwitchToWindow(Expression<Func<IWebDriver, bool>> predicateExp)
    {
      if (!IsWorking) return;
      var predicate = predicateExp.Compile();
      foreach (var handle in driver.WindowHandles)
      {
        driver.SwitchTo().Window(handle);
        if (predicate(driver))
        {
          return;
        }
      }
      throw new ArgumentException(string.Format("Unable to find window with condition: '{0}'", predicateExp.Body));
    }

    #endregion

    public string Username { get; set; }
    public string Password { get; set; }
    public string ExtraPassword { get; set; }
    public string LoginUrl { get; set; }

    public void Dispose()
    {
      Stop();
    }
  }
}