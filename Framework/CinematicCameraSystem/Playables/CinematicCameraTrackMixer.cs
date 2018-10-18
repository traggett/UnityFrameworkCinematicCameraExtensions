using System.Collections.Generic;

using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Maths;

	namespace CinematicCameraSystem
	{
		public class CinematicCameraTrackMixer : PlayableBehaviour
		{
			private CinematicCameraTrack _trackAsset;
			private PlayableDirector _director;
			private IEnumerable<TimelineClip> _clips;
			private CinematicCameraState _defaultState;
			private CinematicCamera _trackBinding;
			private bool _firstFrameHappened;

			public void SetTrackAsset(TrackAsset trackAsset, PlayableDirector director)
			{
				_trackAsset = (CinematicCameraTrack)trackAsset;
				_director = director;
			}

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				_trackBinding = playerData as CinematicCamera;

				if (_trackBinding == null)
					return;

				if (!_firstFrameHappened)
				{
					_defaultState = _trackBinding.GetState();
					_firstFrameHappened = true;
				}
				
				int numInputs = playable.GetInputCount();
				float[] inputWeights = new float[numInputs];
				float totalWeights = 0.0f;
				CinematicCameraState[] states = new CinematicCameraState[numInputs];

				for (int i = 0; i < numInputs; i++)
				{
					ScriptPlayable<CinematicCameraPlayableBehaviour> scriptPlayable = (ScriptPlayable<CinematicCameraPlayableBehaviour>)playable.GetInput(i);
					CinematicCameraPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

					if (inputBehaviour != null && inputBehaviour._cameraShot != null)
					{
						float inputWeight = playable.GetInputWeight(i);

						if (inputWeight > 0.0f)
						{
							TimelineClip clip = _trackAsset.GetClip(inputBehaviour._clipAsset);

							if (clip != null)
							{
								double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
								double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

								if (_director.time >= clipStart && _director.time <= clipStart + clipDuration)
								{
									inputWeights[i] = inputWeight;

									eExtrapolation extrapolation = eExtrapolation.Hold;

									if (clip.hasPreExtrapolation && _director.time < clip.start)
										extrapolation = GetExtrapolation(clip.preExtrapolationMode);
									else if (clip.hasPostExtrapolation && _director.time > clip.start + clip.duration)
										extrapolation = GetExtrapolation(clip.postExtrapolationMode);

									float clipPosition = CinematicCameraMixer.GetClipPosition(extrapolation, (float)(_director.time - clip.start), (float)clip.duration);

									states[i] = inputBehaviour._cameraShot.GetState(clipPosition);


									totalWeights += inputWeights[i];
								}
							}
						}
					}
				}

				if (totalWeights > 0.0f)
				{
					CinematicCameraState blendedState = _defaultState;

					float weightAdjust = 1.0f / totalWeights;
					bool firstBlend = true;

					for (int i=0; i<numInputs; i++)
					{
						if (inputWeights[i] > 0.0f)
						{
							if (firstBlend)
							{
								blendedState = states[i];
								firstBlend = false;
							}
							else
							{
								blendedState = CinematicCameraState.Interpolate(_trackBinding, blendedState, states[i], eInterpolation.Linear, inputWeights[i] * weightAdjust);
							}
						}
					}

					_trackBinding.SetState(blendedState);
				}
				else
				{
					_trackBinding.SetState(_defaultState);
				}
			}

			public override void OnGraphStop(Playable playable)
			{
				if (_trackBinding != null)
					_trackBinding.SetState(_defaultState);
				
				_firstFrameHappened = false;
			}

			private static eExtrapolation GetExtrapolation(TimelineClip.ClipExtrapolation clipExtrapolation)
			{
				switch (clipExtrapolation)
				{
					case TimelineClip.ClipExtrapolation.Loop:
						return eExtrapolation.Loop;
					case TimelineClip.ClipExtrapolation.PingPong:
						return eExtrapolation.PingPong;
					case TimelineClip.ClipExtrapolation.None:
					case TimelineClip.ClipExtrapolation.Continue:
					case TimelineClip.ClipExtrapolation.Hold:
					default:
						return eExtrapolation.Hold;
				}
			}

		}
	}
}
