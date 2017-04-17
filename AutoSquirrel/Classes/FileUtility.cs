namespace AutoSquirrel
{
    using System;
    using System.IO;
    using System.Windows;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

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
            try {
                using (StreamReader file = File.OpenText(filePath)) {
                    var serializer = new JsonSerializer()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    if (typeof(TRet) == typeof(AutoSquirrelModel)) {
                        serializer.Binder = new AutoSquirrelBindAll();
                    }

                    return (TRet)serializer.Deserialize(file, typeof(TRet));
                }
            } catch (Exception ex) {
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
            if (!File.Exists(filePath)) {
                File.Create(filePath).Close();
            }

            try {
                var serializer = new JsonSerializer()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    NullValueHandling = NullValueHandling.Ignore
                };

                using (var sw = new StreamWriter(filePath))
                using (JsonWriter writer = new JsonTextWriter(sw)) {
                    serializer.Serialize(writer, objectToSerialize);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private class AutoSquirrelBindAll : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                switch (typeName) {
                case "AutoSquirrel.WebConnectionBase":
                    return typeof(WebConnectionBase);

                case "AutoSquirrel.AutoSquirrelModel":
                    return typeof(AutoSquirrelModel);

                case "AutoSquirrel.FileSystemConnection":
                    return typeof(FileSystemConnection);

                case "AutoSquirrel.AmazonS3Connection":
                    return typeof(AmazonS3Connection);

                case "AutoSquirrel.ItemLink":
                    return typeof(ItemLink);

                case "System.Collections.Generic.List`1[[AutoSquirrel.WebConnectionBase, AutoSquirrel]]":
                    return typeof(List<WebConnectionBase>);

                case "System.Collections.ObjectModel.ObservableCollection`1[[AutoSquirrel.ItemLink, AutoSquirrel]]":
                    return typeof(ObservableCollection<ItemLink>);

                default:
                    return null;
                }
            }
        }
    }
}