using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "DebugText", menuName = "ScriptableObjects/Debug/Text", order = 1)]
public class DebugScriptableObject : ScriptableObject
{
    public string generalDebugMessage;
}
