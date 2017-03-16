using System;
using SAMLSilly.Utils;
using Xunit;

namespace SAMLSilly.Tests.Utils
{
    /// <summary>
    /// <see cref="Saml20Utils"/>  tests.
    /// </summary>

    public class Saml20UtilsTests
    {
        /// <summary>
        /// <c>FromUtcString</c> method tests.
        /// </summary>

        public class FromUtcStringMethod
        {
            /// <summary>
            /// Verify can convert UTC formatted string.
            /// </summary>
            [Fact]
            public void CanConvertString()
            {
                // Arrange
                var now = DateTime.UtcNow;
                var localtime = now.ToString("o");

                // Act
                var result = Saml20Utils.FromUtcString(localtime);

                // Assert
                Assert.Equal(now, result);
            }

            /// <summary>
            /// Verify <see cref="Saml20FormatException"/> is thrown on failure.
            /// </summary>
            [Fact]

            public void ThrowsSaml20FormatExceptionOnFailure()
            {
                // Arrange
                var localtime = DateTime.UtcNow.ToString();
                
                // Assert
                Assert.Throws(typeof(Saml20FormatException), () => {
                    // Act
                    Saml20Utils.FromUtcString(localtime);
                    //Assert.Fail("Conversion from non-UTC string must not succeed");
                });
            }
        }

        /// <summary>
        /// <c>ToUtcString</c> method tests.
        /// </summary>

        public class ToUtcStringMethod
        {
            /// <summary>
            /// Verify can convert UTC formatted string.
            /// </summary>
            [Fact]
            public void CanConvertToString()
            {
                // Arrange
                var now = DateTime.Parse("2015-02-04T20:43:40.8618531Z");
                var localtime = now.ToString("o");

                // Act
                var result = Saml20Utils.ToUtcString(now);

                // Assert
                Assert.Equal("2015-02-04T20:43:40.8618531Z", result);
            }
        }
    }
}
