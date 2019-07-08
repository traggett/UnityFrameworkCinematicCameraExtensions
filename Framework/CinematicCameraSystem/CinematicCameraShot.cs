using Framework.Maths;
using System;
using UnityEngine;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		public class CinematicCameraShot : MonoBehaviour
		{
			#region Public Data
			[Range(0.0001f, 180f)]
			public float _fieldOfView = 60.0f;
			public Rect _cameraRect = new Rect(0, 0, 1, 1);
			public CinematicCameraFocusInfo _focusInfo = CinematicCameraFocusInfo.kDefault;
			public CinematicCameraShotModifier[] _cinematicCameraShotModifiers;

			public enum eShotType
			{
				Static,
				AutoPan,
				Rail,
			}
			public eShotType _shotType;

			#region Auto Pan
			[Flags]
			public enum eAutoPanFlags
			{
				Translate = 1,
				Rotate = 2,
				Zoom = 4,
			}
			public eAutoPanFlags _autoPanFlags;

			public enum eAutoPanStyle
			{
				PanThrough,
				PanTo,
			}
			public eAutoPanStyle _autoPanStyle;
			public InterpolationType _autoPanEase;

			public Vector3 _autoPanTranslation;

			public enum eAutoRotateType
			{
				RotateAroundFocusPoint,
				RotateAroundLocalPoint,
				RotateAroundAxis,
			}
			public eAutoRotateType _autoRotateType;
			public float _autoRotateAngle;
			public Vector3 _autoRotateLocalPoint;

			public enum eAutoZoomStyle
			{
				PlainZoom,
				DollyZoom,
			}
			public eAutoZoomStyle _autoZoomStyle;
			public float _autoZoomAmount;
			#endregion

			#region Previewing
#if UNITY_EDITOR
			[HideInInspector]
			public CinematicCamera _previewCamera;
			[HideInInspector]
			public float _previewClipDuration;
			[HideInInspector]
			public eExtrapolation _previewClipExtrapolation;
#endif
			#endregion

			#endregion

			#region MonoBehaviour
			void Update()
			{
#if UNITY_EDITOR
				if (Application.isPlaying)
#endif
				{
					
				}
					
			}

#if UNITY_EDITOR
			void OnDrawGizmos()
			{
				Gizmos.DrawIcon(transform.position, "CameraShotIcon", true);
			}
#endif
			#endregion

			#region Public Functions
			public CinematicCameraState GetState(float clipPosition = 0.0f)
			{
				//Get default state
				CinematicCameraState state = new CinematicCameraState
				{
					_position = this.transform.position,
					_rotation = this.transform.rotation,
					_fieldOfView = this._fieldOfView,
					_cameraRect = this._cameraRect,
					_focusInfo = this._focusInfo
				};

				//Apply pan or rail
				switch (_shotType)
				{
					case eShotType.AutoPan:
						ApplyAutoPan(ref state, clipPosition);
						break;
					case eShotType.Rail:
						ApplyRailShot(ref state, clipPosition);
						break;
				}

				//Finally apply modifiers
				for (int i=0; i<_cinematicCameraShotModifiers.Length; i++)
				{
					_cinematicCameraShotModifiers[i].ModifiyState(ref state);
				}

				return state;
			}
			#endregion

			#region Private Functions
			private void ApplyAutoPan(ref CinematicCameraState state, float clipPosition)
			{
				if ((_autoPanFlags & eAutoPanFlags.Zoom) == eAutoPanFlags.Zoom)
				{
					ApplyAutoPanZoom(ref state, clipPosition);
				}

				if ((_autoPanFlags & eAutoPanFlags.Rotate) == eAutoPanFlags.Rotate)
				{
					ApplyAutoPanRotation(ref state, clipPosition);
				}

				if ((_autoPanFlags & eAutoPanFlags.Translate) == eAutoPanFlags.Translate)
				{
					ApplyAutoPanTranslation(ref state, clipPosition);
				}
			}

			private void ApplyAutoPanZoom(ref CinematicCameraState state, float clipPosition)
			{
				float fieldOfView = state._fieldOfView + MathUtils.Interpolate(_autoPanEase, _autoZoomAmount, _autoPanStyle == eAutoPanStyle.PanThrough ? -_autoZoomAmount : 0.0f, clipPosition);

				switch (_autoZoomStyle)
				{
					case eAutoZoomStyle.DollyZoom:
						{
							//From here https://en.wikipedia.org/wiki/Dolly_zoom
							// distance =  fixedWidth / 2tan(FoV*0.5)
							// width = distance * 2tan(fov*0.5)
							float focusDistance = state._focusInfo.GetFocusDistance(state._position);
							float frustrumHeightAtDistance = focusDistance * 2.0f * Mathf.Tan(state._fieldOfView * 0.5f * Mathf.Deg2Rad);

							//Now need to work out need camera distance to target
							float distance = frustrumHeightAtDistance / (2.0f * Mathf.Tan(fieldOfView * Mathf.Deg2Rad * 0.5f));

							//Then move camera back so its now this far away
							float neededMovement = distance - focusDistance;
							state._position -= state._rotation * Vector3.forward * neededMovement;
							//update field of view
							state._fieldOfView = fieldOfView;
							
							//Also need to update focus data
							state._focusInfo._focusPointDistance += neededMovement;
							//Also need to update focus width???
							state._focusInfo._focusPointFNumber += neededMovement * 0.01f;
						}
						break;
					case eAutoZoomStyle.PlainZoom:
						{
							state._fieldOfView = fieldOfView;
						}
						break;
				}
			}

			private void ApplyAutoPanTranslation(ref CinematicCameraState state, float clipPosition)
			{
				Vector3 worldSpaceTranslate = this.transform.TransformPoint(_autoPanTranslation);
				Vector3 movement = worldSpaceTranslate - this.transform.position;

				state._position += MathUtils.Interpolate(_autoPanEase, movement, _autoPanStyle == eAutoPanStyle.PanThrough ? -movement : Vector3.zero, clipPosition);
			}

			private void ApplyAutoPanRotation(ref CinematicCameraState state, float clipPosition)
			{
				float angle = MathUtils.Interpolate(_autoPanEase, _autoRotateAngle, _autoPanStyle == eAutoPanStyle.PanThrough ? -_autoRotateAngle : 0.0f, clipPosition);

				switch (_autoRotateType)
				{
					case eAutoRotateType.RotateAroundFocusPoint:
						{
							if (_focusInfo._focusPointTarget != null)
							{
								Vector3 pivotPoint = _focusInfo._focusPointTarget.position;
								Vector3 toPivot = this.transform.position - pivotPoint;

								if (toPivot.sqrMagnitude > 0.0f)
								{
									Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);

									state._position = pivotPoint + (rotation * toPivot);
									state._rotation *= rotation;
								}
							}
						}
						break;
					case eAutoRotateType.RotateAroundLocalPoint:
						{
							Vector3 pivotPoint = this.transform.TransformPoint(_autoRotateLocalPoint);
							Vector3 toPivot = this.transform.position - pivotPoint;

							if (toPivot.sqrMagnitude > 0.0f)
							{
								Vector3 axis = Vector3.Cross(toPivot, this.transform.right);
								Quaternion rotation = Quaternion.AngleAxis(angle, axis);

								state._position = pivotPoint + (rotation * toPivot);
								state._rotation *= rotation;
							}
						}
						break;
					case eAutoRotateType.RotateAroundAxis:
					default:
						{
							Vector3 axis = _autoRotateLocalPoint;
							Quaternion rotation = Quaternion.AngleAxis(angle, axis);

							state._rotation *= rotation;
						}
						break;
				}
			}

			private void ApplyRailShot(ref CinematicCameraState state, float clipPosition)
			{
				
			}
			#endregion
		}
	}
}