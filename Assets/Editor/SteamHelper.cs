﻿using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using Gameloop.Vdf;
using UnityEditor;

public static class SteamHelper
{
    public static readonly int HOLLOWKNIGHT_APP_ID = 367520;
    public static readonly string HOLLOWKNIGHT_GAME_NAME = "Hollow Knight";

    public static string FindHollowKnightPath()
    {
        return FindSteamGamePath(HOLLOWKNIGHT_APP_ID, HOLLOWKNIGHT_GAME_NAME);
    }

    public static string FindSteamGamePath(int appid, string gameName)
    {
        //TODO: Linux, Mac Steam, and GOG.
        if (ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath") == null)
        {
            EditorUtility.DisplayDialog("HKEdit", "You either don't have steam installed or your registry variable isn't set.", "OK");
            return "";
        }

        string appsPath = Path.Combine((string)ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath"), "steamapps");

        if (File.Exists(Path.Combine(appsPath, "appmanifest_" + appid + ".acf")))
        {
            return Path.Combine(Path.Combine(appsPath, "common"), gameName);
        }

        string path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), appid, gameName);
        if (path == null)
        {
            EditorUtility.DisplayDialog("HKEdit", "It appears you don't have " + gameName + " installed anywhere. The game files are needed to find the game.", "OK");
        }
        else
        {
            return path;
        }

        return "";
    }

    private static string SearchAllInstallations(string libraryfolders, int appid, string gameName)
    {
        dynamic libraryData = VdfConvert.Deserialize(File.ReadAllText(libraryfolders));

        foreach (dynamic entry in libraryData.Value.Children())
        {
            int number;
            if (int.TryParse(entry.Key,out number))
            {
                foreach (dynamic item in entry.Value.Children())
                {
                    string key = item.Key;
                    string value = item.Value.ToString();
                    if (key == "path")
                    {
                        if (File.Exists(Path.Combine(value, "steamapps/appmanifest_" + appid + ".acf")))
                        {
                            return Path.Combine(Path.Combine(value, "steamapps/common"), gameName);
                        }
                    }
                }
                
            }
        }

        return null;
    }

    private static object ReadRegistrySafe(string path, string key)
    {
        using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(path))
        {
            if (subkey != null)
            {
                return subkey.GetValue(key);
            }
        }

        return null;
    }
}