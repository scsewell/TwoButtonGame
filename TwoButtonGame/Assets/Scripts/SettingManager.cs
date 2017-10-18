using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework;
using Framework.SettingManagement;
using Framework.IO;

public class SettingManager : Singleton<SettingManager>
{
    private static readonly string[] BOOL_VALS = new string[] { "On", "Off" };

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

    private Setting<Resolution> m_resolution;
    public Setting<Resolution> Resolution { get { return m_resolution; } }

    private Setting<bool> m_fullscreen;
    public Setting<bool> Fullscreen { get { return m_fullscreen; } }

    private Setting<bool> m_vsync;
    public Setting<bool> Vsync { get { return m_vsync; } }

    private Setting<int> m_masterVolume;
    public Setting<int> MasterVolume {  get { return m_masterVolume; } }

    private Setting<int> m_musicVolume;
    public Setting<int> MusicVolume { get { return m_musicVolume; } }

    public SettingManager()
    {
        m_settings = new Settings();

        m_resolution = m_settings.Add(Categories.Screen,"Resolution", Screen.resolutions.Last(),
            GetSupportedResolutions(),
            (v) => SerializeResolution(v),
            (s) => ParseResolution(s),
            (v) => Screen.SetResolution(v.width, v.height, Screen.fullScreen)
            );

        m_fullscreen    = m_settings.Add(Categories.Screen, "Fullscreen", true, BOOL_VALS, SerializeBool, ParseBool, (v) => Screen.fullScreen = v);
        m_vsync         = m_settings.Add(Categories.Screen, "VSync", true, BOOL_VALS, SerializeBool, ParseBool, (v) => QualitySettings.vSyncCount = (v ? 1 : 0));

        string[] volumeValues = Enumerable.Range(0, 101).Where(i => i % 2 == 0).Select(v => v.ToString()).ToArray();
        m_masterVolume = m_settings.Add(Categories.Audio, "Master Volume", 100, volumeValues, (v) => v.ToString(), (s) => int.Parse(s));
        m_musicVolume = m_settings.Add(Categories.Audio, "Music Volume", 50, volumeValues, (v) => v.ToString(), (s) => int.Parse(s));
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
        FileIO.WriteFile(JsonConverter.ToJson(m_settings.Serialize()), FileIO.GetInstallDirectory(), "Settings.ini");
    }

    public void Load()
    {
        string str = FileIO.ReadFile(FileIO.GetInstallDirectory(), "Settings.ini");
        if (str == null || !m_settings.Deserialize(JsonConverter.FromJson<SerializableSettings>(str)))
        {
            m_settings.UseDefaults();
            Save();
        }
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
}