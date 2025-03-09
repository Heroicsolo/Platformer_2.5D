using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace HeroicEngine.Utils.Data
{
    public static class DataSaver
    {
        private static readonly byte[] key = Encoding.UTF8.GetBytes("0123456789abcdef");

        /// <summary>
        /// This method saves custom data of type T into PlayerPrefs with certain key
        /// In case if data has not serializable type, it will not be saved and method will return false. 
        /// This method encrypts data by AES symmetric algorithm.
        /// </summary>
        /// <typeparam name="T">Type of data object</typeparam>
        /// <param name="key">Unique key for storing this data</param>
        /// <param name="data">Data object to save</param>
        /// <returns>true, if data was saved, otherwise false</returns>
        public static bool SavePrefsSecurely<T>(string key, T data)
        {
            try
            {
                // Step 1: Serialize the data to JSON for portability
                string jsonData = JsonUtility.ToJson(data);

                // Step 2: Convert JSON string to byte array
                byte[] serializedData = Encoding.UTF8.GetBytes(jsonData);

                // Step 3: Encrypt the serialized byte array
                byte[] encryptedData = EncryptData(serializedData);

                // Step 4: Convert encrypted data to Base64 for safe storage
                string base64Data = Convert.ToBase64String(encryptedData);

                // Step 5: Save the Base64 string to PlayerPrefs
                PlayerPrefs.SetString(key, base64Data);
                PlayerPrefs.Save();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SavePrefsSecurely failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// This method loads custom data of type T saved in PlayerPrefs with known key.
        /// In case if this key isn't presented in PlayerPrefs, it returns false.
        /// </summary>
        /// <typeparam name="T">Type of data object</typeparam>
        /// <param name="key">Unique key for storing this data</param>
        /// <param name="data">Data object to load</param>
        /// <returns>true, if data is found, otherwise false</returns>
        public static bool LoadPrefsSecurely<T>(string key, out T data)
        {
            data = default;

            try
            {
                if (PlayerPrefs.HasKey(key))
                {
                    // Step 1: Retrieve the Base64 string from PlayerPrefs
                    string base64Data = PlayerPrefs.GetString(key);

                    // Step 2: Convert Base64 string back to byte array
                    byte[] encryptedData = Convert.FromBase64String(base64Data);

                    // Step 3: Decrypt the data
                    byte[] decryptedData = DecryptData(encryptedData);

                    // Step 4: Convert decrypted byte array back to JSON string
                    string jsonData = Encoding.UTF8.GetString(decryptedData);

                    // Step 5: Deserialize the JSON string into the object
                    data = JsonUtility.FromJson<T>(jsonData);

                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"LoadPrefsSecurely failed: {e.Message}");
            }

            return false;
        }


        /// <summary>
        /// This method saves custom data of type T into JSON file with given fileName. In case if data has not serializable type, it will not be saved and method will return false.
        /// File will be stored in persistent application data path, on Windows it will be "C:\Users\%username%\AppData\LocalLow\Heroicsolo\Heroic Engine\" directory.
        /// </summary>
        /// <typeparam name="T">Type of data object</typeparam>
        /// <param name="data">Data object to save</param>
        /// <param name="fileName">File name</param>
        /// <returns>true, if data was saved, otherwise false</returns>
        public static bool SaveData<T>(T data, string fileName)
        {
            if (IsSerializable(data))
            {
                string json = JsonUtility.ToJson(data, true);
                string path = $"{Application.persistentDataPath}/{fileName}.json";
                File.WriteAllText(path, json);
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method saves custom data of type T into encoded *.data file with given fileName. In case if data has not serializable type, it will not be saved and method will return false.
        /// File will be stored in persistent application data path, on Windows it will be "C:\Users\%username%\AppData\LocalLow\Heroicsolo\Heroic Engine\" directory. This method encrypts data by AES symmetric algorithm.
        /// </summary>
        /// <typeparam name="T">Type of data object</typeparam>
        /// <param name="data">Data object to save</param>
        /// <param name="fileName">File name</param>
        /// <returns>true, if data was saved, otherwise false</returns>
        public static bool SaveDataSecurely<T>(T data, string fileName)
        {
            if (IsSerializable(data))
            {
                // Step 1: Serialize the data into a binary format
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream();
                formatter.Serialize(memoryStream, data);

                // Get the serialized byte array
                byte[] serializedData = memoryStream.ToArray();

                // Step 2: Encrypt the serialized byte array
                byte[] encryptedData = EncryptData(serializedData);

                string path = $"{Application.persistentDataPath}/{fileName}.data";

                // Step 3: Write the encrypted data to a file
                File.WriteAllBytes(path, encryptedData);

                return true;
            }

            return false;
        }

        /// <summary>
        /// This method loads custom data of type T from JSON file with given fileName.
        /// File should be stored in persistent application data path, on Windows it will be "C:\Users\%username%\AppData\LocalLow\Heroicsolo\Heroic Engine\" directory.
        /// In case if file was not found, it returns false.
        /// </summary>
        /// <typeparam name="T">Type of data object</typeparam>
        /// <param name="fileName">File name</param>
        /// <param name="data">Data object to load</param>
        /// <returns>true, if data is found, otherwise false</returns>
        public static bool LoadData<T>(string fileName, out T data)
        {
            string path = $"{Application.persistentDataPath}/{fileName}.json";

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<T>(json);
                return true;
            }

            data = default;
            return false;
        }

        /// <summary>
        /// This method loads custom data of type T from encrypted *.data file with given fileName.
        /// File should be stored in persistent application data path, on Windows it will be "C:\Users\%username%\AppData\LocalLow\Heroicsolo\Heroic Engine\" directory.
        /// In case if file was not found, it returns false.
        /// </summary>
        /// <typeparam name="T">Type of data object</typeparam>
        /// <param name="fileName">File name</param>
        /// <param name="data">Data object to load</param>
        /// <returns>true, if data is found, otherwise false</returns>
        public static bool LoadDataSecurely<T>(string fileName, out T data)
        {
            string path = $"{Application.persistentDataPath}/{fileName}.data";

            if (File.Exists(path))
            {
                // Step 1: Read the encrypted data from the file
                byte[] encryptedData = File.ReadAllBytes(path);

                // Step 2: Decrypt the data
                byte[] decryptedData = DecryptData(encryptedData);

                // Step 3: Deserialize the data back into the PlayerData object
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream(decryptedData);
                data = (T)formatter.Deserialize(memoryStream);
                return true;
            }

            data = default;
            return false;
        }

        private static bool IsSerializable<T>(T obj)
        {
            return ((obj is ISerializable) || (Attribute.IsDefined(typeof(T), typeof(SerializableAttribute))));
        }

        // Encrypt the byte array using AES encryption
        private static byte[] EncryptData(byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV(); // Generate a new IV for each encryption
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

                    // Combine IV and encrypted data
                    byte[] result = new byte[aesAlg.IV.Length + encryptedData.Length];
                    Buffer.BlockCopy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
                    Buffer.BlockCopy(encryptedData, 0, result, aesAlg.IV.Length, encryptedData.Length);

                    return result;
                }
            }
        }


        // Decrypt the byte array using AES decryption
        private static byte[] DecryptData(byte[] data)
        {
            if (data == null || data.Length < 16)
                throw new ArgumentException("Invalid data: Too short to contain an IV and encrypted data.");

            using (Aes aesAlg = Aes.Create())
            {
                byte[] iv = new byte[16]; // Extract IV
                byte[] encryptedData = new byte[data.Length - 16]; // Extract encrypted data

                Buffer.BlockCopy(data, 0, iv, 0, 16);
                Buffer.BlockCopy(data, 16, encryptedData, 0, encryptedData.Length);

                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                }
            }
        }

    }
}