using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomCharacters
{
    class CharApi
    {
        public static void Init(string prefix)
        {
            CharApi.prefix = prefix;
            CharApiHiveMind.Init(prefix);
            Hooks.Init();
            ToolsCharApi.Init();
            ETGMod.StartGlobalCoroutine(DelayedStartCR());
            //SaveFileBullShit.Load();

        }

        public static IEnumerator DelayedStartCR()
        {
            yield return null;
            GameStatsManager.Load();
            yield break;
        }

        public static string prefix;
    }
}
