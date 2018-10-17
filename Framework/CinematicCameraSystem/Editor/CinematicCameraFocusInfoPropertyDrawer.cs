using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(CinematicCameraFocusInfo))]
			public class CinematicCameraFocusInfoPropertyDrawer : PropertyDrawer
			{
				#region Editor Calls
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					position.height = EditorGUIUtility.singleLineHeight;
					property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName);

					if (property.isExpanded)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						SerializedProperty focusPoint = property.FindPropertyRelative("_focusPointTarget");
						SerializedProperty focusDistance = property.FindPropertyRelative("_focusPointDistance");
						SerializedProperty fNumber = property.FindPropertyRelative("_focusPointFNumber");
						SerializedProperty focalLength = property.FindPropertyRelative("_focusPointFocalLength");

						position.y += EditorGUIUtility.singleLineHeight;
						EditorGUI.PropertyField(position, focusPoint);

						position.y += EditorGUIUtility.singleLineHeight;
						EditorGUI.PropertyField(position, focusDistance);

						//Focal length
						{
							position.y += EditorGUIUtility.singleLineHeight;
							EditorGUI.BeginChangeCheck();
							float f = focalLength.floatValue * 1000;
							f = EditorGUI.Slider(position, "Focal Length", f, 1.0f, 300.0f);
							if (EditorGUI.EndChangeCheck())
							{
								focalLength.floatValue = f / 1000;
							}
						}

						position.y += EditorGUIUtility.singleLineHeight;
						EditorGUI.PropertyField(position, fNumber);

						EditorGUI.indentLevel = origIndent;
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return property.isExpanded ? EditorGUIUtility.singleLineHeight * 5 : EditorGUIUtility.singleLineHeight;
				}
				#endregion
			}
		}
	}
}