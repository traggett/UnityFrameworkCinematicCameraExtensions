using System;

namespace Framework
{
	using Maths;
	using StateMachineSystem;
	using Utils;

	namespace CinematicCameraSystem
	{
		[Serializable]
		public class BackgroundLogicCameraShot : ConditionalStateBackgroundLogic
		{
			#region Public Data
			public ComponentRef<CinematicCameraMixer> _camera;
			public ComponentRef<CinematicCameraShot> _shot;
			public float _duration = 0.0f;
			public eExtrapolation _extrapolation = eExtrapolation.Hold;
			public float _blendTime = 0.0f;
			public InterpolationType _blendEaseType = InterpolationType.InOutSine;
			#endregion

			#region BranchingBackgroundLogic
#if UNITY_EDITOR
			public override string GetDescription()
		{
			return "<b>" + _shot + "</b>";
		}
#endif
			#endregion

			#region IBranch
			public override void OnLogicStarted(StateMachineComponent stateMachine)
			{
				CinematicCameraMixer camera = _camera.GetComponent();
				CinematicCameraShot cameraShot = _shot.GetComponent();

				if (camera != null && cameraShot != null)
				{
					camera.StartCameraShot(cameraShot, _duration, _extrapolation, _blendTime, _blendEaseType);
				}
			}

			public override void OnLogicFinished(StateMachineComponent stateMachine)
			{
				CinematicCameraMixer camera = _camera.GetComponent();
				CinematicCameraShot cameraShot = _shot.GetComponent();

				if (camera != null && cameraShot != null)
				{
					camera.StopCameraShot(cameraShot);
				}
			}

			public override void UpdateLogic(StateMachineComponent stateMachine)
			{

			}
			#endregion
		}
	}	
}