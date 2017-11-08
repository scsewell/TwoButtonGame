using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PostProcessing;
using Framework;
using Framework.SettingManagement;
using Framework.IO;

public class SettingManager : Singleton<SettingManager>
{
    private static readonly string[] BOOL_VALS = new string[] { "On", "Off" };
    private static readonly string[] AA_VALS = new string[] { "High", "Low", "Off" };
    private static readonly string[] SHADOW_VALS = new string[] { "Ultra", "High", "Medium", "Low", "Off" };

    public struct Categories
    {
        public static readonly string Screen = "Screen";
        public static readonly string Audio = "Audio";
    }

    private Settings m_settings;
    public Settings Settings
    {
        get { return m_settings; }
    }
    
    private Setting<int> m_aa;
    public Setting<int> AA { get { return m_aa; } }

    private Setting<bool> m_useMotionBlur;
    public Setting<bool> UseMotionBlur { get { return m_useMotionBlur; } }

    private Setting<int> m_masterVolume;
    public Setting<int> MasterVolume { get { return m_masterVolume; } }

    private Setting<int> m_musicVolume;
    public Setting<int> MusicVolume { get { return m_musicVolume; } }

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

        m_aa = m_settings.Add(Categories.Screen, "Anti Aliasing", 1, AA_VALS, (v) => AA_VALS[v], (s) => ParseIndex(s, AA_VALS));
        m_useMotionBlur = m_settings.Add(Categories.Screen, "Motion Blur", true, BOOL_VALS, SerializeBool, ParseBool);
        m_settings.Add(Categories.Screen, "Shadow Quality", 1, SHADOW_VALS, (v) => SHADOW_VALS[v], (s) => ParseIndex(s, SHADOW_VALS), (v) => QualitySettings.SetQualityLevel(v));

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
#if (UNITY_EDITOR || UNITY_STANDALONE)
        FileIO.WriteFile(JsonConverter.ToJson(m_settings.Serialize()), FileIO.GetInstallDirectory(), "Settings.ini");
#elif (!UNITY_WEBGL)
        PlayerPrefs.SetString("Settings", JsonConverter.ToJson(m_settings.Serialize()));
#endif
    }

    public void Load()
    {
#if (UNITY_EDITOR || UNITY_STANDALONE)
        string str = FileIO.ReadFile(FileIO.GetInstallDirectory(), "Settings.ini");
#else
        string str = PlayerPrefs.GetString("Settings", null);
#endif
#if UNITY_WEBGL
        str = null;
#endif
        if (str == null || !m_settings.Deserialize(JsonConverter.FromJson<SerializableSettings>(str)))
        {
            m_settings.UseDefaults();
            Save();
        }
    }

    public void ConfigureCamera(Camera cam, bool permitMotionBlur)
    {
        PostProcessingBehaviour post = cam.GetComponent<PostProcessingBehaviour>();
        PostProcessingProfile profile = Object.Instantiate(post.profile);

        profile.motionBlur.enabled = m_useMotionBlur.Value && permitMotionBlur;

        int aaVal = m_aa.Value;
        if (aaVal < 2)
        {
            profile.antialiasing.enabled = true;
            AntialiasingModel.Settings aa = profile.antialiasing.settings;
            aa.method = (aaVal == 0) ? AntialiasingModel.Method.Taa : AntialiasingModel.Method.Fxaa;
            profile.antialiasing.settings = aa;
        }
        else
        {
            profile.antialiasing.enabled = false;
        }

        post.profile = profile;
    }

    public void SetShadowDistance()
    {
        float distance = 20;
        switch (QualitySettings.GetQualityLevel())
        {
            case 0: distance = 500; break;
            case 1: distance = 450; break;
            case 2: distance = 300; break;
            case 3: distance = 100; break;
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
        resolutions.Reverse();
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