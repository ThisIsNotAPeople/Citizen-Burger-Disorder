using System.Collections;
using UnityEngine;

public class NetworkSend : MonoBehaviour
{
	private static string sendToURL = "http://kritz.net/CBD/CBD.php";

	private static string salt = "burgerburger";

	public static IEnumerator Send(string username, string argument, string value)
	{
		WWWForm form = new WWWForm();
		form.AddField("username", username);
		form.AddField("argument", argument);
		form.AddField("value", value);
		form.AddField("hash", Secure.Md5Sum(value + username + argument + salt));
		WWW download = new WWW(sendToURL, form);
		yield return download;
		if (!string.IsNullOrEmpty(download.error))
		{
			MonoBehaviour.print("Error downloading: " + download.error);
		}
		else
		{
			MonoBehaviour.print(download.text);
		}
	}
}
