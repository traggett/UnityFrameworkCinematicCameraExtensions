using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		[Serializable]
		public class CinematicCameraShotClip : PlayableAsset, ITimelineClipAsset
		{
			public ExposedReference<CinematicCameraShot> _cameraShot;
			
			public ClipCaps clipCaps
			{
				get { return ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.Looping; }
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				var playable = ScriptPlayable<CinematicCameraPlayableBehaviour>.Create(graph, new CinematicCameraPlayableBehaviour());
				CinematicCameraPlayableBehaviour clone = playable.GetBehaviour();
				clone._clipAsset = this;
				clone._cameraShot = _cameraShot.Resolve(graph.GetResolver());
				return playable;
			}
		}
	}
}
