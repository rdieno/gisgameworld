using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class OverpassManager
{
    private const string Overpass_URL_String = "http://overpass-api.de/api/interpreter";
    
    public IEnumerator RunQuery(OSMInfo info)//, bool saveData = false)
    {
        // build query using current location
        string queryString = Overpass_URL_String + "?data=" + CreateQueryString(info.bounds);

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(queryString);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Stream dataStream = response.GetResponseStream();

            // Open the stream using a StreamReader for easy access. 
            StreamReader reader = new StreamReader(dataStream);

            // Read the content.  
            string responseFromServer = reader.ReadToEnd();

            if(!String.IsNullOrEmpty(responseFromServer))
            {
                // Display the content.  
                Debug.Log(responseFromServer);
                Debug.Log("Overpass Manager: API Query successful, data retrieved");

                yield return JsonConvert.DeserializeObject<OSMData>(responseFromServer);
            }
            else
            {
                Debug.Log("Overpass API Query Error - Empty Response ");
                yield return null;
            }

            reader.Close();
        }
        else
        {
            Debug.Log("Overpass API Query Error - Bad Response: " + response.StatusCode);
            yield return null;
        }

        response.Close();
    }

    public string CreateQueryString(Region bounds)
    {
        string query = "[out:json];way[\"building\"](poly: \"" + bounds.ToString() + "\");out geom;relation[\"building\"](poly:\"" + bounds.ToString() + "\");out;way(r)[!\"building:part\"]; out geom;";

        return query;
    }
}
