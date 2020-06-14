using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	
	namespace CinematicCameraSystem
	{
		public class CinematicCameraPath : MonoBehaviour
		{
			#region Public Data
			public CinematicCameraShot _cameraShots;

			[Serializable]
			public struct ControlPoint
			{
				public Vector3 _startTangent;
				public Vector3 _endTangent;
			}

			[SerializeField]
			public ControlPoint[] _controlPoints;
			#endregion

			#region Public Functions

			#endregion

			#region Private Functions

			#endregion
		}
	}
}