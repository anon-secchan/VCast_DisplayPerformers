using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Infiniteloop.VRLive;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

namespace DisplayPerformers
{
    [BepInPlugin(GUID: GUID, Name: "DisplayPerformers", Version: "1.1.0")]
    public class DisplayPerformers : BaseUnityPlugin
    {
        internal const string GUID = "jp.anon.vcast.DisplayPerformers";

        private VCastModManager.VCastModManager modManager;
        private bool showingUI = true;
        private Rect UI = new Rect(540, 20, 150, 50);

        private GUIStyle fontGUIStyle = new GUIStyle();
        private GUIStyle thumbStyle = new GUIStyle();
        private Texture2D dummyThumb = MakeTex(1000, 1000, new Color(0.43f, 0.43f, 0.43f, 0.4f));
        private int fontSizeLevel = 0;
        private int fontSizeMax = 140;
        private int nicknameMaxLength = 10;

        public PerformersState performersState = new PerformersState();

        private Dictionary<int, int> fontSizeSet = new Dictionary<int, int>()
        {
            {0,20},
            {1,15},
            {2,20},
            {3,30},
            {4,60},
            {5,80}
        };

        public static Dictionary<int, DPerformer> DPerformers = new Dictionary<int, DPerformer>();

        public class DPerformer
        {
            public string Nickname;
            public bool IsOwner;
            public Texture2D thumb;
        }

        protected void Start()
        {
            modManager = new VCastModManager.VCastModManager(GUID);
            ConfigInitialize();
            Hooks.InstallHooks();
            performersState.Start();
        }

        private void ConfigInitialize()
        {
            int defaultSize = fontSizeSet[0];
            int defaultNicknameLength = nicknameMaxLength;
            int configFontSize = defaultSize;
            int configNicknameLength = defaultNicknameLength;

            try
            {
                configFontSize = int.Parse(BepInEx.Config.GetEntry("default-font-size", "-1", GUID));
                if (configFontSize == -1)
                    BepInEx.Config.SetEntry("default-font-size", defaultSize.ToString(), GUID);
            }
            catch(Exception ex)
            {
                modManager.ErrorReport(ex);
            }

            if (configFontSize > fontSizeMax)
                configFontSize = fontSizeMax;

            if (configFontSize < 1)
                configFontSize = defaultSize;

            fontSizeSet[0] = configFontSize;


            try
            {
                configNicknameLength = int.Parse(BepInEx.Config.GetEntry("nickname-max-length", "-1", GUID));
                if (configNicknameLength == -1)
                    BepInEx.Config.SetEntry("nickname-max-length", defaultNicknameLength.ToString(), GUID);
            }
            catch (Exception ex)
            {
                modManager.ErrorReport(ex);
            }

            if (configNicknameLength > 20)
                configNicknameLength = 20;

            if (configNicknameLength < 1)
                configNicknameLength = defaultNicknameLength;

            nicknameMaxLength = configNicknameLength;

        }

        protected void Update()
        {
            try
            {
                if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F8))
                {
                    showingUI = !showingUI;

                    string str = "[MOD] ";

                    if (showingUI)
                        str += "DisplayPerformers has been Enabled";
                    else
                        str += "DisplayPerformers has been Disabled ";

                    StudioSelf.Notification.Notify(str);
                    BepInEx.Logger.Log(LogLevel.Debug, str);
                }

                if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift))
                {
                    if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
                        fontSizeLevel = 0;
                    else if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha1))
                        fontSizeLevel = 1;
                    else if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha2))
                        fontSizeLevel = 2;
                    else if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha3))
                        fontSizeLevel = 3;
                    else if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha4))
                        fontSizeLevel = 4;
                    else if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha5))
                        fontSizeLevel = 5;
                }
            }
            catch (Exception ex)
            {
                modManager.ErrorReport(ex);
            }

        }

        protected void OnGUI()
        {
            if (modManager.IsModEnable && showingUI && SceneManager.GetActiveScene().name == "StudioScene")
            {
                try
                {
                    PrintPerformersInfo();
                }
                catch(Exception ex)
                {
                    modManager.ErrorReport(ex);
                }
            }
        }

        private void PrintPerformersInfo()
        {
            UI.width = 150;
            UI.height = 50;
            fontGUIStyle.fontSize = fontSizeSet[fontSizeLevel];
            fontGUIStyle.normal.textColor = Color.white;
            UI = GUILayout.Window("jp.anon.vcast.DisplayPerformers".GetHashCode(), UI, WindowFunction, "キャスター");
        }

        private void WindowFunction(int windowID)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Space(5);

                foreach (var performer in DPerformers.Values)
                {
                    string nickname = performer.Nickname;

                    thumbStyle.fixedHeight = fontGUIStyle.fontSize+10;
                    thumbStyle.fixedWidth = fontGUIStyle.fontSize+10;

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(performer.thumb == null ? dummyThumb : performer.thumb, thumbStyle);
                        GUILayout.Space(5);

                        GUILayout.BeginVertical();
                        {
                            GUILayout.Space(10);
                            GUILayout.Label(nickname == null ? "" : nickname, fontGUIStyle);
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                    
                }

            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

    }

}