using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	using Maths;
	using Paths;

	namespace CinematicCameraSystem
	{
		public class CinematicCameraPlayableBehaviour : PlayableBehaviour
		{
			public Object _clipAsset;
			public CinematicCameraShot _cameraShot;
			public Path _path;
			public InterpolationType _pathInterpolation;
		}
	}
}
