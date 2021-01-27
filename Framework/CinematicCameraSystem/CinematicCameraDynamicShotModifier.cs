namespace Framework
{
	using DynamicValueSystem;

	namespace CinematicCameraSystem
	{
		public class CinematicCameraDynamicShotModifier : CinematicCameraShotModifier
		{
			#region Public Data
			public DynamicQuaternion _rotation;
			public DynamicVector3 _translation;
			#endregion

			#region Public Functions
			public override void ModifiyState(ref CinematicCameraState state, float shotTime, float shotDuration)
			{
				state._rotation *= _rotation;
				state._position += state._rotation * _translation;
			}
			#endregion
		}
	}
}