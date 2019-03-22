using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class OverpassManager : MonoBehaviour
{
    //private readonly Uri Overpass_URI = new Uri("http://overpass-api.de/api/interpreter");
    private const string Overpass_URL_String = "http://overpass-api.de/api/interpreter";

    private const float BoundsHalfHeight = 0.0075f;
    private const float BoundsHalfWidth = 16.0f * BoundsHalfHeight / 9.0f;
    private float boundsScale = 1;

    //private OSMData data;
    //public OSMData Data { get { return data; } }

    //void Awake()
    //{
    //    DontDestroyOnLoad(this);
    //}

    //// Start is called before the first frame update
    //void Start()
    //{

    //    //testQuery = "data=[bbox:49.269905,-123.14807,49.293086,-123.09940];(node;rel(bn)->.x;way;node(w)->.x;rel(bw););out meta;";

    //    //testQuery = Overpass_URL_String + "?data=[bbox:49.269905,-123.14807,49.293086,-123.09940];(node;rel(bn)->.x;way;node(w)->.x;rel(bw););out meta;";

    //    //RunQuery();

    //    //float testLatitude = 49.22552f;
    //    //float testLongitude = -123.0064f;

    //    //Box bounds = CreateBoundingBoxFromCoordinate(testLatitude, testLongitude);
    //    //string queryString = Overpass_URL_String + "?data=" + CreateQueryString(bounds);
    //    //Debug.Log(queryString);
    //    //RunQuery(queryString);

    //    //// Start the HandleFile method.
    //    //Task<string> task = RunQuery();

    //    //Debug.Log("Running Query");

    //    //task.Wait();
    //    //var x = task.Result;
    //    //Debug.Log("Result:\n " + x);

    //    //Debug.Log("Finished Query");
    //}

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator RunQuery(Box bounds)
    {
        // build query using current location
        //Box bounds = CreateBoundingBoxFromCoordinate(location);
        string queryString = Overpass_URL_String + "?data=" + CreateQueryString(bounds);

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

                // parse JSON data
                //data = JsonConvert.DeserializeObject<OSMData>(responseFromServer);

                //if(data != null)
                //{
                //    yield return data;
                //}
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

    //public string CreateTestQuery(Coordinate min, Coordinate max)
    //{
    //    //this(apiURL, "[bbox:" + min.lat + "," + min.lon + "," + max.lat + "," + max.lon + "];(node;rel(bn)->.x;way;node(w)->.x;rel(bw););out meta;");
    //    return "";
    //}

    public string CreateQueryString(Box bounds)
    {
        string query = "[out:json];way[\"building\"](poly: \"" + bounds.ToString() + "\");out geom;relation[\"building\"](poly:\"" + bounds.ToString() + "\");out;way(r)[!\"building:part\"]; out geom;";

        return query;
    }

    public Box CreateBoundingBoxFromCoordinate(Coordinate location)
    {
        float latitude = location.latitude;
        float longitude = location.longitude;

        float halfWidth = BoundsHalfWidth * boundsScale;
        float halfHeight = BoundsHalfHeight * boundsScale;

        Coordinate topLeft = new Coordinate(latitude + halfHeight, longitude + halfWidth);
        Coordinate topRight = new Coordinate(latitude + halfHeight, longitude - halfWidth);
        Coordinate bottomRight = new Coordinate(latitude - halfHeight, longitude - halfWidth);
        Coordinate bottomLeft = new Coordinate(latitude - halfHeight, longitude + halfWidth);

        return new Box(topLeft, topRight, bottomRight, bottomLeft);
    }

}