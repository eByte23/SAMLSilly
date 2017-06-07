using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SAMLSilly.Utils
{
    public static class ExtentionMethods
    {
        //Make this compelation time varible
        /// <summary>
        /// Copies one stream to another stream
        /// </summary>
        /// <returns>A new stream</returns>
#if NET35
        public static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
#endif
    }
}
