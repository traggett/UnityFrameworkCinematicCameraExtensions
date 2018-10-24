using Framework.Utils.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(CinematicCameraShot), true)]
			public class CinematicCameraShotInspector : UnityEditor.Editor
			{
				private SerializedProperty _fieldOfViewProperty;
				private SerializedProperty _cameraRectProperty;
				private SerializedProperty _focusInfoProperty;

				private SerializedProperty _shotTypeProperty;
				private SerializedProperty _shotModifiersProperty;

				private SerializedProperty _autoPanFlagsProperty;
				private SerializedProperty _autoPanStyleProperty;
				private SerializedProperty _autoPanEaseProperty;
				private SerializedProperty _autoPanTranslationProperty;
				private SerializedProperty _autoRotateTypeProperty;
				private SerializedProperty _autoRotateAngleProperty;
				private SerializedProperty _autoRotateLocalPointProperty;
				private SerializedProperty _autoZoomStyleProperty;
				private SerializedProperty _autoZoomAmountProperty;

				private SerializedProperty _previewCameraProperty;
				private SerializedProperty _previewDurationProperty;
				private SerializedProperty _previewClipExtrapolationProperty;

				private static readonly float kMouseLookSpeed = 82.0f;
				private static readonly float kMovementSpeed = 0.075f;
				private static readonly float kFastMovementSpeed = 0.16f;
				private static readonly bool[] _mouseDown = new bool[3];
				private static float kAspectRatio = 16f / 9f;
				private static CinematicCameraState _originalCameraState;
				private static bool _preview;
				private static CinematicCameraShotInspector _previewShot;
				private static RenderTexture _targetTexture;
				private static Dictionary<KeyCode, bool> _buttonPressed = new Dictionary<KeyCode, bool>();
				private static float _previewClipTime;

				#region Editor Calls
				private void OnEnable()
				{
					_fieldOfViewProperty = serializedObject.FindProperty("_fieldOfView");
					_cameraRectProperty = serializedObject.FindProperty("_cameraRect");
					_focusInfoProperty = serializedObject.FindProperty("_focusInfo");

					_shotTypeProperty = serializedObject.FindProperty("_shotType");
					_shotModifiersProperty = serializedObject.FindProperty("_cinematicCameraShotModifiers");

					_autoPanFlagsProperty = serializedObject.FindProperty("_autoPanFlags");
					_autoPanStyleProperty = serializedObject.FindProperty("_autoPanStyle");
					_autoPanEaseProperty = serializedObject.FindProperty("_autoPanEase");
					_autoPanTranslationProperty = serializedObject.FindProperty("_autoPanTranslation");
					_autoRotateTypeProperty = serializedObject.FindProperty("_autoRotateType");
					_autoRotateAngleProperty = serializedObject.FindProperty("_autoRotateAngle");
					_autoRotateLocalPointProperty = serializedObject.FindProperty("_autoRotateLocalPoint");
					_autoZoomStyleProperty = serializedObject.FindProperty("_autoZoomStyle");
					_autoZoomAmountProperty = serializedObject.FindProperty("_autoZoomAmount");

					_previewCameraProperty = serializedObject.FindProperty("_previewCamera");
					_previewDurationProperty = serializedObject.FindProperty("_previewClipDuration");
					_previewClipExtrapolationProperty = serializedObject.FindProperty("_previewClipExtrapolation");
				}

				private void OnDisable()
				{
					StopPreviewing();
				}

				private void OnDestroy()
				{
					StopPreviewing();
				}

				public override void OnInspectorGUI()
				{
					EditorGUILayout.LabelField("<b>Camera Properties</b>", EditorUtils.InspectorSubHeaderStyle);
					EditorGUILayout.Separator();

					EditorGUILayout.PropertyField(_fieldOfViewProperty);
					EditorGUILayout.PropertyField(_cameraRectProperty);
					EditorGUILayout.PropertyField(_focusInfoProperty);

					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("<b>Shot Properties</b>", EditorUtils.InspectorSubHeaderStyle);
					EditorGUILayout.Separator();
					
					EditorGUILayout.PropertyField(_shotTypeProperty);

					CinematicCameraShot.eShotType shotType = (CinematicCameraShot.eShotType )_shotTypeProperty.enumValueIndex;

					switch (shotType)
					{
						case CinematicCameraShot.eShotType.AutoPan:
							DrawAutoPanProperties();
							break;
						case CinematicCameraShot.eShotType.Rail:
							DrawRailCamProperties();
							break;
					}

					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("<b>Modifiers</b>", EditorUtils.InspectorSubHeaderStyle);
					EditorGUILayout.Separator();

					EditorGUILayout.PropertyField(_shotModifiersProperty, true);

					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("<b>Editor Preview</b>", EditorUtils.InspectorSubHeaderStyle);
					EditorGUILayout.Separator();
					CinematicCamera oldPreviewCamera = (CinematicCamera)_previewCameraProperty.objectReferenceValue;
					CinematicCamera previewCamera = oldPreviewCamera;

					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(_previewCameraProperty);
					if (EditorGUI.EndChangeCheck())
					{
						//If changed to new camera, set old cameras state back to orig
						if (oldPreviewCamera != null && _preview)
						{
							oldPreviewCamera.SetState(_originalCameraState);
						}
						_preview = false;

						previewCamera = (CinematicCamera)_previewCameraProperty.objectReferenceValue;
					}

					EditorGUILayout.PropertyField(_previewDurationProperty);
					EditorGUILayout.PropertyField(_previewClipExtrapolationProperty);

					if (previewCamera != null)
					{
						EditorGUILayout.Separator();
						if (GUILayout.Button("Preview Shot", _preview ? EditorUtils.ToggleButtonToggledStyle : EditorUtils.ToggleButtonStyle, GUILayout.ExpandWidth(false)))
						{
							if (!_preview)
							{
								StartPreviewing(this);
							}
							else
							{
								StopPreviewing();
							}
						}
						EditorGUILayout.Separator();
					}
					else
					{
						_preview = false;
					}

					serializedObject.ApplyModifiedProperties();
				}

				public virtual void OnSceneGUI()
				{
					if (_preview && Event.current != null)
					{
						switch (Event.current.type)
						{
							case EventType.Repaint: RenderGamePreview(Event.current); break;
							case EventType.KeyDown: HandleKeyDown(Event.current); break;
							case EventType.KeyUp: HandleKeyUp(Event.current); break;
							case EventType.MouseDown: HandleMouseDown(Event.current); break;
							case EventType.MouseUp: HandleMouseUp(Event.current); break;
							case EventType.MouseDrag: HandleMouseDrag(Event.current); break;
						}
					}
				}
				#endregion

				private static void StartPreviewing(CinematicCameraShotInspector inspector)
				{
					if (!_preview)
					{
						CinematicCamera previewCamera = (CinematicCamera)inspector._previewCameraProperty.objectReferenceValue;
						_originalCameraState = previewCamera.GetState();
						EditorApplication.update += UpdateKeys;
						EditorSceneManager.sceneSaving += OnSaveScene;
						_preview = true;
						_previewShot = inspector;
						_previewClipTime = 0.0f;
					}
				}

				private static void StopPreviewing()
				{
					if (_preview)
					{
						EditorApplication.update -= UpdateKeys;

						CinematicCamera previewCamera = (CinematicCamera)_previewShot._previewCameraProperty.objectReferenceValue;
						previewCamera.SetState(_originalCameraState);

						_preview = false;
						_previewShot = null;
					}
				}

				private void RenderGamePreview(Event evnt)
				{
					CinematicCamera previewCamera = (CinematicCamera)_previewCameraProperty.objectReferenceValue;

					if (_preview && previewCamera != null)
					{
						CinematicCameraShot shot = (CinematicCameraShot)target;

						float clipPosition = CinematicCameraMixer.GetClipPosition(shot._previewClipExtrapolation, _previewClipTime, shot._previewClipDuration);
						previewCamera.SetState(shot.GetState(clipPosition));

						Rect sceneViewRect = Camera.current.pixelRect;
						int viewWidth = (int)sceneViewRect.width;
						int viewHeight = (int)sceneViewRect.height;

						//If at this height the width is to big, need to make height less
						if (viewHeight * kAspectRatio > viewWidth)
						{
							viewHeight = (int)(sceneViewRect.width * (1f / kAspectRatio));
						}
						//If at this height the height is to big, need to make width less
						if (viewWidth * (1f / kAspectRatio) > viewHeight)
						{
							viewWidth = (int)(sceneViewRect.height * kAspectRatio);
						}

						if (_targetTexture == null || viewWidth != _targetTexture.width || viewHeight != _targetTexture.height)
						{
							if (_targetTexture == null)
							{
								_targetTexture = new RenderTexture(viewWidth, viewHeight, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
							}
							else
							{
								_targetTexture.Release();
							}

							_targetTexture.width = viewWidth;
							_targetTexture.height = viewHeight;
							_targetTexture.antiAliasing = 1;
							_targetTexture.Create();
						}

						if (_targetTexture.IsCreated())
						{
							//Render scene - need better way of grabbing post fx
							RenderTexture active = RenderTexture.active;
							RenderTexture.active = _targetTexture;
							GL.Clear(true, true, Color.clear);
							RenderTexture.active = active;

							RenderCameras();

							//Render on screen
							GUI.BeginGroup(sceneViewRect);

							//Clear screen to black
							Color guiColor = GUI.color;
							GUI.color = Color.black;
							GUI.DrawTexture(sceneViewRect, EditorUtils.OnePixelTexture);
							GUI.color = guiColor;

							//Render game texture to screen
							Rect viewRect = new Rect
							{
								width = viewWidth,
								height = viewHeight
							};
							viewRect.x = (sceneViewRect.width - viewRect.width) * 0.5f;
							viewRect.y = (sceneViewRect.height - viewRect.height) * 0.5f;
							GUI.DrawTexture(GetFlippedRect(viewRect), _targetTexture, ScaleMode.StretchToFill, false);

							GUI.EndGroup();
						}
					}
				}

				private void RenderCameras()
				{
					foreach (Camera camera in Camera.allCameras)
					{
						RenderTexture texture = camera.targetTexture;
						camera.targetTexture = _targetTexture;
						camera.Render();
						camera.targetTexture = texture;
					}
				}

				private static Rect GetFlippedRect(Rect rect)
				{
					if (SystemInfo.graphicsUVStartsAtTop)
					{
						return rect;
					}
					else
					{
						Rect targetInView = rect;
						targetInView.y += targetInView.height;
						targetInView.height = -targetInView.height;
						return targetInView;
					}
				}

				private void HandleMouseDrag(Event evnt)
				{
					if (InFreeCamMode())
					{
						CinematicCameraShot cameraShot = (CinematicCameraShot)target;
						Vector3 eulerAngles = cameraShot.transform.eulerAngles;
						eulerAngles += new Vector3(evnt.delta.y / Camera.current.pixelRect.height, evnt.delta.x / Camera.current.pixelRect.width, 0.0f) * kMouseLookSpeed;
						cameraShot.transform.eulerAngles = eulerAngles;
					}

					Event.current.Use();
				}

				private static void HandleKeyDown(Event evnt)
				{
					_buttonPressed[evnt.keyCode] = true;
					evnt.Use();
				}

				private static void HandleKeyUp(Event evnt)
				{
					_buttonPressed[evnt.keyCode] = false;
					evnt.Use();
				}

				private static void HandleMouseDown(Event evnt)
				{
					_mouseDown[evnt.button] = true;
					evnt.Use();
				}

				private static void HandleMouseUp(Event evnt)
				{
					_mouseDown[evnt.button] = false;
					evnt.Use();
				}

				private static bool InFreeCamMode()
				{
					return _preview && _previewShot != null && _mouseDown[1];
				}

				private static void UpdateKeys()
				{
					_previewClipTime += Time.deltaTime;

					if (InFreeCamMode())
					{
						Vector3 movement = Vector3.zero;
						bool held = false;

						if (_buttonPressed.TryGetValue(KeyCode.A, out held) && held)
						{
							movement.x -= 1.0f;
						}
						if (_buttonPressed.TryGetValue(KeyCode.D, out held) && held)
						{
							movement.x += 1.0f;
						}
						if (_buttonPressed.TryGetValue(KeyCode.W, out held) && held)
						{
							movement.z += 1.0f;
						}
						if (_buttonPressed.TryGetValue(KeyCode.S, out held) && held)
						{
							movement.z -= 1.0f;
						}
						if (_buttonPressed.TryGetValue(KeyCode.R, out held) && held)
						{
							movement.y += 1.0f;
						}
						if (_buttonPressed.TryGetValue(KeyCode.F, out held) && held)
						{
							movement.y -= 1.0f;

						}

						CinematicCameraShot cameraShot = (CinematicCameraShot)_previewShot.target;

						float speed = kMovementSpeed;

						if (_buttonPressed.TryGetValue(KeyCode.Space, out held) && held)
							speed = kFastMovementSpeed;

						cameraShot.transform.Translate(movement * speed, Space.Self);
					}
				}

				private static void OnSaveScene(Scene scene, string path)
				{
					StopPreviewing();
				}

				private void DrawAutoPanProperties()
				{
					EditorGUILayout.PropertyField(_autoPanStyleProperty);
					EditorGUILayout.PropertyField(_autoPanEaseProperty);

					CinematicCameraShot.eAutoPanFlags autoPanFlags = (CinematicCameraShot.eAutoPanFlags)EditorGUILayout.EnumFlagsField("Auto Pan Flags", (CinematicCameraShot.eAutoPanFlags)_autoPanFlagsProperty.intValue);
					_autoPanFlagsProperty.intValue = (int)autoPanFlags;

					if ((autoPanFlags & CinematicCameraShot.eAutoPanFlags.Translate) == CinematicCameraShot.eAutoPanFlags.Translate)
					{
						EditorGUILayout.LabelField("Translation", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(_autoPanTranslationProperty);
					}

					if ((autoPanFlags & CinematicCameraShot.eAutoPanFlags.Rotate) == CinematicCameraShot.eAutoPanFlags.Rotate)
					{
						EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(_autoRotateTypeProperty);
						EditorGUILayout.PropertyField(_autoRotateAngleProperty);

						CinematicCameraShot.eAutoRotateType rotateType = (CinematicCameraShot.eAutoRotateType)_autoRotateTypeProperty.enumValueIndex;

						switch (rotateType)
						{
							case CinematicCameraShot.eAutoRotateType.RotateAroundLocalPoint:
								{
									EditorGUILayout.PropertyField(_autoRotateLocalPointProperty, new GUIContent("Auto Rotate Local Pivot"));
								}
								break;
							case CinematicCameraShot.eAutoRotateType.RotateAroundAxis:
								{
									EditorUtils.AxisPropertyField(_autoRotateLocalPointProperty, new GUIContent("Auto Rotate Axis"));
								}
								break;
						}
					}

					if ((autoPanFlags & CinematicCameraShot.eAutoPanFlags.Zoom) == CinematicCameraShot.eAutoPanFlags.Zoom)
					{
						EditorGUILayout.LabelField("Zoom", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(_autoZoomStyleProperty);
						EditorGUILayout.PropertyField(_autoZoomAmountProperty);
					}
				}

				private void DrawRailCamProperties()
				{

				}
			}
		}
	}
}