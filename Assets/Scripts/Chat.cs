using System.Collections.Generic;
using UnityEngine;

public class Chat : MonoBehaviour
{
	private bool typing;

	private List<string> chat = new List<string>();

	private List<string> chatUsers = new List<string>();

	private List<bool> chatEditingLine = new List<bool>();

	private List<float> chatTimestamp = new List<float>();

	private string newChatInput = string.Empty;

	private string currentChatLine = string.Empty;

	private float inputStartTime;

	private float inputStartDelay = 0.2f;

	private float sentFrame;

	private float receivedFrame;

	private float lastChatInput;

	private float chatFadeStartTime = 5f;

	private float chatFadeDuration = 2f;

	private menu mainMenu;

	private void Start()
	{
		mainMenu = GetComponent<menu>();
	}

	[RPC]
	private void SendNewChatInput(string username, string chatInput, bool endLine)
	{
		int num = -1;
		if (chatUsers.Count == 0)
		{
			num = -1;
		}
		else if (chatUsers.Count > 0)
		{
			num = chatUsers.LastIndexOf(username);
		}
		else if (chatUsers[chatUsers.Count - 1] != username)
		{
			num = -1;
		}
		if (num == -1 || !chatEditingLine[num])
		{
			chatUsers.Add(username);
			chat.Add(chatInput);
			chatEditingLine.Add(true);
			chatTimestamp.Add(Time.time);
		}
		else
		{
			List<string> list;
			List<string> list2 = (list = chat);
			int index;
			int index2 = (index = num);
			string text = list[index];
			list2[index2] = text + " " + chatInput;
			chatEditingLine[num] = endLine;
			chatTimestamp[num] = Time.time;
		}
		lastChatInput = Time.time;
	}

	private void LateUpdate()
	{
		if (!mainMenu.enabled && sentFrame == (float)Time.frameCount && receivedFrame != (float)Time.frameCount && newChatInput != string.Empty)
		{
			lastChatInput = Time.time;
			receivedFrame = Time.frameCount;
			base.networkView.RPC("SendNewChatInput", RPCMode.All, PlayerPrefs.GetString("Username"), newChatInput, typing);
			newChatInput = string.Empty;
		}
	}

	private void OnGUI()
	{
		Event current = Event.current;
		if (!mainMenu.enabled)
		{
			if (typing)
			{
				if (inputStartTime + inputStartDelay < Time.time && current.type == EventType.KeyDown && Event.current.character == "\n"[0])
				{
					if (currentChatLine != string.Empty)
					{
						int num = currentChatLine.LastIndexOf(" ");
						string text = currentChatLine.Substring(0, currentChatLine.Length);
						int num2 = Mathf.Max(0, text.LastIndexOf(" "));
						if (num2 > 0)
						{
							num2++;
						}
						text = text.Substring(num2, text.Length - num2);
						newChatInput = text;
						if (newChatInput.Length == 0)
						{
							newChatInput = " ";
						}
						sentFrame = Time.frameCount;
					}
					currentChatLine = string.Empty;
					typing = false;
				}
				if (currentChatLine != string.Empty && currentChatLine.Substring(currentChatLine.Length - 1) != " " && sentFrame + 5f < (float)Time.frameCount && current.keyCode == KeyCode.Space)
				{
					int num3 = currentChatLine.LastIndexOf(" ");
					string text2 = currentChatLine.Substring(0, currentChatLine.Length);
					int num4 = Mathf.Max(0, text2.LastIndexOf(" "));
					if (num4 > 0)
					{
						num4++;
					}
					text2 = text2.Substring(num4, text2.Length - num4);
					newChatInput = text2;
					sentFrame = Time.frameCount;
				}
				GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
				GUI.skin.textField.fontSize = 28;
				GUI.SetNextControlName("ChatLine");
				currentChatLine = GUI.TextField(new Rect(20f, Screen.height - 90, Screen.width - 40, 80f), string.Empty + currentChatLine);
				GUI.FocusControl("ChatLine");
			}
			else
			{
				sentFrame = Time.frameCount;
				if (current.type == EventType.KeyDown && Event.current.character == "\n"[0])
				{
					currentChatLine = string.Empty;
					newChatInput = string.Empty;
					typing = true;
					inputStartTime = Time.time;
				}
			}
		}
		GUI.skin.label.fontSize = 20;
		string text3 = string.Empty;
		if (chatUsers.Count > 0)
		{
			int num5 = Mathf.Min(chatUsers.Count, 6);
			int num6 = num5;
			while (num6 > 0 && num6 <= chatUsers.Count)
			{
				string text4 = text3;
				text3 = text4 + "\n" + chatUsers[chatUsers.Count - num6] + ": " + chat[chat.Count - num6];
				num6--;
			}
		}
		GUI.skin.label.alignment = TextAnchor.LowerLeft;
		if (typing)
		{
			GUI.skin.label.normal.textColor = Color.black;
			GUI.Label(new Rect(19f, Screen.height - 400, 550f, 300f), string.Empty + text3);
			GUI.skin.label.normal.textColor = Color.white;
			GUI.Label(new Rect(20f, Screen.height - 400 - 2, 550f, 300f), string.Empty + text3);
		}
		else
		{
			Color textColor = Color.black;
			Color textColor2 = Color.white;
			if (Time.time > lastChatInput + chatFadeStartTime)
			{
				textColor = Color.Lerp(Color.black, new Color(0f, 0f, 0f, 0f), (Time.time - lastChatInput - chatFadeStartTime) / (chatFadeStartTime + chatFadeDuration - chatFadeStartTime));
				textColor2 = Color.Lerp(Color.white, new Color(0f, 0f, 0f, 0f), (Time.time - lastChatInput - chatFadeStartTime) / (chatFadeStartTime + chatFadeDuration - chatFadeStartTime));
			}
			GUI.skin.label.normal.textColor = textColor;
			GUI.Label(new Rect(19f, Screen.height - 300, 550f, 300f), string.Empty + text3);
			GUI.skin.label.normal.textColor = textColor2;
			GUI.Label(new Rect(20f, Screen.height - 300 - 2, 550f, 300f), string.Empty + text3);
		}
		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
	}
}
