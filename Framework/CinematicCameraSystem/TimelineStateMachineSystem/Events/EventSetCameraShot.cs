using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	using TimelineStateMachineSystem;
	using StateMachineSystem;
	using TimelineSystem;
	using Utils;

	namespace CinematicCameraSystem
	{
		[Serializable]
		[EventCategory("Animation")]
		public class EventSetCameraShot : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<CinematicCameraMixer> _camera;
			public ComponentRef<CinematicCameraShot> _shot;
			public float _duration = 0.0f;
			public eExtrapolation _extrapolation = eExtrapolation.Hold;
			public float _blendTime = 0.0f;
			public eInterpolation _blendEaseType = eInterpolation.InOutSine;
			#endregion

			#region Event
			public override float GetDuration()
			{
				return Mathf.Max(_blendTime, _duration);
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

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				CinematicCameraMixer camera = _camera.GetComponent();
				CinematicCameraShot cameraShot = _shot.GetComponent();

				if (camera != null && cameraShot != null)
				{
					camera.StartCameraShot(cameraShot, _duration, _extrapolation, _blendTime, _blendEaseType);
				}

				return eEventTriggerReturn.EventOngoing;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachineComponent stateMachine)
			{
				CinematicCameraMixer camera = _camera.GetComponent();
				CinematicCameraShot cameraShot = _shot.GetComponent();

				if (camera != null && cameraShot != null)
				{
					camera.StopCameraShot(cameraShot);
				}
			}

#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
