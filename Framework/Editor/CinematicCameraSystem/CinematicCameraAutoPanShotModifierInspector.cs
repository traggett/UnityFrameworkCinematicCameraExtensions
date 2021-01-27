using Framework.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(CinematicCameraAutoPanShotModifier), true)]
			public class CinematicCameraAutoPanShotModifierInspector : UnityEditor.Editor
			{
				private SerializedProperty _autoPanFlagsProperty;
				private SerializedProperty _durationProperty;
				private SerializedProperty _autoPanStyleProperty;
				private SerializedProperty _autoPanEaseProperty;
				private SerializedProperty _autoPanTranslationProperty;
				private SerializedProperty _autoRotateTypeProperty;
				private SerializedProperty _autoRotateAngleProperty;
				private SerializedProperty _autoRotateLocalPointProperty;
				private SerializedProperty _autoZoomStyleProperty;
				private SerializedProperty _autoZoomAmountProperty;
				private SerializedProperty _previewClipPosProperty;

				#region Editor Calls
				private void OnEnable()
				{
					_autoPanFlagsProperty = serializedObject.FindProperty("_autoPanFlags");
					_durationProperty = serializedObject.FindProperty("_panDuration");
					_autoPanStyleProperty = serializedObject.FindProperty("_autoPanStyle");
					_autoPanEaseProperty = serializedObject.FindProperty("_autoPanEase");
					_autoPanTranslationProperty = serializedObject.FindProperty("_autoPanTranslation");
					_autoRotateTypeProperty = serializedObject.FindProperty("_autoRotateType");
					_autoRotateAngleProperty = serializedObject.FindProperty("_autoRotateAngle");
					_autoRotateLocalPointProperty = serializedObject.FindProperty("_autoRotateLocalPoint");
					_autoZoomStyleProperty = serializedObject.FindProperty("_autoZoomStyle");
					_autoZoomAmountProperty = serializedObject.FindProperty("_autoZoomAmount");
					_previewClipPosProperty = serializedObject.FindProperty("_previewClipPos");
				}

				public override void OnInspectorGUI()
				{
					EditorGUILayout.PropertyField(_autoPanStyleProperty);
					EditorGUILayout.PropertyField(_durationProperty);
					EditorGUILayout.PropertyField(_autoPanEaseProperty);

					CinematicCameraAutoPanShotModifier.AutoPanFlags autoPanFlags = (CinematicCameraAutoPanShotModifier.AutoPanFlags)EditorGUILayout.EnumFlagsField("Auto Pan Flags", (CinematicCameraAutoPanShotModifier.AutoPanFlags)_autoPanFlagsProperty.intValue);
					_autoPanFlagsProperty.intValue = (int)autoPanFlags;

					if ((autoPanFlags & CinematicCameraAutoPanShotModifier.AutoPanFlags.Translate) == CinematicCameraAutoPanShotModifier.AutoPanFlags.Translate)
					{
						EditorGUILayout.LabelField("Translation", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(_autoPanTranslationProperty);
					}

					if ((autoPanFlags & CinematicCameraAutoPanShotModifier.AutoPanFlags.Rotate) == CinematicCameraAutoPanShotModifier.AutoPanFlags.Rotate)
					{
						EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(_autoRotateTypeProperty);
						EditorGUILayout.PropertyField(_autoRotateAngleProperty);

						CinematicCameraAutoPanShotModifier.AutoRotateType rotateType = (CinematicCameraAutoPanShotModifier.AutoRotateType)_autoRotateTypeProperty.enumValueIndex;

						switch (rotateType)
						{
							case CinematicCameraAutoPanShotModifier.AutoRotateType.RotateAroundLocalPoint:
								{
									EditorGUILayout.PropertyField(_autoRotateLocalPointProperty, new GUIContent("Auto Rotate Local Pivot"));
								}
								break;
							case CinematicCameraAutoPanShotModifier.AutoRotateType.RotateAroundAxis:
								{
									EditorUtils.AxisPropertyField(_autoRotateLocalPointProperty, new GUIContent("Auto Rotate Axis"));
								}
								break;
						}
					}

					if ((autoPanFlags & CinematicCameraAutoPanShotModifier.AutoPanFlags.Zoom) == CinematicCameraAutoPanShotModifier.AutoPanFlags.Zoom)
					{
						EditorGUILayout.LabelField("Zoom", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(_autoZoomStyleProperty);
						EditorGUILayout.PropertyField(_autoZoomAmountProperty);
					}

					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("<b>Editor Preview</b>", EditorUtils.InspectorSubHeaderStyle);
					EditorGUILayout.Separator();

					//if shot is currently being preview show clip pos
					 EditorGUILayout.Slider(_previewClipPosProperty, 0f, 1f);

					serializedObject.ApplyModifiedProperties();
				}
				#endregion

		
			}
		}
	}
}