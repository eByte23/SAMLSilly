using System;
using System.Security.Cryptography;

namespace SAMLSilly.Utils
{
#if !NETSTANDARD2_0
    /// <summary>
    /// Used to validate SHA512 signatures
    /// </summary>
    public class RSAPKCS1SHA512SignatureDescription : SignatureDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RSAPKCS1SHA512SignatureDescription"/> class.
        /// </summary>
        public RSAPKCS1SHA512SignatureDescription()
        {
            KeyAlgorithm = typeof(RSACryptoServiceProvider).FullName;
            DigestAlgorithm = typeof(SHA512Managed).FullName;   // Note - SHA512CryptoServiceProvider is not registered with CryptoConfig
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
                throw new ArgumentNullException("RSAPKCS1SHA512SignatureDescription AsymmetricAlgorithm param: key is null");

            RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(key);
            deformatter.SetHashAlgorithm("SHA512");
            return deformatter;
        }

        public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
        {
            if (key == null)
                throw new ArgumentNullException("RSAPKCS1SHA512SignatureDescription AsymmetricAlgorithm param: key is null");

            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(key);
            formatter.SetHashAlgorithm("SHA512");
            return formatter;
        }
    }
#endif
}