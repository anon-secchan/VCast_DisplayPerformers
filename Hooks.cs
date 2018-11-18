using Harmony;
using BepInEx.Logging;
using VRM;
using System.Reflection;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Infiniteloop.VRLive;

namespace DisplayPerformers
{
    public static class Hooks
    {

        public static void InstallHooks()
        {
            try
            {
                var harmony = HarmonyInstance.Create(DisplayPerformers.GUID);
                harmony.PatchAll(typeof(Hooks));
            }
            catch (System.Exception ex)
            {
                BepInEx.Logger.Log(LogLevel.Error, ex);
            }

        }
        
    }
}
