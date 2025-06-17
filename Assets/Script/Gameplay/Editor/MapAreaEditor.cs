using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var totalChanceInGrass = serializedObject.FindProperty("totalChance").intValue;
        var totalChanceInWater = serializedObject.FindProperty("totalChanceWater").intValue;
        var totalChanceInSand = serializedObject.FindProperty("totalChanceSand").intValue;

        // if (totalChanceInGrass != 100 && totalChanceInGrass != -1)
        //     EditorGUILayout.HelpBox($"The total chance percentage in grass is {totalChanceInGrass}, not 100.", MessageType.Error);

        // if (totalChanceInWater != 100 && totalChanceInWater != -1)
        //     EditorGUILayout.HelpBox($"The total chance percentage in water is {totalChanceInWater}, not 100.", MessageType.Error);

        // if (totalChanceInSand != 100 && totalChanceInSand != -1)
        //     EditorGUILayout.HelpBox($"The total chance percentage in sand is {totalChanceInSand}, not 100.", MessageType.Error);
    }
}
