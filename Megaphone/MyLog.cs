using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;

namespace Megaphone;

public class MyLog
{
    public static ManualLogSource Logger => Megaphone.Logger;
}
