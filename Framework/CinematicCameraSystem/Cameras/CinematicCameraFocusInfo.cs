using System;
using UnityEngine;

namespace Framework
{
	using Maths;

	namespace CinematicCameraSystem
	{
		[Serializable]
		public struct CinematicCameraFocusInfo
		{
			#region Public Data
			public Transform _focusPointTarget;
			public float _focusPointDistance;

			//Makes things more / less blurry
			public float _focusPointFocalLength;
			//Makes the range of distances in focus bigger / smaller
			public float _focusPointFNumber;

			public static readonly CinematicCameraFocusInfo kDefault = new CinematicCameraFocusInfo(2.5f, 0.075f, 5.0f);
			#endregion

			public CinematicCameraFocusInfo(float focusPointDistance, float focusPointFocalLength, float focusPointFNumber)
			{
				_focusPointTarget = null;
				_focusPointDistance = focusPointDistance;
				_focusPointFocalLength = focusPointFocalLength;
				_focusPointFNumber = focusPointFNumber;
			}

			public CinematicCameraFocusInfo(Transform focusPoint, float focusPointFocalLength, float focusPointFNumber)
			{
				_focusPointTarget = focusPoint;
				_focusPointDistance = 0.0f;
				_focusPointFocalLength = focusPointFocalLength;
				_focusPointFNumber = focusPointFNumber;
			}

			public float GetFocusDistance(CinematicCamera camera)
			{
				return GetFocusDistance(camera.transform.position);
			}

			public float GetFocusDistance(Vector3 cameraPos)
			{
				if (_focusPointTarget != null)
				{
					return (cameraPos - _focusPointTarget.transform.position).magnitude;
				}

				return _focusPointDistance;
			}

			public static CinematicCameraFocusInfo Interpolate(CinematicCamera camera, eInterpolation ease, CinematicCameraFocusInfo from, CinematicCameraFocusInfo to, float t)
			{
				float focusDistance = MathUtils.Interpolate(ease, from.GetFocusDistance(camera), to.GetFocusDistance(camera), t);
				float focusLength = MathUtils.Interpolate(ease, from._focusPointFocalLength, to._focusPointFocalLength, t);
				float focusPointFNumber = MathUtils.Interpolate(ease, from._focusPointFNumber, to._focusPointFNumber, t);

				return new CinematicCameraFocusInfo(focusDistance, focusLength, focusPointFNumber);
			}
		}
	}
}