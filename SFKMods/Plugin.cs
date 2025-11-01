using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ModItems;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFKMod.Mods
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        public bool m_AddBg = false;

        private bool m_Visible = false;
        public const string PLUGIN_GUID = "com.sfk.uilib";
        public const string PLUGIN_NAME = "SFKUI Lib";
        public const string PLUGIN_VERSION = "1.0.0";
        private Rect m_WindowRect = new(100, 100, 300, 200);
        internal static new ManualLogSource Logger;
        private ConfigEntry<KeyboardShortcut> m_ToggleKey;
        private ConfigEntry<float> m_CfgWindowX;
        private ConfigEntry<float> m_CfgWindowY;
        private ConfigEntry<float> m_CfgWindowW;
        private ConfigEntry<float> m_CfgWindowH;

        Harmony harmony = new Harmony(PLUGIN_GUID);

        void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            m_ToggleKey = Config.Bind("General", "ToggleKey", new KeyboardShortcut(KeyCode.F1), "Key to show/hide the GUI panel");

            m_CfgWindowX = Config.Bind("Window", "X", m_WindowRect.x, "Window X position");
            m_CfgWindowY = Config.Bind("Window", "Y", m_WindowRect.y, "Window Y position");
            m_CfgWindowW = Config.Bind("Window", "W", m_WindowRect.width, "Window width");
            m_CfgWindowH = Config.Bind("Window", "H", m_WindowRect.height, "Window height");

            m_WindowRect.x = m_CfgWindowX.Value;
            m_WindowRect.y = m_CfgWindowY.Value;
            m_WindowRect.width = m_CfgWindowW.Value;
            m_WindowRect.height = m_CfgWindowH.Value;

            SceneManager.sceneLoaded += OnSceneLoaded;

            harmony.PatchAll();

            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            DontDestroyOnLoad(this);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            Logger.LogInfo($"scene={scene.name} loadMode={loadMode}");
            if (scene.name == "TitleScene")
            {
                Sprite defenseIcon = UnityEngine.Resources
                    .FindObjectsOfTypeAll<Sprite>()
                    .FirstOrDefault(s => s.name == "IconDefense");
                var bigShield = new ModItemDef
                {
                    id = "mod:BigShield100",
                    title = "Big Shield (+100)",
                    description = "Increase base shield by 100.",
                    icon = defenseIcon,
                    cost = 10,
                    cooldown = 9999999f, // one-time
                    statMods =
                    {
                        new ModStatMod { statPath = "maxShield", valueType = ModValueType.Flat, value = 100f, origin = -1 }
                    }
                };
                ModItemRegistry.Register(bigShield);
                Logger.LogInfo("[ModItems] Registered mod:BigShield100.");
            }
        }

        void Update()
        {
            if (m_ToggleKey.Value.IsDown())
            {
                m_Visible = !m_Visible;
            }
        }

        void OnGUI()
        {
            if (!m_Visible)
            {
                return;
            }
            m_WindowRect = GUILayout.Window(123456, m_WindowRect, DrawWindowContents, "Mod");
        }

        Vector3 ScreenCenter()
        {
            // Convert screen center to world
            var cam = Camera.main;
            if (!cam)
            {
                Debug.LogWarning("No main camera found!");
                return Vector3.zero;
            }

            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, cam.nearClipPlane + 5f);
            Vector3 worldPos = cam.ScreenToWorldPoint(screenCenter);
            worldPos.z = 0f;

            return worldPos;
        }

        private void DrawWindowContents(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("IMGUI Panel");
            if (GUILayout.Button("Rect"))
            {
                UIObject.TestCreateRandomRect("randRect", UnityEngine.Color.red, GameObject.Find("Canvas").transform);
            }
            if (GUILayout.Button("Text"))
            {
                UIObject.TestCreateRandomTextRect("randText", "Hello World", GameObject.Find("Canvas").transform);
            }
            if (GUILayout.Button("Button"))
            {
                UIObject.TestCreateMenuButton(
                    "menuButton",
                    "Toggle GUI",
                    () => { m_Visible = !m_Visible; },
                    GameObject.Find("Canvas/Left/Menu").transform
                )
                .RelativeTo(
                    GameObject.Find("Canvas/Left/Menu/Profiles"), 
                    new Vector2(0, -50f)
                );
            }
            if (GUILayout.Button("Layout"))
            {
                var panel = UIObject.CreateVerticalLayout(new Vector2(250, 200), new Vector2(600, -500), new UnityEngine.Color(1, 1, 1, 0.5f));
                // Test adding 3 things
                for (int i = 0; i < 3; i++)
                {
                    var btn = UIObject.TestCreateMenuButton($"test_{i}", $"button {i}", null, panel.transform);
                }
            }
            if (GUILayout.Button("Test Resource Spawn"))
            {
                ShardAPI.SpawnFaith(ScreenCenter(), 1, "CustomFaith");
            }
            if (GUILayout.Button("Test Item Spawn"))
            {
                var cam = Camera.main;
                var wp = cam ? cam.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, cam.nearClipPlane + 5f)) : Vector3.zero;
                ModItemSpawner.SpawnDrag("mod:BigShield100", new Vector2(wp.x, wp.y));
            }


            GUILayout.Space(8);
            GUILayout.Label($"Window position: {Mathf.RoundToInt(m_WindowRect.x)}, {Mathf.RoundToInt(m_WindowRect.y)}");
            GUILayout.Label($"Window size: {Mathf.RoundToInt(m_WindowRect.width)} x {Mathf.RoundToInt(m_WindowRect.height)}");

            if (GUILayout.Button("Close"))
            {
                m_Visible = false;
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void OnApplicationQuit()
        {
            SaveWindowRectToConfig();
            harmony.UnpatchSelf();
        }

        private void OnDestroy()
        {
            SaveWindowRectToConfig();
            harmony.UnpatchSelf();
        }

        private void SaveWindowRectToConfig()
        {
            m_CfgWindowH.Value = m_WindowRect.x;
            m_CfgWindowH.Value = m_WindowRect.y;
            m_CfgWindowH.Value = m_WindowRect.width;
            m_CfgWindowH.Value = m_WindowRect.height;

            Config.Save();
        }

    }
}
