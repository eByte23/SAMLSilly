using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SAMLSilly.Utils
{
    /// <summary>
    /// Functions for typed serialization and deserialization of objects.
    /// </summary>
    public static class Serialization
    {
        /// <summary>
        /// Initializes static members of the <see cref="Serialization"/> class.
        /// </summary>
        static Serialization()
        {
            XmlNamespaces = new XmlSerializerNamespaces();
            XmlNamespaces.Add("samlp", Saml20Constants.Protocol);
            XmlNamespaces.Add("saml", Saml20Constants.Assertion);
        }

        /// <summary>
        /// Gets the instance of XmlSerializerNamespaces that is used by this class.
        /// </summary>
        /// <value>The XmlSerializerNamespaces instance.</value>
        public static XmlSerializerNamespaces XmlNamespaces { get; private set; }

        /// <summary>
        /// Reads and deserializes an item from the reader
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <returns>The deserialized item.</returns>
        public static T Deserialize<T>(XmlReader reader)
        {
            var serializer = new XmlSerializer(typeof(T));
            var item = (T)serializer.Deserialize(reader);

            return item;
        }

        /// <summary>
        /// Deserializes an item from an XML string.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="xml">The XML.</param>
        /// <returns>The deserialized item.</returns>
        public static T DeserializeFromXmlString<T>(string xml)
        {
            using (var stream = new StringReader(xml))
            {
                var reader = new XmlTextReader(stream);
                return Deserialize<T>(reader);
            }
        }

        /// <summary>
        /// Serializes the specified item to a stream.
        /// </summary>
        /// <typeparam name="T">The items type</typeparam>
        /// <param name="item">The item to serialize.</param>
        /// <param name="stream">The stream to serialize to.</param>
        public static void Serialize<T>(T item, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, item, XmlNamespaces);
            stream.Flush();
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <typeparam name="T">The items type</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>An XmlDocument containing the serialized form of the item</returns>
        public static XmlDocument Serialize<T>(T item)
        {
            // create the XmlDocument to return
            var doc = new XmlDocument();
            using (var stream = new MemoryStream())
            {
                Serialize(item, stream);

                stream.Seek(0, SeekOrigin.Begin);
                doc.Load(stream);

                stream.Close();
            }

            return doc;
        }

        /// <summary>
        /// Serializes an item to an XML string.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>The serialized string.</returns>
        public static string SerializeToXmlString<T>(T item)
        {
            string serializedValue;
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            {
                Serialize(item, stream);

                stream.Seek(0, SeekOrigin.Begin);
                serializedValue = reader.ReadToEnd();

                reader.Close();
                stream.Close();
            }

            return serializedValue;
        }
    }
}
