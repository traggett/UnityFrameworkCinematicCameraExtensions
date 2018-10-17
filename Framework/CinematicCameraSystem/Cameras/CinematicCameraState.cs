using UnityEngine;

namespace Framework
{
	using Maths;

	namespace CinematicCameraSystem
	{
		public class CinematicCameraState
		{
			public Vector3 _position;
			public Quaternion _rotation;
			public float _fieldOfView = 60.0f;
			public Rect _cameraRect = new Rect(0, 0, 1, 1);
			public CinematicCameraFocusInfo _focusInfo;

			public CinematicCameraState Interpolate(CinematicCamera camera, CinematicCameraState to, eInterpolation ease, float t)
			{
				CinematicCameraState state = new CinematicCameraState
				{
					_position = MathUtils.Interpolate(ease, _position, to._position, t),
					_rotation = MathUtils.Interpolate(ease, _rotation, to._rotation, t),
					_fieldOfView = MathUtils.Interpolate(ease, _fieldOfView, to._fieldOfView, t),
					_cameraRect = MathUtils.Interpolate(ease, _cameraRect, to._cameraRect, t),
					_focusInfo = CinematicCameraFocusInfo.Interpolate(camera, ease, _focusInfo, to._focusInfo, t),
				};

				return state;
			}
		}
	}
}