using System;
using SAMLSilly.AspNetCore.BindingBuilders;
using Xunit;

namespace SAML2.Tests.Bindings
{
    /// <summary>
    /// <see cref="HttpRedirectBindingBuilder"/> tests.
    /// </summary>

    public class HttpRedirectBindingBuilderTests
    {
        /// <summary>
        /// Request property tests.
        /// </summary>

        public class RequestProperty
        {
            /// <summary>
            /// Ensure that it is not possible to add a request, when a response has already been added.
            /// </summary>
            [Fact(Skip = "We are fix this now")]
            public void DoesNotAllowResponseAndRequestToBothBeSet()
            {

                // Assert
                Assert.Throws(typeof(ArgumentException), () =>
                {
                    // Arrange
                    var binding = new HttpRedirectBindingBuilder()
                    {
                        Response = "Response"
                    };

                    // Act
                    binding.Request = "Request";

                });
            }
        }

        /// <summary>
        /// Response property tests
        /// </summary>

        public class ResponseProperty
        {
            /// <summary>
            /// Ensure that it is not possible to add a response, when a request has already been added.
            /// </summary>
            [Fact(Skip = "We are fix this now")]
            //"HttpRedirectBinding did not throw an exception when both Request and Response were set."
            public void DoesNotAllowRequestAndResponseToBothBeSet()
            {
                // Arrange
                var binding = new HttpRedirectBindingBuilder()
                {
                    Request = "Request"
                };

                // Assert
                Assert.Throws(typeof(ArgumentException), () =>
                {
                    // Act
                    binding.Response = "Response";
                });
            }
        }

        /// <summary>
        /// ToQuery method tests.
        /// </summary>

        public class ToQueryMethod
        {
            /// <summary>
            /// Tests that when using the builder to create a response, the relay state is not encoded.
            /// </summary>
           [Fact(Skip = "We are fix this now")]
            public void DoesNotEncodeRelayStateForResponse()
            {
                // Arrange
                var relaystate = string.Empty.PadRight(10, 'A');
                var bindingBuilder = new HttpRedirectBindingBuilder()
                {
                    RelayState = relaystate,
                    Response = "A random response... !!!! .... "
                };

                // Act
                var query = bindingBuilder.ToQuery();

                // Assert
                Assert.True(query.Contains(relaystate));
            }

            /// <summary>
            /// Tests that when using the builder to create a request, the relay state is encoded.
            /// </summary>
           [Fact(Skip = "We are fix this now")]
            public void EncodesRelayStateForRequests()
            {
                // Arrange
                var relaystate = string.Empty.PadRight(10, 'A');
                var bindingBuilder = new HttpRedirectBindingBuilder()
                {
                    Request = "A random request... !!!! .... ",
                    RelayState = relaystate
                };

                // Act
                var query = bindingBuilder.ToQuery();

                // Assert
                Assert.True(!query.Contains(relaystate));
            }
        }
    }
}
