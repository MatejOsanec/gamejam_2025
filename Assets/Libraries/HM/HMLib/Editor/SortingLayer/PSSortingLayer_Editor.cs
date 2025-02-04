using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(PSSortingLayer))]
public class PSSortingLayer_Editor : Editor
{

	public override void OnInspectorGUI() {
		ParticleSystem particleSystem = ((PSSortingLayer)target).gameObject.GetComponent<ParticleSystem>();
		DrawGUI(particleSystem.GetComponent<Renderer>());
	}

	public static void DrawGUI(Renderer renderer)
	{
		// get the list of layers
		string[] layerNames = GetSortingLayerNames();
		
		// get the index of the current sorting layer of the renderer
		int index = 0;
		for (int i = 0; i < layerNames.Length; i ++)
		{
			if (renderer.sortingLayerName == layerNames[i])
			{
				index = i;
				break;
			}
		}
		
		// show the sorting layer names
		int nextIndex;
		nextIndex = EditorGUILayout.Popup ("Sorting Layer", index, layerNames);
		
		// change sorting layer
		if (nextIndex != index)
		{
			Undo.RecordObject(renderer, "Changed Sorting Layer Name");
			string nextLayerName = layerNames[nextIndex];
			renderer.sortingLayerName = nextLayerName;
			EditorUtility.SetDirty(renderer);
		}
		
		// sorting order
		int nextLayerOrder = EditorGUILayout.IntField("Order in Layer", renderer.sortingOrder);
		if (nextLayerOrder != renderer.sortingOrder)
		{
			Undo.RecordObject(renderer, "Edit Sorting Order");
			renderer.sortingOrder = nextLayerOrder;
			EditorUtility.SetDirty(renderer);
		}
	}
	
	// these methods taken from here: http://answers.unity3d.com/questions/585108/how-do-you-access-sorting-layers-via-scripting.html
	// Get the sorting layer names
	public static string[] GetSortingLayerNames()
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null, new object[0]);
	}
	
	// Get the unique sorting layer IDs
	public static int[] GetSortingLayerUniqueIDs()
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
	}
	
}
