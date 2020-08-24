using UnityEngine;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;

public class AotTypeEnforcer : MonoBehaviour
{
    public void Awake()
    {
        AotHelper.EnsureList<string>();
        AotHelper.EnsureList<float>();
        AotHelper.EnsureList<Color32>();
        AotHelper.EnsureList<Color>();
        AotHelper.EnsureList<Vector4>();
        AotHelper.EnsureList<Vector3>();
        AotHelper.EnsureList<Vector2>();
        AotHelper.EnsureList<Matrix4x4>();
        AotHelper.EnsureList<BoneWeight>();
        AotHelper.EnsureList<Building>();
        AotHelper.EnsureList<Shape>();
        AotHelper.EnsureDictionary<string, List<Shape>>();
    }
}
