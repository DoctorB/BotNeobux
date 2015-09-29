using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotNeobux
{
  public class ProgressEventArgs : EventArgs
  {
    private string _progress;
    public ProgressEventArgs(string Progress)
    {
      _progress = Progress;
    }

    public string Progress { get { return _progress; } }
  }
}
