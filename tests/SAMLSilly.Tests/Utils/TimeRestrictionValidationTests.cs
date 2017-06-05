using System;
using Xunit;

namespace SAMLSilly.Tests.Utils
{
    public class TimeRestrictionValidationTests
    {
        [Fact]
        public void not_before_should_be_true_when_before_is_before_now_supplied()
        {
            DateTime now = DateTime.Now;
            DateTime notBefore = now.Subtract(TimeSpan.FromHours(2));
            bool result = SAMLSilly.Utils.TimeRestrictionValidation.NotBeforeValid(notBefore, now, TimeSpan.Zero);

            Assert.True(result, "Not Before should be valid when datetime is before now with no clockskew");
        }

        [Fact]
        public void not_before_should_be_false_when_before_is_after_now_supplied()
        {
            DateTime now = DateTime.Now;
            DateTime notBefore = now.Add(TimeSpan.FromHours(2));
            bool result = SAMLSilly.Utils.TimeRestrictionValidation.NotBeforeValid(notBefore, now, TimeSpan.Zero);

            Assert.False(result, "Not before should fail when NotBefore datetime is after Now");
        }

        [Fact]
        public void not_before_should_be_true_when_before_is_after_now_supplied_but_clock_added()
        {
            DateTime now = DateTime.Now;
            DateTime notBefore = now.Add(TimeSpan.FromHours(2));
            bool result = SAMLSilly.Utils.TimeRestrictionValidation.NotBeforeValid(notBefore, now, TimeSpan.FromHours(3));

            Assert.True(result, "Not before should not fail when NotBefore datetime is after Now and correct clockskew is added");
        }

        [Fact]
        public void Onbefore_should_be_true_when_before_is_after_now_supplied_but_clock_added()
        {
            DateTime now = DateTime.Now;
            DateTime notOnOrAfter = now.Add(TimeSpan.FromHours(2));
            bool result = SAMLSilly.Utils.TimeRestrictionValidation.NotOnOrAfterValid(notOnOrAfter, now, TimeSpan.Zero);

            Assert.True(result, "Not before should not fail when NotBefore datetime is after Now and correct clockskew is added");
        }
    }
}