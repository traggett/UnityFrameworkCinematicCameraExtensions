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
				return new CinematicCameraState(
					this.transform.position,
					this.transform.rotation,
					GetCamera().fieldOfView,
					GetCamera().rect,
					this._focusInfo
					);
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