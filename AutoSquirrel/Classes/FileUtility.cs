namespace AutoSquirrel
{
    using System;
    using System.IO;
    using System.Windows;
    using Newtonsoft.Json;

    /// <summary>
    /// File Utility
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        /// Deserializes the specified file path.
        /// </summary>
        /// <typeparam name="TRet">The type of the ret.</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static TRet Deserialize<TRet>(string filePath)
        {
            try
            {
                using (StreamReader file = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.TypeNameHandling = TypeNameHandling.All;

                    return (TRet)serializer.Deserialize(file, typeof(TRet));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return default(TRet);
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <typeparam name="TRet">The type of the ret.</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="objectToSerialize">The object to serialize.</param>
        public static void SerializeToFile<TRet>(string filePath, TRet objectToSerialize)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            try
            {
                var serializer = new JsonSerializer();
                serializer.TypeNameHandling = TypeNameHandling.All;
                serializer.NullValueHandling = NullValueHandling.Ignore;
                using (StreamWriter sw = new StreamWriter(filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, objectToSerialize);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}