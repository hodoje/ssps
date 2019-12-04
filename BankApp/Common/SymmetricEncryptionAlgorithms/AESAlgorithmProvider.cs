using System.Security.Cryptography;
using System.Text;

namespace Common.SymmetricEncryptionAlgorithms
{
	/// <summary>
	/// AES encryption provider.
	/// </summary>
	public class AESAlgorithmProvider : ISymmetricAlgorithmProvider
	{
		/// <inheritdoc/>
		public byte[] Decrypt(CBCEncriptionInformation decryptionInfo, byte[] encryptedData)
		{
			byte[] decryptedData;
			AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider()
			{
				Key = ASCIIEncoding.ASCII.GetBytes(decryptionInfo.Key),
				Mode = decryptionInfo.CipherMode,
				Padding = PaddingMode.None,
			};

			if (decryptionInfo.CipherMode == CipherMode.CBC)
			{
				aesCrypto.IV = decryptionInfo.InitialVector;
			}

			ICryptoTransform aesDecrypt = aesCrypto.CreateDecryptor();

			decryptedData = aesDecrypt.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

			return decryptedData;
		}

		/// <inheritdoc/>
		public byte[] Encrypt(CBCEncriptionInformation encryptionInfo, byte[] rawData)
		{
			byte[] encryptedData;
			AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider()
			{
				Key = ASCIIEncoding.ASCII.GetBytes(encryptionInfo.Key),
				Mode = encryptionInfo.CipherMode,
				Padding = PaddingMode.None,
			};

			if (encryptionInfo.CipherMode == CipherMode.CBC)
			{
				aesCrypto.GenerateIV();
				encryptionInfo.InitialVector = aesCrypto.IV;
			}

			ICryptoTransform aesEncrypt = aesCrypto.CreateEncryptor();

			encryptedData = aesEncrypt.TransformFinalBlock(rawData, 0, rawData.Length);

			return encryptedData;
		}
	}
}
