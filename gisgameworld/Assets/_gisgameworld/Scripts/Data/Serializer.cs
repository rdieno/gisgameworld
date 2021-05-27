using UnityEngine;
using System.Collections;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

public class Serializer
{
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

            levelData.PrepareForSerialization();

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

    public static void SerializeTestResults(String results, string filename = @"default")
    {
        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "Tests");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".txt";

        string dataPath = Path.Combine(folderPath, filename);

        StreamWriter sr = File.CreateText(dataPath);
        sr.Write(results);
        sr.Close();
    }

    public static void SerializeTestResults(List<BuildingTest> testResults, string filename = @"default")
    {
        // organize results into a table format

        DataSet dataSet = new DataSet("dataSet");
        //dataSet.Namespace = "NetFrameWork";
        DataTable table = new DataTable();

        DataColumn indexColumn = new DataColumn("Index", typeof(int));
        DataColumn rulesetColumn = new DataColumn("Ruleset", typeof(string));
        DataColumn shapeNameColumn = new DataColumn("Shape", typeof(string));
        DataColumn operationColumn = new DataColumn("Operation", typeof(string));
        DataColumn partColumn = new DataColumn("Part", typeof(string));
        DataColumn resultColumn = new DataColumn("Result", typeof(bool));

        table.Columns.Add(indexColumn);
        table.Columns.Add(rulesetColumn);
        table.Columns.Add(shapeNameColumn);
        table.Columns.Add(operationColumn);
        table.Columns.Add(partColumn);
        table.Columns.Add(resultColumn);

        dataSet.Tables.Add(table);

        foreach (BuildingTest bt in testResults)
        {
            int buildingIndex = bt.buildingIndex;
            string ruleset = bt.ruleset;

            foreach(ShapeTest st in bt.shapeTests)
            {
                string shapeName = st.shapeName;

                foreach(OperationTest ot in st.operationTests)
                {
                    foreach(bool result in ot.result)
                    {
                        DataRow newRow = table.NewRow();
                        newRow["Index"] = buildingIndex;
                        newRow["Ruleset"] = ruleset;
                        newRow["Shape"] = shapeName;
                        newRow["Operation"] = ot.operation;
                        newRow["Part"] = ot.part;
                        newRow["Result"] = result;
                        table.Rows.Add(newRow);
                    }
                }
            }
        }

        //dataSet.AcceptChanges();


        

        //string output = JsonConvert.SerializeObject(dataSet, Formatting.Indented);

        // save to file

        string appPath = Application.persistentDataPath;

        string folderPath = Path.Combine(appPath, "Tests");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filename += @".csv";

        string dataPath = Path.Combine(folderPath, filename);

        table.ToCSV(dataPath);

        //StreamWriter sr = File.CreateText(dataPath);
        //sr.Write(output);
        //sr.Close();
    }
}
