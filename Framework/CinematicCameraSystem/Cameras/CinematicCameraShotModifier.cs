using UnityEngine;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		public abstract class CinematicCameraShotModifier : MonoBehaviour
		{
			#region Public Functions
			public abstract void ModifiyState(ref CinematicCameraState state);
			#endregion
		}
	}
}