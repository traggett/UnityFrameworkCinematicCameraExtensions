using UnityEngine;

namespace Framework
{
	
	namespace CinematicCameraSystem
	{
		public abstract class CinematicCameraShotModifier : MonoBehaviour
		{
			#region Public Functions
			public abstract void ModifiyState(ref CinematicCameraState state, float shotTime, float shotDuration);
			#endregion


#if UNITY_EDITOR
			protected bool IsBeingPreviewed()
			{
				return CinematicCameraShot.IsPreviewing();
			}
#endif
		}
	}
}