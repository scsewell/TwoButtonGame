using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using Framework;
using Framework.SettingManagement;
using Framework.IO;

namespace BoostBlasters
{
    public class SettingManager : Singleton<SettingManager>
    {
        private const string FILE_NAME = "Settings.ini";

        private static readonly string[] BOOL_VALS = new string[] { "On", "Off" };
        private static readonly string[] AA_VALS = new string[] { "Off", "Low", "High", };
        private static readonly string[] SHADOW_VALS = new string[] { "Off", "Low", "Medium", "High", "Ultra", };

        public struct Categories
        {
            public static readonly string Screen = "Screen";
            public static readonly string Audio = "Audio";
        }

        private Settings m_settings;
        public Settings Settings => m_settings;

        private Setting<int> m_aa;
        public Setting<int> AA => m_aa;

        private Setting<bool> m_useMotionBlur;
        public Setting<bool> UseMotionBlur => m_useMotionBlur;

        private Setting<int> m_masterVolume;
        public Setting<int> MasterVolume => m_masterVolume;

        private Setting<int> m_musicVolume;
        public Setting<int> MusicVolume => m_musicVolume;

        public SettingManager()
        {
            m_settings = new Settings();

            Resolution[] resolutions = Screen.resolutions;
            if (resolutions.Length > 0)
            {
                m_settings.Add(Categories.Screen, "Resolution", resolutions.Last(),
                    GetSupportedResolutions(),
                    (v) => SerializeResolution(v),
                    (s) => ParseResolution(s),
                    (v) => Screen.SetResolution(v.width, v.height, Screen.fullScreen)
                );
            }
            
            m_settings.Add(Categories.Screen, "Fullscreen", true, BOOL_VALS, SerializeBool, ParseBool, (v) => Screen.fullScreen = v);
            m_settings.Add(Categories.Screen, "VSync", true, BOOL_VALS, SerializeBool, ParseBool, (v) => QualitySettings.vSyncCount = (v ? 1 : 0));

            m_aa = m_settings.Add(Categories.Screen, "Anti Aliasing", 2, AA_VALS, (v) => AA_VALS[v], (s) => ParseIndex(s, AA_VALS));
            m_useMotionBlur = m_settings.Add(Categories.Screen, "Motion Blur", true, BOOL_VALS, SerializeBool, ParseBool);
            m_settings.Add(Categories.Screen, "Shadow Quality", 3, SHADOW_VALS, (v) => SHADOW_VALS[v], (s) => ParseIndex(s, SHADOW_VALS), (v) => QualitySettings.SetQualityLevel(v));

            string[] volumeValues = Enumerable.Range(0, 101).Where(i => i % 2 == 0).Select(v => v.ToString()).ToArray();
            m_masterVolume = m_settings.Add(Categories.Audio, "Master Volume", 100, volumeValues, (v) => v.ToString(), (s) => int.Parse(s));
            m_musicVolume = m_settings.Add(Categories.Audio, "Music Volume", 60, volumeValues, (v) => v.ToString(), (s) => int.Parse(s));
        }
    
        public void UseDefaults()
        {
            m_settings.UseDefaults();
        }

        public void Apply()
        {
            m_settings.Apply();
        }

        public void Save()
        {
            FileIO.WriteFile(JsonConverter.ToJson(m_settings.Serialize()), Path.Combine(FileIO.GetInstallDirectory(), FILE_NAME));
        }

        public void Load()
        {
            string str = FileIO.ReadFileText(Path.Combine(FileIO.GetInstallDirectory(), FILE_NAME));

            if (str == null || !m_settings.Deserialize(JsonConverter.FromJson<SerializableSettings>(str)))
            {
                m_settings.UseDefaults();
                Save();
            }
        }

        public void ConfigureCamera(Camera cam)
        {
            PostProcessLayer layer = cam.GetComponent<PostProcessLayer>();
            if (layer)
            {
                switch (m_aa.Value)
                {
                    case 0: layer.antialiasingMode = PostProcessLayer.Antialiasing.None; break;
                    case 1: layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing; break;
                    case 2: layer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing; break;
                }
            }
        }

        public void SetShadowDistance()
        {
            float distance = 20;
            switch (QualitySettings.GetQualityLevel())
            {
                case 4: distance = 500; break;
                case 3: distance = 450; break;
                case 2: distance = 300; break;
                case 1: distance = 100; break;
            }
            QualitySettings.shadowDistance = distance;
        }

        private string SerializeResolution(Resolution res)
        {
            return res.width + " x " + res.height;
        }

        private Resolution ParseResolution(string s)
        {
            Resolution res = new Resolution();
            string[] split = s.Split('x');
            res.width = int.Parse(split[0].Trim());
            res.height = int.Parse(split[1].Trim());
            return res;
        }

        private string[] GetSupportedResolutions()
        {
            List<string> resolutions = new List<string>();
            foreach (Resolution res in Screen.resolutions)
            {
                string resolution = SerializeResolution(res);
                if (!resolutions.Contains(resolution))
                {
                    resolutions.Add(resolution);
                }
            }
            return resolutions.ToArray();
        }

        private static string SerializeBool(bool b)
        {
            return BOOL_VALS[b ? 0 : 1];
        }

        private static bool ParseBool(string s)
        {
            return s == BOOL_VALS[0];
        }

        private static int ParseIndex(string s, string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == s)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
