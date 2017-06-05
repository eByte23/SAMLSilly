using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SAMLSilly.Tests.Certificates
{
    public class InMemoryResourceUtility
    {
        public static byte[] GetInMemoryResource(Type namespaceFromType, string filePath)
        {
            byte[] resource;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(namespaceFromType, filePath))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                resource = memoryStream.ToArray();

                stream.Close();
            }

            return resource;
        }

        public static byte[] GetInMemoryResource<T>(string filePath) => 
            GetInMemoryResource(typeof(T), filePath);

        public static byte[] GetInMemoryResource(string filePath) =>
            GetInMemoryResource<InMemoryResourceUtility>(filePath);        

        public static X509Certificate2 GetInMemoryCertificate(string filePath, string password)
        {
            return new X509Certificate2(GetInMemoryResource(filePath), password);
        }
    }
}