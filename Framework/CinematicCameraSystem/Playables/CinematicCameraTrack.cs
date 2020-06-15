using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		[TrackColor(0.99f, 0.4f, 0.71372549019f)]
		[TrackClipType(typeof(CinematicCameraShotClip))]
		[TrackClipType(typeof(CinematicCameraPathClip))]
		[TrackBindingType(typeof(CinematicCamera))]
		public class CinematicCameraTrack : TrackAsset
		{
			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				ScriptPlayable<CinematicCameraTrackMixer> playable = ScriptPlayable<CinematicCameraTrackMixer>.Create(graph, inputCount);
				PlayableDirector playableDirector = go.GetComponent<PlayableDirector>();

				CinematicCameraTrackMixer animatedCameraMixerBehaviour = playable.GetBehaviour();

				if (animatedCameraMixerBehaviour != null)
				{
					animatedCameraMixerBehaviour.SetTrackAsset(this, playableDirector);
				}

				return playable;
			}

			public TimelineClip GetClip(Object clipAsset)
			{
				IEnumerable<TimelineClip> clips = GetClips();

				foreach (TimelineClip clip in clips)
				{
					if (clip.asset == clipAsset)
						return clip;
				}

				return null;
			}
		}
	}
}