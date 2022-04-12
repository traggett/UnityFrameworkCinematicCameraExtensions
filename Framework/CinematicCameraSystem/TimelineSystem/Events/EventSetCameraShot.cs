using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	using TimelineSystem;
	using Utils;

	namespace CinematicCameraSystem
	{
		[Serializable]
		[EventCategory("Animation")]
		public class EventSetCameraShot : Event
		{
			#region Public Data
			public ComponentRef<CinematicCameraMixer> _camera;
			public ComponentRef<CinematicCameraShot> _shot;
			public Extrapolation _extrapolation = Extrapolation.Hold;
			public float _blendTime = 0.0f;
			public InterpolationType _blendEaseType = InterpolationType.InOutSine;
			#endregion

			#region Event
			public override void Trigger()
			{
				CinematicCameraMixer camera = _camera.GetComponent();
				CinematicCameraShot cameraShot = _shot.GetComponent();

				if (camera != null && cameraShot != null)
				{
					camera.Play(cameraShot, _extrapolation, _blendTime, _blendEaseType);
				}


			}

			public override void End()
			{
				CinematicCameraMixer camera = _camera.GetComponent();
				CinematicCameraShot cameraShot = _shot.GetComponent();

				if (camera != null && cameraShot != null)
				{
					camera.Stop(cameraShot);
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(108.0f / 255.0f, 198.0f / 255.0f, 197.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return  "Camera Shot " + _shot;
			}
#endif
			#endregion
		}
	}
}
