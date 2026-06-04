using System;
using UnityEngine;

public class NetworkAnimation : MonoBehaviour
{
	public enum AniStates
	{
		Walk_001 = 0,
		Idle = 1,
		Roll = 2,
		other = 3
	}

	public AniStates currentAnimation = AniStates.Idle;

	public AniStates lastAnimation = AniStates.Idle;

	public void SyncAnimation(string animationValue)
	{
		currentAnimation = (AniStates)(int)Enum.Parse(typeof(AniStates), animationValue);
	}

	private void Update()
	{
		if (lastAnimation != currentAnimation || Enum.GetName(typeof(AniStates), currentAnimation) == "Walk_001" || Enum.GetName(typeof(AniStates), currentAnimation) == "Roll")
		{
			lastAnimation = currentAnimation;
			base.animation.CrossFade(Enum.GetName(typeof(AniStates), currentAnimation));
			base.animation["Walk_001"].normalizedSpeed = 1f;
		}
	}

	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			char value = (char)currentAnimation;
			stream.Serialize(ref value);
		}
		else
		{
			char value2 = '\0';
			stream.Serialize(ref value2);
			currentAnimation = (AniStates)value2;
		}
	}
}
