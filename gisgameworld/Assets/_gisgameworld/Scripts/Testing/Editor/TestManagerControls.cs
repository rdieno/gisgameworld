using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(TestManager))]
public class TestManagerControls : Editor
{
    int index = 0;
    bool useLiveFootprint = false;
    bool colorGeometry = false;
    bool takeScreenshot = false;
    string ruleset = "";


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TestManager tm = target as TestManager;

        GUILayout.Label("Ruleset");

        ruleset = GUILayout.TextField(ruleset, 50);

        GUILayout.Space(15);

        useLiveFootprint = GUILayout.Toggle(useLiveFootprint, "Use Live Footprint");

        index = (int)GUILayout.HorizontalSlider(index, 0f, 400f, null);

        GUILayout.TextField((string)index.ToString(), 4);

        GUILayout.Space(15);

        colorGeometry = GUILayout.Toggle(colorGeometry, "Color Geometry");

        GUILayout.Space(15);

        takeScreenshot = GUILayout.Toggle(takeScreenshot, "Take Screenshot");

        GUILayout.Space(15);

        if (GUILayout.Button("Create Building"))
        {
            //tm.Manager.StartCoroutine(tm.CreateReportScreenshots());
            if(ruleset != string.Empty)
            {
                tm.Manager.StartCoroutine(tm.CreateOperationDemoScreenshot(ruleset, colorGeometry, useLiveFootprint, index, takeScreenshot));
            }

            
        }

        if(GUILayout.Button("Take Screenshot"))
        {
            if (ruleset != string.Empty)
            {
                tm.Manager.StartCoroutine(tm.TakeScreenshotOfCurrentView(ruleset));
            }
        }

    }

}
