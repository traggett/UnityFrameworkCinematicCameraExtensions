using UnityEngine;

using Framework.Utils;
using Framework.Maths;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		public enum eExtrapolation
		{
			Loop,
			PingPong,
			Hold
		};

		public class CinematicCameraMixer : MonoBehaviour
		{
			public CinematicCamera _camera;
			
			#region Private Data
			private struct ShotInfo
			{
				public CinematicCameraShot _shot;
				public float _weight;
				public eInterpolation _blendType;
				public float _time;
				public float _duration;
				public eExtrapolation _extrapolation;
				
				public CinematicCameraState GetState()
				{
					return _shot.GetState(GetClipPosition(_extrapolation, _time, _duration));
				}
			}
			private ShotInfo _currentShot;
			private float _currentShotBlendSpeed;

			private ShotInfo[] _blendingShots;
			#endregion

			#region MonoBehaviour
			void Update()
			{
				if (_currentShot._shot != null)
				{
					_currentShot._time += Time.deltaTime;

					if (_currentShot._weight < 1.0f)
					{
						_currentShot._weight = Mathf.Clamp01(_currentShot._weight + _currentShotBlendSpeed * Time.deltaTime);
					}
				}

				if (_currentShot._shot != null && _currentShot._weight >= 1.0f)
				{
					float shotPosition = _currentShot._time / _currentShot._duration;
					_camera.SetState(_currentShot.GetState());
				}
				else if (_blendingShots.Length > 0 || _currentShot._shot != null)
				{
					CinematicCameraState blendedState;

					if (_blendingShots.Length > 0)
					{
						_blendingShots[0]._time += Time.deltaTime;

						blendedState = _blendingShots[0].GetState();

						for (int i = 1; i < _blendingShots.Length; i++)
						{
							_blendingShots[i]._time += Time.deltaTime;
							blendedState = CinematicCameraState.Interpolate(_camera, blendedState, _blendingShots[i].GetState(), _blendingShots[i]._blendType, _blendingShots[i]._weight);
						}
					}
					else
					{
						blendedState = _camera.GetState();
					}

					if (_currentShot._shot != null)
					{
						blendedState = CinematicCameraState.Interpolate(_camera, blendedState, _currentShot.GetState(), _currentShot._blendType, _currentShot._weight);
					}

					_camera.SetState(blendedState);
				}
			}
			#endregion

			#region Public Functions
			public void StartCameraShot(CinematicCameraShot shot, float duration, eExtrapolation extrapolation, float blendTime = -1.0f, eInterpolation blendType = eInterpolation.Linear)
			{
				if (blendTime <= 0.0f)
				{
					_currentShot._weight = 1.0f;
					_blendingShots = new ShotInfo[0];
				}
				else
				{
					ArrayUtils.Add(ref _blendingShots, _currentShot);
					_currentShot._weight = 0.0f;
					_currentShot._blendType = blendType;
					_currentShotBlendSpeed = 1.0f / blendTime;
				}

				_currentShot._shot = shot;
				_currentShot._duration = duration;
				_currentShot._extrapolation = extrapolation;
				_currentShot._time = 0.0f;
			}

			public void StopCameraShot(CinematicCameraShot shot, float blendTime = -1.0f, eInterpolation blendType = eInterpolation.Linear)
			{
				if (_currentShot._shot != null && _currentShot._shot == shot)
				{
					if (blendTime <= 0.0f)
					{
						_currentShot._weight = 1.0f;
						_blendingShots = new ShotInfo[0];
					}
					else
					{
						ArrayUtils.Add(ref _blendingShots, _currentShot);
						_currentShot._weight = 0.0f;
						_currentShot._blendType = blendType;
						_currentShotBlendSpeed = 1.0f / blendTime;
					}

					_currentShot = new ShotInfo();
				}
			}

			public static float GetClipPosition(eExtrapolation extrapolation, float time, float duration)
			{
				if (Mathf.Approximately(duration, 0.0f))
					return 0.0f;

				float t = Mathf.Abs(time) / duration;

				switch (extrapolation)
				{
					case eExtrapolation.Loop:
						{
							return t - Mathf.Floor(t);
						}
						
					case eExtrapolation.PingPong:
						{
							int n = Mathf.FloorToInt(t);
							t -= (float)n;

							if (n % 2 == 1)
								t = 1.0f - t;

							return t;
						}
					case eExtrapolation.Hold:
					default:
						return Mathf.Clamp01(t);
				}
			}
			#endregion
		}
	}
}