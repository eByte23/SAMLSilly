using System;
using System.Security.Cryptography;

namespace SAMLSilly.Utils
{
#if !NETSTANDARD2_0
    /// <summary>
    /// Used to validate SHA256 signatures
    /// </summary>
    public class RSAPKCS1SHA256SignatureDescription : SignatureDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RSAPKCS1SHA256SignatureDescription"/> class.
        /// </summary>
        public RSAPKCS1SHA256SignatureDescription()
        {
            KeyAlgorithm = typeof(RSACryptoServiceProvider).FullName;
            DigestAlgorithm = typeof(SHA256Managed).FullName;   // Note - SHA256CryptoServiceProvider is not registered with CryptoConfig
            FormatterAlgorithm = typeof(RSAPKCS1SignatureFormatter).FullName;
            DeformatterAlgorithm = typeof(RSAPKCS1SignatureDeformatter).FullName;
        }

        /// <summary>
        /// Creates signature deformatter
        /// </summary>
        /// <param name="key">The key to use in the <see cref="T:System.Security.Cryptography.AsymmetricSignatureDeformatter" />.</param>
        /// <returns>The newly created <see cref="T:System.Security.Cryptography.AsymmetricSignatureDeformatter" /> instance.</returns>
        public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
        {
            if (key == null)
                throw new ArgumentNullException("RSAPKCS1SHA256SignatureDescription AsymmetricAlgorithm param: key is null");

            RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(key);
            deformatter.SetHashAlgorithm("SHA256");
            return deformatter;
        }

        public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
        {
            if (key == null)
                throw new ArgumentNullException("RSAPKCS1SHA256SignatureDescription AsymmetricAlgorithm param: key is null");

            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(key);
            formatter.SetHashAlgorithm("SHA256");
            return formatter;
        }
    }
#endif
}