using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	
	namespace CinematicCameraSystem
	{
		public class CinematicCameraAutoPanShotModifier : CinematicCameraShotModifier
		{
			#region Public Data
			[Flags]
			public enum AutoPanFlags
			{
				Translate = 1,
				Rotate = 2,
				Zoom = 4,
			}
			public AutoPanFlags _autoPanFlags;

			public enum eAutoPanStyle
			{
				PanThrough,
				PanTo,
			}
			public eAutoPanStyle _autoPanStyle;
			public InterpolationType _autoPanEase;

			public Vector3 _autoPanTranslation;

			public enum AutoRotateType
			{
				RotateAroundFocusPoint,
				RotateAroundLocalPoint,
				RotateAroundAxis,
			}
			public AutoRotateType _autoRotateType;
			public float _autoRotateAngle;
			public Vector3 _autoRotateLocalPoint;

			public enum AutoZoomStyle
			{
				PlainZoom,
				DollyZoom,
			}
			public AutoZoomStyle _autoZoomStyle;
			public float _autoZoomAmount;
			#endregion

			#region Public Functions
			public override void ModifiyState(ref CinematicCameraState state, float clipPosition)
			{
				ApplyAutoPan(ref state, clipPosition);
			}
			#endregion

			#region Private Functions
			private void ApplyAutoPan(ref CinematicCameraState state, float clipPosition)
			{
				if ((_autoPanFlags & AutoPanFlags.Zoom) == AutoPanFlags.Zoom)
				{
					ApplyAutoPanZoom(ref state, clipPosition);
				}

				if ((_autoPanFlags & AutoPanFlags.Rotate) == AutoPanFlags.Rotate)
				{
					ApplyAutoPanRotation(ref state, clipPosition);
				}

				if ((_autoPanFlags & AutoPanFlags.Translate) == AutoPanFlags.Translate)
				{
					ApplyAutoPanTranslation(ref state, clipPosition);
				}
			}

			private void ApplyAutoPanZoom(ref CinematicCameraState state, float clipPosition)
			{
				float fieldOfView = state._fieldOfView + MathUtils.Interpolate(_autoPanEase, _autoZoomAmount, _autoPanStyle == eAutoPanStyle.PanThrough ? -_autoZoomAmount : 0.0f, clipPosition);

				switch (_autoZoomStyle)
				{
					case AutoZoomStyle.DollyZoom:
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
					case AutoZoomStyle.PlainZoom:
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
					case AutoRotateType.RotateAroundFocusPoint:
						{
							if (state._focusInfo._focusPointTarget != null)
							{
								Vector3 pivotPoint = state._focusInfo._focusPointTarget.position;
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
					case AutoRotateType.RotateAroundLocalPoint:
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
					case AutoRotateType.RotateAroundAxis:
					default:
						{
							Vector3 axis = _autoRotateLocalPoint;
							Quaternion rotation = Quaternion.AngleAxis(angle, axis);

							state._rotation *= rotation;
						}
						break;
				}
			}
			#endregion
		}
	}
}