
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Framework.Maths;
	using Paths;

	namespace CinematicCameraSystem
	{
		[Serializable]
		public class CinematicCameraPathClip : PlayableAsset, ITimelineClipAsset
		{
			public ExposedReference<Path> _path;
			public InterpolationType _interpolation = InterpolationType.Linear;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.Looping; }
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				var playable = ScriptPlayable<CinematicCameraPlayableBehaviour>.Create(graph, new CinematicCameraPlayableBehaviour());
				CinematicCameraPlayableBehaviour clone = playable.GetBehaviour();
				clone._clipAsset = this;
				clone._path = _path.Resolve(graph.GetResolver());
				clone._pathInterpolation = _interpolation;
				return playable;
			}
		}
	}
}
