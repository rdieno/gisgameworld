using UnityEngine;
using System.Collections;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

public class Serializer
{
    //public static void SerializeOSMData(string json, string filename = @"default")
    //{
    //    string appPath = Application.persistentDataPath;

    //    string folderPath = Path.Combine(appPath, "OSMData");
    //    if (!Directory.Exists(folderPath))
    //        Directory.CreateDirectory(folderPath);

    //    filename += @".osm";

    //    string dataPath = Path.Combine(folderPath, filename);

    //    try
    //    {
    //        File.WriteAllText(dataPath, json);
    //        Debug.Log("Saved raw OSM data to file: " + dataPath);
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine($"There was a problem writing raw OSM data to file: '{e}'");
    //    }
    //}


    public static void SerializeOSMData(OSMData data, string filename = @"default")
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "OSMData");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".osm";

        string dataPath = Path.Combine(folderPath, filename);

        using (StreamWriter file = File.CreateText(dataPath))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(file, data);
        }
    }


    public static void SerializeOSMInfo(OSMInfo info, string filename = @"default")
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "OSMData");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".info";

        string dataPath = Path.Combine(folderPath, filename);

        using (StreamWriter file = File.CreateText(dataPath))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(file, info);
        }
    }

    public static OSMInfo DeserializeOSMInfo(string filename = @"default", bool useStreamingAssets = false)
    {
        string appPath = String.Empty;

        if (useStreamingAssets)
        {
            appPath = Application.streamingAssetsPath;
        }
        else
        {
            appPath = Application.persistentDataPath;
        }

        string folderPath = Path.Combine(appPath, "OSMData");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".info";

        string dataPath = Path.Combine(folderPath, filename);

        using (StreamReader file = File.OpenText(dataPath))
        {
            JsonSerializer serializer = new JsonSerializer();
            OSMInfo osmData = (OSMInfo)serializer.Deserialize(file, typeof(OSMInfo));

            return osmData;
        }
    }

    public static OSMData DeserializeOSMData(string filename = @"default", bool useStreamingAssets = false)
    {
        string appPath = String.Empty;

        if (useStreamingAssets)
        {
            appPath = Application.streamingAssetsPath;
        }
        else
        {
            appPath = Application.persistentDataPath;
        }

        string folderPath = Path.Combine(appPath, "OSMData");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".osm";

        string dataPath = Path.Combine(folderPath, filename);

        using (StreamReader file = File.OpenText(dataPath))
        {
            JsonSerializer serializer = new JsonSerializer();
            OSMData osmData = (OSMData)serializer.Deserialize(file, typeof(OSMData));

            return osmData;
        }
    }

    public static void SerializeLevelData(LevelData levelData, string filename = @"default")
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "BuildingData");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".lvl";

        string dataPath = Path.Combine(folderPath, filename);

        using (StreamWriter file = File.CreateText(dataPath))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(file, levelData);
        }
    }

    public static LevelData DeserializeLevelData(string filename = @"default")
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "BuildingData");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".lvl";

        string dataPath = Path.Combine(folderPath, filename);

        string json = "";

        try
        {
            json = File.ReadAllText(dataPath);
        }
        catch (Exception e)
        {
            Console.WriteLine("There was a problem reading from level data file: " + e);
        }

        return JsonConvert.DeserializeObject<LevelData>(json);

        //using (StreamReader file = File.OpenText(dataPath))
        //{
        //    Debug.Log(@"test: " + file.ReadToEnd());

        //    JsonSerializer serializer = new JsonSerializer();
        //    serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //   // serializer.Formatting = Formatting.Indented;

        //    LevelData levelData = (LevelData)serializer.Deserialize(file, typeof(LevelData));

        //    return levelData;
        //}
    }

    //public static OSMInfo DeserializeOSMInfoFromStreamingAssets(string filename = @"default")
    //{
    //    string appPath = Application.streamingAssetsPath;

    //    string folderPath = Path.Combine(appPath, "OSMData");
    //    if (!Directory.Exists(folderPath))
    //        Directory.CreateDirectory(folderPath);

    //    filename += @".info";

    //    string dataPath = Path.Combine(folderPath, filename);

    //    using (StreamReader file = File.OpenText(dataPath))
    //    {
    //        JsonSerializer serializer = new JsonSerializer();
    //        OSMInfo osmData = (OSMInfo)serializer.Deserialize(file, typeof(OSMInfo));

    //        return osmData;
    //    }
    //}

    //public static OSMData DeserializeOSMDataFromStreamingAssets(string filename = @"default")
    //{
    //    string appPath = Application.streamingAssetsPath;

    //    string folderPath = Path.Combine(appPath, "OSMData");
    //    if (!Directory.Exists(folderPath))
    //        Directory.CreateDirectory(folderPath);

    //    filename += @".osm";

    //    string dataPath = Path.Combine(folderPath, filename);

    //    using (StreamReader file = File.OpenText(dataPath))
    //    {
    //        JsonSerializer serializer = new JsonSerializer();
    //        OSMData osmData = (OSMData)serializer.Deserialize(file, typeof(OSMData));

    //        return osmData;
    //    }
    //}
}
