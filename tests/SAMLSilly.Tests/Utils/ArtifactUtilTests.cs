using System;
using SAMLSilly.Utils;
using Xunit;

namespace SAMLSilly.Tests.Utils
{
    /// <summary>
    /// <see cref="ArtifactUtil"/> tests.
    /// </summary>

    public class ArtifactUtilTests
    {
        /// <summary>
        /// CreateArtifact method tests.
        /// </summary>

        public class CreateArtifactMethod
        {
            /// <summary>
            /// Verify a created artifact can be parsed.
            /// </summary>
            [Fact]
            public void CanParseCreatedArtifact()
            {
                // Arrange
                var sourceIdUrl = "https://kleopatra.safewhere.local/Saml2ExtWeb/artifact.ashx";

                var sourceIdHash = ArtifactUtil.GenerateSourceIdHash(sourceIdUrl);
                var messageHandle = ArtifactUtil.GenerateMessageHandle();

                short typeCode = 4;
                short endpointIndex = 1;

                // Act
                var artifact = ArtifactUtil.CreateArtifact(typeCode, endpointIndex, sourceIdHash, messageHandle);

                short parsedTypeCode = -1;
                short parsedEndpointIndex = -1;
                var parsedSourceIdHash = new byte[20];
                var parsedMessageHandle = new byte[20];

                var result = ArtifactUtil.TryParseArtifact(artifact, ref parsedTypeCode, ref parsedEndpointIndex, ref parsedSourceIdHash, ref parsedMessageHandle);

                // Assert
                Assert.True(result, "Unable to parse artifact");
                Assert.True(typeCode == parsedTypeCode, "Original and parsed typeCode did not match");
                Assert.True(endpointIndex == parsedEndpointIndex, "Original and parsed endpointIndex did not match");

                Assert.Equal(sourceIdHash, parsedSourceIdHash);

                //Assert.Fail("Original and parsed sourceIdHash are not identical");

                Assert.Equal(messageHandle, parsedMessageHandle);
                //Assert.Fail("Original and parsed messageHandle are not identical");
            }

            /// <summary>
            /// Verify exception is thrown on message handle length mismatch.
            /// </summary>
            [Fact]
            public void ThrowsExceptionWhenMessageHandleLengthMismatch()
            {
                // Arrange
                short typeCode = 4;
                short endpointIndex = 1;
                var sourceIdHash = new byte[20];
                var messageHandle = new byte[19];

                // Act
                Assert.Throws(typeof(ArgumentException), () =>
                {
                    ArtifactUtil.CreateArtifact(typeCode, endpointIndex, sourceIdHash, messageHandle);
                });
            }

            /// <summary>
            /// Verify exception is thrown on source id hash length mismatch.
            /// </summary>
            [Fact]
            public void ThrowsExceptionWhenSourceIdHashLengthMismatch()
            {
                // Arrange
                short typeCode = 4;
                short endpointIndex = 1;
                var sourceIdHash = new byte[19];
                var messageHandle = new byte[20];

                // Act
                Assert.Throws(typeof(ArgumentException), () =>
                {
                    ArtifactUtil.CreateArtifact(typeCode, endpointIndex, sourceIdHash, messageHandle);
                });
            }
        }

        /// <summary>
        /// ParseArtifact method tests.
        /// </summary>

        public class ParseArtifactMethod
        {
            /// <summary>
            /// Verify exception is thrown on source id hash length mismatch.
            /// </summary>
            [Fact]
            public void ThrowsExceptionWhenSourceIdHashLengthMismatch()
            {
                // Arrange
                short parsedTypeCode = -1;
                short parsedEndpointIndex = -1;
                var parsedSourceIdHash = new byte[19];
                var parsedMessageHandle = new byte[20];
                var artifact = string.Empty;

                // Act
                Assert.Throws(typeof(ArgumentException), () =>
                {
                    ArtifactUtil.ParseArtifact(artifact, ref parsedTypeCode, ref parsedEndpointIndex, ref parsedSourceIdHash, ref parsedMessageHandle);
                });
            }

            /// <summary>
            /// Verify exception is thrown on message handle length mismatch.
            /// </summary>
            [Fact]
            public void ThrowsExceptionWhenMessageHandleLengthMismatch()
            {
                // Arrange
                short parsedTypeCode = -1;
                short parsedEndpointIndex = -1;
                var parsedSourceIdHash = new byte[20];
                var parsedMessageHandle = new byte[19];
                var artifact = string.Empty;

                // Act
                Assert.Throws(typeof(ArgumentException), () =>
                {
                    ArtifactUtil.ParseArtifact(artifact, ref parsedTypeCode, ref parsedEndpointIndex, ref parsedSourceIdHash, ref parsedMessageHandle);
                });
            }

            /// <summary>
            /// Verify exception is thrown on artifact length mismatch.
            /// </summary>
            [Fact]
            public void ThrowsExceptionWhenArtifactLengthMismatch()
            {
                // Arrange
                short parsedTypeCode = -1;
                short parsedEndpointIndex = -1;
                var parsedSourceIdHash = new byte[20];
                var parsedMessageHandle = new byte[20];
                var artifact = string.Empty;

                // Act
                Assert.Throws(typeof(ArgumentException), () =>
                {
                    ArtifactUtil.ParseArtifact(artifact, ref parsedTypeCode, ref parsedEndpointIndex, ref parsedSourceIdHash, ref parsedMessageHandle);
                });
            }
        }

        /// <summary>
        /// TryParseArtifact method tests.
        /// </summary>

        public class TryParseArtifact
        {
            /// <summary>
            /// Verify returns false on artifact length mismatch.
            /// </summary>
            [Fact]
            public void ReturnsFalseOnArtifactLengthMismatch()
            {
                // Arrange
                short parsedTypeCode = -1;
                short parsedEndpointIndex = -1;
                var parsedSourceIdHash = new byte[20];
                var parsedMessageHandle = new byte[20];
                var artifact = string.Empty;

                // Act
                var result = ArtifactUtil.TryParseArtifact(artifact, ref parsedTypeCode, ref parsedEndpointIndex, ref parsedSourceIdHash, ref parsedMessageHandle);

                // Assert
                Assert.True(!result, "TryParseArtifact did not fail as expected");
            }

            /// <summary>
            /// Verify returns false on source id hash length mismatch.
            /// </summary>
            [Fact]
            public void ReturnsFalseOnSourceIdHashLengthMismatch()
            {
                // Arrange
                short parsedTypeCode = -1;
                short parsedEndpointIndex = -1;
                var parsedSourceIdHash = new byte[19];
                var parsedMessageHandle = new byte[20];
                var artifact = string.Empty;

                // Act
                var result = ArtifactUtil.TryParseArtifact(artifact, ref parsedTypeCode, ref parsedEndpointIndex, ref parsedSourceIdHash, ref parsedMessageHandle);

                // Assert
                Assert.True(!result, "TryParseArtifact did not fail as expected");
            }

            /// <summary>
            /// Verify returns false on message handle length mismatch.
            /// </summary>
            [Fact]
            public void ReturnsFalseOnMessageHandleLengthMismatch()
            {
                // Arrange
                short parsedTypeCode = -1;
                short parsedEndpointIndex = -1;
                var parsedSourceIdHash = new byte[20];
                var parsedMessageHandle = new byte[19];
                var artifact = string.Empty;

                // Act
                var result = ArtifactUtil.TryParseArtifact(artifact, ref parsedTypeCode, ref parsedEndpointIndex, ref parsedSourceIdHash, ref parsedMessageHandle);

                // Assert
                Assert.True(!result, "TryParseArtifact did not fail as expected");
            }
        }
    }
}
