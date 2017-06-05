using System;
using System.IO;
using SAMLSilly.Config;

namespace SAMLSilly.Utils
{
    public class FileLoadUtils
    {
        public static Stream GetStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Open);
        }

        public static bool TryGetStream(string filePath, out Stream stream)
        {
            if (!File.Exists(filePath))
            {
                stream = null;
                return false;
            }

            try
            {
                stream = GetStream(filePath);
            }
            catch (Exception)
            {
                stream = null;
                return false;
            }

            return true;
        }
    }
}