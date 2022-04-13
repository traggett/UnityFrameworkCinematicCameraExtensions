using UnityEngine;
using System;

namespace Framework
{
	using Animations;
	using Maths;
	
	namespace CinematicCameraSystem
	{
		[RequireComponent(typeof(LegacyAnimator))]
		public class CinematicCameraLegacyAnimationShotModifier : CinematicCameraShotModifier
		{
			#region Public Data
			public AnimationClip _animation;

			#region Previewing
#if UNITY_EDITOR
			[HideInInspector]
			public float _previewClipPos;
#endif
			#endregion

			#endregion

			#region Private Data
			private LegacyAnimator _animator;
			#endregion

			#region Public Functions
			public override void ModifiyState(ref CinematicCameraState state, float shotTime, float shotDuration)
			{
				if (_animator == null)
				{
					_animator = GetComponent<LegacyAnimator>();
				}

				if (_animation != null)
				{
					float animationDuration = shotDuration > 0f ? shotDuration : _animation.length;
					float clipPosition = animationDuration > 0f ? shotTime / animationDuration : 1f;

#if UNITY_EDITOR
					if (IsBeingPreviewed())
					{
						clipPosition = _previewClipPos;
					}
#endif

					SampleAnimationClip(ref state, Mathf.Clamp01(clipPosition));
				}
			}
			#endregion

			#region Private Functions
			private void SampleAnimationClip(ref CinematicCameraState state, float clipPosition)
			{
				_animator.Play(_animation.name);
				_animator.SetAnimationSpeed(_animation.name, 0f);
				_animator.SetAnimationNormalizedTime(_animation.name, clipPosition);
			}
			#endregion
		}
	}
}