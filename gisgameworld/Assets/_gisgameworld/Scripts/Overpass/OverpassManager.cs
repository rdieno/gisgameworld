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
    public static OverpassManager instance;

    private readonly Uri Overpass_URI = new Uri("http://overpass-api.de/api/interpreter");
    private const string Overpass_URL_String = "http://overpass-api.de/api/interpreter";

    private const float BoundsHalfHeight = 0.0075f;
    private const float BoundsHalfWidth = 16.0f * BoundsHalfHeight / 9.0f;
    private float boundsScale = 1;

    private string testQuery;
    //private readonly string testQuery = "(
    //    node(51.249,7.148,51.251,7.152);
    //    <;
    //    );
    //    out meta;
    //    ";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {

        //testQuery = "data=[bbox:49.269905,-123.14807,49.293086,-123.09940];(node;rel(bn)->.x;way;node(w)->.x;rel(bw););out meta;";

        testQuery = Overpass_URL_String + "?data=[bbox:49.269905,-123.14807,49.293086,-123.09940];(node;rel(bn)->.x;way;node(w)->.x;rel(bw););out meta;";

        //RunQuery();

        float testLatitude = 49.22552f;
        float testLongitude = -123.0064f;

        Box bounds = CreateBoundingBoxFromCoordinate(testLatitude, testLongitude);
        string queryString = Overpass_URL_String + "?data=" + CreateQueryString(bounds);
        Debug.Log(queryString);
        RunQuery(queryString);

        //// Start the HandleFile method.
        //Task<string> task = RunQuery();

        //Debug.Log("Running Query");

        //task.Wait();
        //var x = task.Result;
        //Debug.Log("Result:\n " + x);

        //Debug.Log("Finished Query");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RunQuery(string queryString)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(queryString);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if(response.StatusCode == HttpStatusCode.OK)
            {
                Stream dataStream = response.GetResponseStream();

                // Open the stream using a StreamReader for easy access. 
                StreamReader reader = new StreamReader(dataStream);
                try
                {
                    // Read the content.  
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    Debug.Log(responseFromServer);

                    // parse JSON data
                    OSMData data = JsonConvert.DeserializeObject<OSMData>(responseFromServer);
                    int i = 0;
                }
                catch (Exception e)
                {
                    throw new Exception("Overpass API Query Error - Reading Response", e);
                }

                reader.Close();


                // XmlTextReader
            }
            else
            {
                Debug.Log("Overpass API Query Error - Bad Response: " + response.StatusCode);
            }

            response.Close();
        }
        catch (Exception e)
        {
            throw new Exception("Overpass API Query Error - Web Request", e);
        }
    }

    //public async Task<string> RunQuery(UInt32 Timeout = 0)
    //{

    //    if (Timeout > 0) { }
    //        //_QueryTimeout = Timeout;

    //    using (HttpClient HTTPclient = new HttpClient())
    //    {
    //        try
    //        {
    //            using (HttpResponseMessage ResponseMessage = await HTTPclient.PostAsync(Overpass_URI, new StringContent(testQuery)))
    //            {
    //                if (ResponseMessage.StatusCode == HttpStatusCode.OK)
    //                {
    //                    using (var ResponseContent = ResponseMessage.Content)
    //                    {
    //                        return await ResponseContent.ReadAsStringAsync();
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            throw new Exception("The OverpassQuery led to an error!", e);
    //        }

    //    }

    //    throw new Exception("General HTTP client error!");
    //}


    public string CreateTestQuery(Coordinate min, Coordinate max)
    {
        //this(apiURL, "[bbox:" + min.lat + "," + min.lon + "," + max.lat + "," + max.lon + "];(node;rel(bn)->.x;way;node(w)->.x;rel(bw););out meta;");
        return "";
    }

    public string CreateQueryString(Box bounds)
    {
        string query = "[out:json];way[\"building\"](poly: \"" + bounds.ToString() + "\");out geom;relation[\"building\"](poly:\"" + bounds.ToString() + "\");out;way(r)[!\"building:part\"]; out geom;";

        return query;
    }

    public Box CreateBoundingBoxFromCoordinate(float latitude, float longitude)
    {
        float halfWidth = BoundsHalfWidth * boundsScale;
        float halfHeight = BoundsHalfHeight * boundsScale;

        Coordinate topLeft = new Coordinate(latitude + halfHeight, longitude + halfWidth);
        Coordinate topRight = new Coordinate(latitude + halfHeight, longitude - halfWidth);
        Coordinate bottomRight = new Coordinate(latitude - halfHeight, longitude - halfWidth);
        Coordinate bottomLeft = new Coordinate(latitude - halfHeight, longitude + halfWidth);

        return new Box(topLeft, topRight, bottomRight, bottomLeft);
    }

}