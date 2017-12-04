using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace pirklaHeatMap
{
    [CustomEditor(typeof(HeatMap))]

    [System.Serializable]
    public class HeatMapEditor : Editor
    {


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            HeatMap script = (HeatMap)target;

            if (GUILayout.Button("Hide Boxes"))
            {
                script.HideBoxes();
            }
            if (GUILayout.Button("Show Boxes"))
            {
                script.ShowBoxes();
            }
            if (GUILayout.Button("CreateGrid"))
            {
                script.GenerateGrid();
            }
            if (GUILayout.Button("DestroyGrid"))
            {
                script.RemoveGrid();
            }
            if (script.IsLoading)
            {
                EditorGUILayout.LabelField("Loading:", script.LoadingPercentage.ToString());
                SceneView.RepaintAll();
            }
        }
    }
}