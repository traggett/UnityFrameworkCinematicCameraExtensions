using UnityEngine;

namespace Framework
{
	using Maths;

	namespace CinematicCameraSystem
	{
		public struct CinematicCameraState
		{
			public Vector3 _position;
			public Quaternion _rotation;
			public float _fieldOfView;
			public Rect _cameraRect;
			public CinematicCameraFocusInfo _focusInfo;

			public CinematicCameraState(Vector3 position, Quaternion rotation, float fieldOfView, Rect cameraRect, CinematicCameraFocusInfo focusInfo)
			{
				_position = position;
				_rotation = rotation;
				_fieldOfView = fieldOfView;
				_cameraRect = cameraRect;
				_focusInfo = focusInfo;
			}

			public static CinematicCameraState Interpolate(CinematicCamera camera, CinematicCameraState from, CinematicCameraState to, InterpolationType ease, float t)
			{
				return new CinematicCameraState(
					MathUtils.Interpolate(ease, from._position, to._position, t),
					MathUtils.Interpolate(ease, from._rotation, to._rotation, t),
					MathUtils.Interpolate(ease, from._fieldOfView, to._fieldOfView, t),
					MathUtils.Interpolate(ease, from._cameraRect, to._cameraRect, t),
					CinematicCameraFocusInfo.Interpolate(camera, ease, from._focusInfo, to._focusInfo, t)
					);
			}
		}
	}
}