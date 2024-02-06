
#if UNITY_EDITOR

using FoodForTheGods.Framework.Attributes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FoodForTheGods.Editor.PropertyDraw
{
	// taken from https://stackoverflow.com/a/76895716
	[CustomPropertyDrawer(typeof(ScriptableObjectIdAttribute))]
	public class ScriptableObjectIdDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;

			Object owner = property.serializedObject.targetObject;

			// This is the unity managed GUID of the scriptable object, which is always unique :3
			string unityManagedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(owner));

			if (property.stringValue != unityManagedGuid)
			{
				property.stringValue = unityManagedGuid;
			}

			EditorGUI.PropertyField(position, property, label, true);

			GUI.enabled = true;
		}
	}
}
#endif