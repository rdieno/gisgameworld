using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocationService
{
    public IEnumerator GetLocation(int timeout = 20)
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location service is disabled on device");
            yield return null;
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = timeout;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield return null;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed || Input.location.status == LocationServiceStatus.Stopped)
        {
            Debug.Log("Unable to determine device location - Status: " + Input.location.status);
            yield return null;
        }
        else
        {
            // Access granted and location value could be retrieved
            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;

            Debug.Log("Location: " + latitude + " " + longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

            yield return new Coordinate(latitude, longitude);
        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }
}
