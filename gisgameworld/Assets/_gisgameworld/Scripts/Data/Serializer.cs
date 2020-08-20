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

    public static void SerializeLevelData(LevelData levelData)
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "BuildingData");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filename = levelData.ID;
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

    public static LevelData DeserializeLevelData(string id)
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "BuildingData");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filename = id;
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
    }

    public static void SerializeLocation(Location location, string filename = @"default")
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "Location");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".json";

        string dataPath = Path.Combine(folderPath, filename);

        LocationData locationData = null;

        bool fileExists = File.Exists(dataPath);
        if(fileExists)
        {
            string existingFileContents = File.ReadAllText(dataPath);
            locationData = JsonConvert.DeserializeObject<LocationData>(existingFileContents);
        }

        using (StreamWriter file = File.CreateText(dataPath))
        {
            if (locationData == null)
            {
                locationData = new LocationData(location);
            }
            else
            {
                locationData.locations.Add(location);
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(file, locationData);
        }
    }

    public static LocationData DeserializeLocations(string filename = @"default")
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "Location");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".json";

        string dataPath = Path.Combine(folderPath, filename);

        bool fileExists = File.Exists(dataPath);
        if (!fileExists)
        {
            return null;
        }

        using (StreamReader file = File.OpenText(dataPath))
        {
            JsonSerializer serializer = new JsonSerializer();
            LocationData locationData = (LocationData)serializer.Deserialize(file, typeof(LocationData));

            return locationData;
        }
    }

    //public static void SerializeBuildings(Transform level, string filename = @"default")
    //{
    //    string appPath = Application.persistentDataPath;

    //    string folderPath = Path.Combine(appPath, "Level");
    //    if (!Directory.Exists(folderPath))
    //        Directory.CreateDirectory(folderPath);

    //    filename += @".";

    //    string dataPath = Path.Combine(folderPath, filename);

    //    LocationData locationData = null;

    //    bool fileExists = File.Exists(dataPath);
    //    if (fileExists)
    //    {
    //        string existingFileContents = File.ReadAllText(dataPath);
    //        locationData = JsonConvert.DeserializeObject<LocationData>(existingFileContents);
    //    }

    //    using (StreamWriter file = File.CreateText(dataPath))
    //    {
    //        if (locationData == null)
    //        {
    //            locationData = new LocationData(location);
    //        }
    //        else
    //        {
    //            locationData.locations.Add(location);
    //        }

    //        JsonSerializer serializer = new JsonSerializer();
    //        serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    //        serializer.Formatting = Formatting.Indented;
    //        serializer.Serialize(file, locationData);
    //    }
    //}


    //public static LocationData DeserializeLocations(string filename = @"default")
    //{
    //    string appPath = Application.persistentDataPath;

    //    string folderPath = Path.Combine(appPath, "Location");
    //    if (!Directory.Exists(folderPath))
    //        Directory.CreateDirectory(folderPath);

    //    filename += @".json";

    //    string dataPath = Path.Combine(folderPath, filename);

    //    bool fileExists = File.Exists(dataPath);
    //    if (!fileExists)
    //    {
    //        return null;
    //    }

    //    using (StreamReader file = File.OpenText(dataPath))
    //    {
    //        JsonSerializer serializer = new JsonSerializer();
    //        LocationData locationData = (LocationData)serializer.Deserialize(file, typeof(LocationData));

    //        return locationData;
    //    }
    //}

}
