using System.Collections.Generic;
using SAMLSilly.Schema.Metadata;

namespace SAMLSilly.Bindings
{
    public interface IHttpBindingParser
    {
        bool IsRequest { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is response.
        /// </summary>
        bool IsResponse { get;  }

        /// <summary>
        /// Gets a value indicating whether the message is signed.
        /// </summary>
        bool IsSigned { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        string Message { get; }

        bool VerifySignature(IEnumerable<KeyDescriptor> keys);

    }
}
