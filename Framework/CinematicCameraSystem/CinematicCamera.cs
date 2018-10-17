using UnityEngine;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		public class CinematicCamera : MonoBehaviour
		{
			#region Public Data
			public CinematicCameraFocusInfo _focusInfo;
			#endregion

			#region Private Data
			private Camera _camera;
			#endregion
			
			#region Public Functions
			public Camera GetCamera()
			{
				if (_camera == null)
					_camera = GetComponentInChildren<Camera>();

				return _camera;
			}
			
			public virtual CinematicCameraState GetState()
			{
				CinematicCameraState state = new CinematicCameraState
				{
					_position = this.transform.position,
					_rotation = this.transform.rotation,
					_cameraRect = GetCamera().rect,
					_fieldOfView = GetCamera().fieldOfView,
					_focusInfo = this._focusInfo
				};
				return state;
			}

			public virtual void SetState(CinematicCameraState state)
			{
				this.transform.position = state._position;
				this.transform.rotation = state._rotation;
				GetCamera().fieldOfView = state._fieldOfView;
				GetCamera().rect = state._cameraRect;
				_focusInfo = state._focusInfo;
			}
			#endregion
		}
	}
}