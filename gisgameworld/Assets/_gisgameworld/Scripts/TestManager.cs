using UnityEngine;
using System.Collections;
using System;

public class TestManager
{
    private GameManager manager = null;

    public TestManager(GameManager manager)
    {
        this.manager = manager;
    }

    public void TakeTestScreenshot()
    {
        string id = DateTime.Now.ToString(@"MM-dd-yyyy_hh-mm-ss_tt");

        string filename = "TestScreenshot_" + id + ".png";

        ScreenCapture.CaptureScreenshot(filename, 4);
    }
}
