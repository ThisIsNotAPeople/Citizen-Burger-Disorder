using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Secure : MonoBehaviour
{
	private static string salt = "BurgerBurgerStorung";

	public static string Md5Sum(string strToEncrypt)
	{
		MD5 mD = MD5.Create();
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] inArray = mD.ComputeHash(uTF8Encoding.GetBytes(strToEncrypt));
		return Convert.ToBase64String(inArray);
	}
}
