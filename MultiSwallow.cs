using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace MultiSwallow
{
    [BepInPlugin("drwoof.multiswallow", "MultiSwallow", "0.0.1")]
    public class MultiSwallow : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource modLogger;
        public MultiSwallow()
        {
            modLogger = Logger;
            new MultiSwallowPatch(this, Logger);
        }
    }
}
