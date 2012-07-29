using System;
using System.Reflection;
using System.Runtime.Serialization;
using log4net;
using System.Xml;
using System.IO;
using OpenBus.Common.Security;
using OpenBus.Messages;

namespace OpenBus.Common.Serialization
{
    /// <summary>
    /// Custom serialization and deserialization handler.
    /// Capable of serializing generics and objects of type 'Type'.
    /// </summary>
    public static class XmlSerializer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(XmlSerializer));

        private delegate object DeserializeHandler<T>(string xml) where T : BusMessage;

        /// <summary>
        /// Gets the default serializer
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DataContractSerializer GetDataContractSerializer(Type type)
        {
            return new DataContractSerializer(type, null, Int32.MaxValue, false, false, null, new BusDataContractResolver());  
        }

        /// <summary>
        /// Serializes an object to xml.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The xml.</returns>
        public static string Serialize<T>(T obj)
        {
            if (Equals(obj, default(T)))
                return null;

            MemoryStream memoryStream = new MemoryStream();

            XmlWriterSettings settings = new XmlWriterSettings
                                             {
                                                 Encoding = System.Text.Encoding.UTF8,
                                                 Indent = true,
                                                 IndentChars = "\t",
                                                 NewLineChars = Environment.NewLine,
                                                 ConformanceLevel = ConformanceLevel.Document
                                             };

            XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings);

            try
            {
                DataContractSerializer dataContractSerializer = GetDataContractSerializer(typeof(T));
                dataContractSerializer.WriteObject(xmlWriter, obj);
                xmlWriter.Flush();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            string xml = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());

            // Release
            xmlWriter.Close();
            memoryStream.Close();
            memoryStream.Dispose();

            return xml;
        }

        /// <summary>
        /// Serializes an object to an xml-file.
        /// The xml file will be overwritten if it exists.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="filePath">The file path to save the xml to. Can be relative.</param>
        public static void SerializeToFile<T>(T obj, string filePath)
        {
            if (filePath.StartsWith("/"))
                filePath = String.Format("{0}{1}", Directory.GetCurrentDirectory(), filePath);

            int index = filePath.LastIndexOf(@"/");

            if (index > -1)
            {
                string directory = filePath.Substring(0, index);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            string xml = Serialize(obj);
            StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            writer.Write(xml);
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Deserializes xml to an object.
        /// Use only if type is unknown at compile time.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="xml">The object serialized as xml.</param>
        /// <returns>The deserialized object.</returns>
        public static BusMessage Deserialize(Type messageType, string xml)
        {
            if (!SubscriptionValidator.IsValidSubscription(messageType) || String.IsNullOrEmpty(xml))
            {
                Logger.Error("Could not deserialize message because it was not valid or xml was null/empty.");
                return null;
            }

            DeserializeHandler<BusMessage> deserializeHandler = Deserialize<BusMessage>;
            MethodInfo genericMethod = deserializeHandler.Method.GetGenericMethodDefinition().MakeGenericMethod(messageType);

            BusMessage deserializedMessage = null;

            try
            {
                object deserialized = genericMethod.Invoke(null, new object[] { xml });
                deserializedMessage = deserialized as BusMessage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return deserializedMessage;
        }

        /// <summary>
        /// Deserializes xml to an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="xml">The xml.</param>
        /// <returns>The type of the object to return.</returns>
        public static T Deserialize<T>(string xml)
        {
            MemoryStream memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
            DataContractSerializer dataContractSerializer = GetDataContractSerializer(typeof (T));

            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8);

            T deserialized = default(T);

            try
            {
                deserialized = (T)dataContractSerializer.ReadObject(xmlTextWriter.BaseStream);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            memoryStream.Close();
            memoryStream.Dispose();

            return deserialized;
        }

        /// <summary>
        /// Deserializes an xml file to an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="filePath">The file path of the xml file. Can be relative.</param>
        /// <returns>The type of the object to return.</returns>
        public static T DeserializeFromFile<T>(string filePath)
        {
            if (filePath.StartsWith("/"))
                filePath = String.Format("{0}{1}", Directory.GetCurrentDirectory(), filePath);

            if (!File.Exists(filePath))
                return default(T);

            string xml = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

            return Deserialize<T>(xml);
        }
    }
}
