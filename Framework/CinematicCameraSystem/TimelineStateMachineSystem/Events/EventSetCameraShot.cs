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
			public Extrapolation _extrapolation = Extrapolation.Hold;
			public float _blendTime = 0.0f;
			public InterpolationType _blendEaseType = InterpolationType.InOutSine;
			#endregion

			#region Event
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
					camera.Play(cameraShot, _extrapolation, _blendTime, _blendEaseType);
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
					camera.Stop(cameraShot);
				}
			}

#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
