using Framework.Maths;
using System;
using UnityEngine;

namespace Framework
{
	namespace CinematicCameraSystem
	{
		public class CinematicCameraShot : MonoBehaviour
		{
			#region Public Data
			[Range(0.0001f, 180f)]
			public float _fieldOfView = 60.0f;
			public Rect _cameraRect = new Rect(0, 0, 1, 1);
			public CinematicCameraFocusInfo _focusInfo = CinematicCameraFocusInfo.kDefault;
			public float _defaultDuration = 1f;
			public CinematicCameraShotModifier[] _cinematicCameraShotModifiers;

			#region Previewing
#if UNITY_EDITOR
			[HideInInspector]
			public CinematicCamera _previewCamera;
			[HideInInspector]
			public bool _previewUsingAllCameras;
			[HideInInspector]
			public Extrapolation _previewClipExtrapolation;
			public static bool _preview;
#endif
			#endregion

			#endregion

			#region Unity Messages
			void Update()
			{
#if UNITY_EDITOR
				if (Application.isPlaying)
#endif
				{
					
				}
					
			}

#if UNITY_EDITOR
			void OnDrawGizmos()
			{
				Gizmos.DrawIcon(transform.position, "CameraShotIcon", true);
			}

			public static bool IsPreviewing()
			{
				return _preview;
			}
#endif
			#endregion

			#region Public Functions
			public CinematicCameraState GetState(float shotTime = 0.0f, float shotDuration = -1f)
			{
				//Get default state
				CinematicCameraState state = new CinematicCameraState
				{
					_position = this.transform.position,
					_rotation = this.transform.rotation,
					_fieldOfView = this._fieldOfView,
					_cameraRect = this._cameraRect,
					_focusInfo = this._focusInfo
				};

				//Apply modifiers
				for (int i=0; i<_cinematicCameraShotModifiers.Length; i++)
				{
					_cinematicCameraShotModifiers[i].ModifiyState(ref state, shotTime, shotDuration);
				}

				return state;
			}
			#endregion
		}
	}
}