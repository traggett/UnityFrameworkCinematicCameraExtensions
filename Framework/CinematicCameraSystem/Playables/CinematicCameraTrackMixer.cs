using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Paths;
	using Maths;

	namespace CinematicCameraSystem
	{
		public class CinematicCameraTrackMixer : PlayableBehaviour
		{
			private CinematicCameraTrack _trackAsset;
			private PlayableDirector _director;
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

					if (inputBehaviour != null && (inputBehaviour._cameraShot != null || inputBehaviour._path != null))
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

									Extrapolation extrapolation = Extrapolation.Hold;

									if (clip.hasPreExtrapolation && _director.time < clip.start)
										extrapolation = GetExtrapolation(clip.preExtrapolationMode);
									else if (clip.hasPostExtrapolation && _director.time > clip.start + clip.duration)
										extrapolation = GetExtrapolation(clip.postExtrapolationMode);

									float timeInClip = (float)(_director.time - clip.start);	

									//Single shot
									if (inputBehaviour._cameraShot != null)
									{
										states[i] = inputBehaviour._cameraShot.GetState(timeInClip, (float)clip.duration);
									}
									//Camera path
									else if (inputBehaviour._path != null)
									{
										float clipPosition = CinematicCameraMixer.GetClipPosition(extrapolation, timeInClip, (float)clip.duration);
										float pathT = (float)MathUtils.Interpolate(inputBehaviour._pathInterpolation, 0d, 1f, clipPosition);

										//Work out path position
										PathPosition pos = inputBehaviour._path.GetPoint(pathT);

										//Work out a lerp between to path nodes
										inputBehaviour._path.GetNodeSection(pathT, out int startNode, out int endNode, out float sectionT);

										//Lerp camera shot state based on the relevant section of the path
										GetPathNodeStateInfo(timeInClip, (float)clip.duration, inputBehaviour._path, startNode, out CinematicCameraState startNodeState, out Quaternion startNodeFromTo, out Vector3 startNodeCamFor, out Vector3 startNodeCamUp);
										GetPathNodeStateInfo(timeInClip, (float)clip.duration, inputBehaviour._path, endNode, out CinematicCameraState endNodeState, out Quaternion endNodeFromTo, out Vector3 endNodeCamFor, out Vector3 endNodeCamUp);

										Vector3 cameraForward;
										Vector3 cameraUp;

										if (sectionT <= 0f)
										{
											states[i] = startNodeState;
											//cameraForward = startNodeFromTo * pos._pathForward;
											cameraForward = startNodeCamFor;
											cameraUp = startNodeCamUp;
										}
										else if (sectionT >= 1f)
										{
											states[i] = endNodeState;
											//cameraForward = endNodeFromTo * pos._pathForward;
											cameraForward = endNodeCamFor;
											cameraUp = endNodeCamUp;
										}
										else
										{
											states[i] = CinematicCameraState.Interpolate(_trackBinding, startNodeState, endNodeState, InterpolationType.Linear, sectionT);

											//Work out rotation based on 
											Quaternion cameraRotation = Quaternion.Slerp(startNodeFromTo, endNodeFromTo, sectionT);
											cameraForward = cameraRotation * pos._pathForward;

											cameraForward = Vector3.Lerp(startNodeCamFor, endNodeCamFor, sectionT);
											cameraUp = Vector3.Lerp(startNodeCamUp, endNodeCamUp, sectionT);
										}
											

										//Set camera position from path pos
										states[i]._position = pos._pathPosition;
										states[i]._rotation = Quaternion.LookRotation(cameraForward, cameraUp);


									}
									
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
								blendedState = CinematicCameraState.Interpolate(_trackBinding, blendedState, states[i], InterpolationType.Linear, inputWeights[i] * weightAdjust);
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

			private static Extrapolation GetExtrapolation(TimelineClip.ClipExtrapolation clipExtrapolation)
			{
				switch (clipExtrapolation)
				{
					case TimelineClip.ClipExtrapolation.Loop:
						return Extrapolation.Loop;
					case TimelineClip.ClipExtrapolation.PingPong:
						return Extrapolation.PingPong;
					case TimelineClip.ClipExtrapolation.None:
					case TimelineClip.ClipExtrapolation.Continue:
					case TimelineClip.ClipExtrapolation.Hold:
					default:
						return Extrapolation.Hold;
				}
			}

			private void GetPathNodeStateInfo(float timeInClip, float clipDuration, Path path, int nodeIndex, out CinematicCameraState cameraState, out Quaternion rotationToPath, out Vector3 forwardDir, out Vector3 upDir)
			{
				// (TO DO: cache components)
				CinematicCameraShot shot = path._nodes[nodeIndex]._node.GetComponent<CinematicCameraShot>();

				if (shot != null)
				{
					cameraState = shot.GetState(timeInClip, clipDuration);

					PathPosition nodePos = path.GetPoint(path.GetPathT(path._nodes[nodeIndex]._node));
					rotationToPath = Quaternion.FromToRotation(nodePos._pathForward, shot.transform.forward);
					forwardDir = shot.transform.forward;
					upDir = shot.transform.up;
				}
				else
				{
					cameraState = default;
					rotationToPath = Quaternion.identity;
					forwardDir = Vector3.forward;
					upDir = Vector3.up;
				}
			}
		}
	}
}
