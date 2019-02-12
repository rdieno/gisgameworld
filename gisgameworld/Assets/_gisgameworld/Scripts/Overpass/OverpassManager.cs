using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;



public class OverpassManager : MonoBehaviour
{
    private readonly Uri Overpass_URI = new Uri("http://overpass-api.de/api/interpreter");

    //private readonly string testQuery = "(
    //    node(51.249,7.148,51.251,7.152);
    //    <;
    //    );
    //    out meta;
    //    ";

    // Start is called before the first frame update
    void Start()
    {
        // Start the HandleFile method.
        Task<string> task = RunQuery();

        Debug.Log("Running Query");

        task.Wait();
        var x = task.Result;
        Debug.Log("Result:\n " + x);

        Debug.Log("Finished Query");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async Task<string> RunQuery(UInt32 Timeout = 0)
    {

        if (Timeout > 0)
            //_QueryTimeout = Timeout;

        using (var HTTPClient = new HttpClient())
        {
            try
            {
                using (var ResponseMessage = await HTTPClient.PostAsync(Overpass_URI, new StringContent(this.ToString())))
                {
                    if (ResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        using (var ResponseContent = ResponseMessage.Content)
                        {
                            return await ResponseContent.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("The OverpassQuery led to an error!", e);
            }

        }

        throw new Exception("General HTTP client error!");
    }
}