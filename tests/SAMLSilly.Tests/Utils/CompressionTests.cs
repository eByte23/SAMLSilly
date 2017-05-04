using SAMLSilly.Utils;
using Xunit;

namespace SAMLSilly.Tests.Utils
{
    public class CompresssionTests
    {
        public const string DECODE_TEST_VALUE = "ValueHere";

        //Create In a third party library to ensure compat
        public const string ENCODE_TEST_VALUE = "C0vMKU31SC1KBQA=";

        [Fact]
        public void Deflate_should_correctly_encode_the_value()
        {
            Assert.Equal(ENCODE_TEST_VALUE, Compression.Deflate(DECODE_TEST_VALUE));
        }

        [Fact]
        public void Inflate_should_correctly_decode_the_value()
        {
            Assert.Equal(DECODE_TEST_VALUE, Compression.Inflate(ENCODE_TEST_VALUE));
        }

        [Fact]
        public void Should_be_able_to_encode_and_decode_with_our_methods()
        {
            Assert.Equal("A NEW VALUE", Compression.Inflate(Compression.Deflate("A NEW VALUE")));
        }
    }
}