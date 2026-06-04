using UnityEngine;

public class NetworkObject : MonoBehaviour
{
	public struct State
	{
		internal double timestamp;

		internal Vector3 pos;

		internal Quaternion rot;

		internal Vector3 vel;
	}

	public bool positionOnly;

	public bool tracking;

	public bool disableIfMine;

	private PickupObject pickup;

	private Vector3 newPos;

	private Vector3 startPos;

	private Quaternion newRot;

	public float interpolationMS = 0.2f;

	private int timestampCount;

	private int latestState;

	private bool networkRunning;

	private int collisionCount;

	private float timeUntilCollisionCountResets = 1f;

	private float lastCollisionTime;

	private bool sentRPCForLastCollision;

	public State[] states = new State[8];

	private void Awake()
	{
		if ((bool)GetComponent<PickupObject>())
		{
			pickup = GetComponent<PickupObject>();
		}
		if (disableIfMine && base.networkView.isMine)
		{
			base.enabled = false;
		}
		startPos = base.transform.position;
		State state = default(State);
		state.timestamp = Network.time;
		state.pos = startPos;
		state.rot = Quaternion.identity;
		state.vel = Vector3.zero;
		states[0] = state;
	}

	private void OnGUI()
	{
		if (!tracking)
		{
			return;
		}
		double time = Network.time;
		double num = time - (double)interpolationMS;
		if (states[0].timestamp > num)
		{
			GUI.backgroundColor = Color.white;
			GUI.Label(new Rect(10f, 10f, 300f, 30f), "Inter: " + states[0].timestamp + " >= " + (Network.time - (double)interpolationMS));
		}
		else
		{
			GUI.contentColor = Color.black;
			GUI.Label(new Rect(10f, 10f, 300f, 30f), "Extra: " + states[0].timestamp + " < " + (Network.time - (double)interpolationMS));
		}
		GUI.contentColor = Color.black;
		double num2 = 0.0;
		for (int i = 0; i < Mathf.Min(5, states.Length); i++)
		{
			double num3 = Network.time - states[i].timestamp;
			if (num3 < (double)interpolationMS)
			{
				GUI.contentColor = Color.green;
			}
			else
			{
				GUI.contentColor = Color.red;
			}
			GUI.Label(new Rect(10f, 140 + i * 32, 500f, 30f), "[" + i + "] delay: " + Mathf.Round((float)num3 * 100f));
			num2 += Network.time - states[i].timestamp;
		}
		GUI.contentColor = Color.white;
		GUI.Label(new Rect(10f, 60f, 400f, 30f), "Ping: " + Mathf.Round((float)(num2 / 5.0) * 100f));
		GUI.Label(new Rect(10f, 90f, 400f, 30f), "Latest State: " + latestState);
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		if (Network.isServer)
		{
			lastCollisionTime = Time.time;
		}
	}

	public void Update()
	{
		if (base.networkView.isMine && Network.peerType != 0 && !sentRPCForLastCollision && lastCollisionTime + timeUntilCollisionCountResets > Time.time && (bool)base.rigidbody && base.rigidbody.velocity.magnitude < 0.5f)
		{
			base.networkView.RPC("SetObjectPosition", RPCMode.Others, base.transform.position, base.transform.rotation, base.networkView.viewID);
			sentRPCForLastCollision = true;
		}
		if (Network.peerType == NetworkPeerType.Disconnected || base.networkView.isMine || (base.tag.Contains("Physics") && !base.name.Contains("rat") && (((bool)base.renderer && base.renderer.isVisible) || states[0].pos == Vector3.zero)))
		{
			return;
		}
		if ((bool)base.rigidbody)
		{
			base.rigidbody.isKinematic = false;
		}
		float num = interpolationMS;
		double time = Network.time;
		double num2 = time - (double)num;
		if (!pickup || ((bool)pickup && !pickup.beingHeld))
		{
			if (states[0].timestamp > num2)
			{
				for (int i = 0; i < timestampCount; i++)
				{
					if (states[i].timestamp <= num2 || i == timestampCount - 1)
					{
						latestState = i;
						State state = states[Mathf.Max(i - 1, 0)];
						State state2 = states[i];
						double num3 = state.timestamp - state2.timestamp;
						float t = 0f;
						if (num3 > 0.0001)
						{
							t = (float)((num2 - state2.timestamp) / num3);
						}
						base.transform.position = Vector3.Lerp(base.transform.position, Vector3.Lerp(state2.pos, state.pos, t), 0.1f);
						if (!positionOnly)
						{
							base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.Slerp(state2.rot, state.rot, t), 0.1f);
						}
						if (states[i].vel == Vector3.zero && base.rigidbody != null)
						{
							base.rigidbody.velocity = Vector3.zero;
							base.rigidbody.angularVelocity = Vector3.zero;
						}
						return;
					}
				}
			}
			else
			{
				if ((bool)GetComponent<FirstPersonControl>())
				{
					return;
				}
				if (!networkRunning)
				{
					for (int j = 1; j < timestampCount; j++)
					{
						states[j].pos = states[0].pos;
						states[j].timestamp = Network.time;
					}
					networkRunning = true;
				}
				Vector3 position = base.transform.position;
				Quaternion quaternion = Quaternion.FromToRotation(position, states[0].pos);
				Vector3 vector = quaternion * (states[0].pos - position);
				Vector3 vector2 = states[0].pos + vector;
				if (states[0].vel == Vector3.zero && base.rigidbody != null)
				{
					base.rigidbody.velocity = Vector3.zero;
					base.rigidbody.angularVelocity = Vector3.zero;
				}
				if (base.transform.position != vector2)
				{
					base.transform.position = Vector3.Lerp(base.transform.position, Vector3.Lerp(base.transform.position, vector2, 0.2f), 0.1f);
					if (!positionOnly)
					{
						base.transform.rotation = Quaternion.Lerp(base.transform.rotation, states[0].rot, 0.2f);
					}
				}
			}
		}
		if ((states[0].pos - base.transform.position).magnitude > 0.1f)
		{
			base.transform.position = states[0].pos;
		}
	}

	public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 value = base.transform.localPosition;
			Quaternion value2 = base.transform.localRotation;
			Vector3 zero = Vector3.zero;
			if ((bool)base.rigidbody)
			{
				zero = base.rigidbody.velocity;
			}
			stream.Serialize(ref value);
			stream.Serialize(ref value2);
			return;
		}
		Vector3 value3 = Vector3.zero;
		Quaternion value4 = Quaternion.identity;
		Vector3 zero2 = Vector3.zero;
		stream.Serialize(ref value3);
		stream.Serialize(ref value4);
		for (int num = states.Length - 1; num >= 1; num--)
		{
			states[num] = states[num - 1];
		}
		State state = default(State);
		state.timestamp = info.timestamp;
		state.pos = value3;
		state.rot = value4;
		state.vel = zero2;
		states[0] = state;
		timestampCount = Mathf.Min(timestampCount + 1, states.Length);
	}

	private void OnPlayerConnected(NetworkPlayer player)
	{
		base.networkView.RPC("SyncObject", RPCMode.Others, base.transform.position, base.transform.rotation);
		if ((bool)base.rigidbody)
		{
			base.networkView.RPC("SyncRigidbody", RPCMode.Others, base.rigidbody.velocity, base.rigidbody.angularVelocity);
		}
	}

	[RPC]
	private void SetObjectPosition(Vector3 pos, Quaternion rot, NetworkViewID id)
	{
		Transform parent = NetworkView.Find(id).transform;
		if (parent.name.Equals("PlateModel") || parent.name.Equals("burger-bottom"))
		{
			parent = parent.parent;
		}
		if ((bool)parent.rigidbody && !parent.rigidbody.isKinematic)
		{
			parent.rigidbody.velocity = Vector3.zero;
			parent.rigidbody.angularVelocity = Vector3.zero;
		}
		parent.position = pos;
		parent.rotation = rot;
		states[0].pos = pos;
		states[0].rot = rot;
		newPos = pos;
		newRot = rot;
	}

	[RPC]
	private void SyncObject(Vector3 pos, Quaternion rot)
	{
		base.transform.position = pos;
		base.transform.rotation = rot;
		for (int i = 0; i < states.Length; i++)
		{
			states[i].pos = pos;
			states[i].rot = rot;
		}
	}

	[RPC]
	private void SyncRigidbody(Vector3 vel, Vector3 aVel)
	{
		if (!base.rigidbody)
		{
			base.gameObject.AddComponent<Rigidbody>();
		}
		if (!base.rigidbody.isKinematic)
		{
			base.rigidbody.velocity = vel;
			base.rigidbody.angularVelocity = aVel;
		}
	}

	private Color rand()
	{
		Color white = Color.white;
		float value = Random.value;
		if ((double)value > 0.9)
		{
			return Color.red;
		}
		if ((double)value > 0.8)
		{
			return Color.yellow;
		}
		if ((double)value > 0.7)
		{
			return Color.green;
		}
		if ((double)value > 0.6)
		{
			return Color.blue;
		}
		if ((double)value > 0.5)
		{
			return Color.magenta;
		}
		if ((double)value > 0.4)
		{
			return Color.cyan;
		}
		if ((double)value > 0.3)
		{
			return Color.white;
		}
		if ((double)value > 0.2)
		{
			return Color.Lerp(Color.red, Color.yellow, 0.5f);
		}
		return Color.black;
	}
}
