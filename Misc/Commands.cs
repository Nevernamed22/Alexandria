//using Alexandria.SaveAPI;
using Alexandria.DungeonAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.Misc
{
    class Commands
    {
        public static void Init()
        {
            ETGModConsole.Commands.AddGroup("alexandria");

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("logAllFlags", delegate (string[] args)
            {
                /*foreach (var save in SaveAPIManager.AdvancedGameSaves)
                {
                    ETGModConsole.Log($"--=== {save.Key.ToUpper()} ===---");
                    var inst = AdvancedGameStatsManager.GetInstance(save.Key);
                    foreach (var flag in inst.m_flags)
                    {
                        //flag.ToString()
                        ETGModConsole.Log($"    {flag}");
                    }
                }
                ETGModConsole.Log($"--=== END ===---");*/
            });

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("checkStaticObjects", delegate (string[] args)
            {
                foreach (var thing in StaticReferences.customObjects)
                {
                    ETGModConsole.Log($"{thing.Key} - {thing.Value.name}");
                }
                
            });
        }
    }
}
