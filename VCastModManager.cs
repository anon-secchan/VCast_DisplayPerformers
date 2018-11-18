using BepInEx;
using Harmony;
using UnityEngine.UI;
using UnityEngine;
using BepInEx.Logging;
using Infiniteloop.VRLive;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;


namespace VCastModManager
{
    public class VCastModManager 
    {
        private bool _isModEnable;
        private string GUID;
        private List<ReportData> _reportCollection = new List<ReportData>();
        
        class ReportData
        {
            public DateTime date;
            public Exception error;

            public ReportData(DateTime date, Exception error)
            {
                this.date = date;
                this.error = error;
            }
        }

        public VCastModManager(string guid)
        {
            this.GUID = guid;
            this._isModEnable = true;

            RewriteVersionTextObject();
            Hooks.InstallHooks();
        }


        public static void Init()
        {
            RewriteVersionTextObject();
            Hooks.InstallHooks();
        }

        private static void RewriteVersionTextObject()
        {
            string modPostfix = "-MOD";

            GameObject txtversionGo = GameObject.Find("TxtVersion");
            Text goText = txtversionGo.GetComponent<Text>();
            if (goText.text.Contains(modPostfix))
                return;

            goText.text = goText.text + modPostfix;
        }

        public void ErrorReport(Exception ex)
        {
            BepInEx.Logger.Log(LogLevel.Error, $"[MOD] {ex.GetType().Name} => {ex.Message}, StackTrace: {ex.StackTrace}");

            if (!IsModEnable)
                return;

            DateTime dt = DateTime.Now;
            ReportData report = new ReportData(dt, ex);

            this._reportCollection.Add(report);
            CleanReportCollection();

            if (this._reportCollection.Count() >= 10)
            {
                TimeSpan ts = this._reportCollection.Last().date - this._reportCollection[0].date ;
                if (ts.Seconds <= 10)
                {
                    this._isModEnable = false;
                    BepInEx.Logger.Log(LogLevel.Fatal, $"{this.GUID} caused mass errors. MOD has been disabled.");
                    StudioSelf.Notification.Notify($"MOD:{this.GUID}でエラーが発生したため機能を停止しました。");
                }
            }

        }

        private void CleanReportCollection()
        {
            int maxlength = 10;

            while (this._reportCollection.Count() > maxlength)
            {
                this._reportCollection.RemoveAt(0);
            }
        }

        public bool IsModEnable
        {
            get
            {
                return this._isModEnable;
            }
        }

    }
    public static class Hooks
    {

        public static void InstallHooks()
        {
            try
            {
                var harmony = HarmonyInstance.Create("jp.anon.vcast.VCastModManager"); 
                harmony.PatchAll(typeof(Hooks));
            }
            catch (System.Exception ex)
            {
                BepInEx.Logger.Log(LogLevel.Error, ex);
            }

        }

        [HarmonyPostfix, HarmonyPatch(typeof(Title), "Awake")]
        public static void AwakeHook(Title __instance)
        {
            string modPostfix = "-MOD";

            FieldInfo cls_versionText = typeof(Title).GetField("versionText", BindingFlags.NonPublic | BindingFlags.Instance);
            Text versionText = (Text)cls_versionText.GetValue(__instance);

            if (versionText.text.Contains(modPostfix))
                return;

            versionText.text = versionText.text + modPostfix;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ErrorLogger), "Write")]
        public static bool DisableWrite()
        {
            BepInEx.Logger.Log(LogLevel.Message, $"[MOD] ErrorLogger Write Stopper!");
            return false;
        }

    }
}
