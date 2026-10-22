namespace Crisis.Runtime.FullSource
{
using Crisis.Runtime.Weapons;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class Encryption
{
	private const string Key = "44afsg/M/9+pLnn+ba/P2QzEdZ0RcFw7Obft19sbSNQ=";

	private const string IV = "nXDTe6/AwdWhJIxppvFYHStqbbqZ6ydPNq3GoWOzo1g=";

	public static string Encrypt(string prm_text_to_encrypt)
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Padding = PaddingMode.PKCS7;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.KeySize = 256;
		rijndaelManaged.BlockSize = 256;
		RijndaelManaged rijndaelManaged2 = rijndaelManaged;
		byte[] rgbKey = Convert.FromBase64String("44afsg/M/9+pLnn+ba/P2QzEdZ0RcFw7Obft19sbSNQ=");
		byte[] rgbIV = Convert.FromBase64String("nXDTe6/AwdWhJIxppvFYHStqbbqZ6ydPNq3GoWOzo1g=");
		ICryptoTransform transform = rijndaelManaged2.CreateEncryptor(rgbKey, rgbIV);
		MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		byte[] bytes = Encoding.ASCII.GetBytes(prm_text_to_encrypt);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		byte[] inArray = memoryStream.ToArray();
		return Convert.ToBase64String(inArray);
	}

	public static string Decrypt(string prm_text_to_decrypt)
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Padding = PaddingMode.PKCS7;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.KeySize = 256;
		rijndaelManaged.BlockSize = 256;
		RijndaelManaged rijndaelManaged2 = rijndaelManaged;
		byte[] rgbKey = Convert.FromBase64String("44afsg/M/9+pLnn+ba/P2QzEdZ0RcFw7Obft19sbSNQ=");
		byte[] rgbIV = Convert.FromBase64String("nXDTe6/AwdWhJIxppvFYHStqbbqZ6ydPNq3GoWOzo1g=");
		ICryptoTransform transform = rijndaelManaged2.CreateDecryptor(rgbKey, rgbIV);
		byte[] array = Convert.FromBase64String(prm_text_to_decrypt);
		byte[] array2 = new byte[array.Length];
		MemoryStream stream = new MemoryStream(array);
		CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		cryptoStream.Read(array2, 0, array2.Length);
		return Encoding.ASCII.GetString(array2);
	}
}

}
