using System;
using System.IO;
using System.Linq;
using SAMLSilly.Schema.Core;
using SAMLSilly.Tests;
using SAMLSilly.Utils;
using Xunit;

namespace SAMLSilly.Validation.Tests
{
    public class Test1 : IClassFixture<TestContext>
    {
        private readonly TestContext context;
        public Test1(TestContext context)
        {
            this.context = context;

        }

        [Fact]
        public void TestValidatingSomething()
        {
            var assertion = new Saml20Assertion(AssertionUtil.LoadXmlDocument(Path.Combine("Assertions", "Saml2Assertion_03")).DocumentElement, null, false, TestConfiguration.Configuration);
            //assertion.Assertion;

            var result = new AssertionClockSkewValidator().Validate(assertion.Assertion, new Saml20AssertionValidatorOptions()
            {
                AllowedClockSkew = TimeSpan.FromHours(1),
                IgnoredMessages = new ValidationMessage[]
                {
                    AssertionClockSkewValidator.AuthnStatement_SessionNotOnOrAfter,
                    AssertionClockSkewValidator.Conditions_NotOnOrAfter,
                    AssertionClockSkewValidator.SubjectConfirmationData_NotOnOrAfter
                }
        });

            Assert.True(result.IsValid);
        }








}

public class AssertionClockSkewValidator : IValidate<Assertion, Saml20AssertionValidatorOptions>
{
    public ValidationResult<Assertion> Validate(Assertion @object, Saml20AssertionValidatorOptions options)
    {
        var validationResult = new ValidationResult<Assertion>(@object, false)
            .SetOptions(options);

        if (@object.Conditions == null)
        {
            return validationResult;
        }

        var conditions = @object.Conditions;
        var now = DateTime.UtcNow;

        // Negative allowed clock skew does not make sense - we are trying to relax the restriction interval, not restrict it any further
        var allowedClockSkew = options.AllowedClockSkew;
        if (allowedClockSkew < TimeSpan.Zero)
        {
            allowedClockSkew = allowedClockSkew.Negate();
        }

        if (!ValidateNotBefore(conditions.NotBefore, now, allowedClockSkew))
        {
            //throw new Saml20FormatException("Conditions.NotBefore must not be in the future");
            validationResult.AddValidationMessage(Conditions_NotBefore);
        }

        // NotOnOrAfter must not be in the past
        if (!ValidateNotOnOrAfter(conditions.NotOnOrAfter, now, allowedClockSkew))
        {
            //throw new Saml20FormatException("Conditions.NotOnOrAfter must not be in the past");
            validationResult.AddValidationMessage(Conditions_NotOnOrAfter);
        }

        foreach (var statement in @object.GetAuthnStatements())
        {
            if (statement.SessionNotOnOrAfter != null && statement.SessionNotOnOrAfter <= now)
            {
                //throw new Saml20FormatException("AuthnStatement attribute SessionNotOnOrAfter MUST be in the future");
                validationResult.AddValidationMessage(AuthnStatement_SessionNotOnOrAfter);
            }

            // TODO: Consider validating that authnStatement.AuthnInstant is in the past
        }

        if (@object.Subject != null)
        {
            foreach (var subjectConfirmation in @object.Subject.Items.OfType<SubjectConfirmation>().Where(subjectConfirmation => subjectConfirmation.SubjectConfirmationData != null))
            {
                if (!ValidateNotBefore(subjectConfirmation.SubjectConfirmationData.NotBefore, now, allowedClockSkew))
                {
                    //throw new Saml20FormatException("SubjectConfirmationData.NotBefore must not be in the future");
                    validationResult.AddValidationMessage(SubjectConfirmationData_NotBefore);
                }

                if (!ValidateNotOnOrAfter(subjectConfirmation.SubjectConfirmationData.NotOnOrAfter, now, allowedClockSkew))
                {
                    //throw new Saml20FormatException("SubjectConfirmationData.NotOnOrAfter must not be in the past");
                    validationResult.AddValidationMessage(SubjectConfirmationData_NotOnOrAfter);
                }
            }


        }


        return validationResult;
    }

    //Validations Messages
    public static ValidationMessage Conditions_NotBefore = new ValidationMessage(
                "Conditions.NotBefore",
                "Conditions.NotBefore must not be in the future",
                true);

    public static ValidationMessage Conditions_NotOnOrAfter = new ValidationMessage(
                "Conditions.NotOnOrAfter",
                "Conditions.NotOnOrAfter must not be in the past",
                true);
    public static ValidationMessage SubjectConfirmationData_NotOnOrAfter = new ValidationMessage(
                "SubjectConfirmationData.NotOnOrAfter",
                "SubjectConfirmationData.NotOnOrAfter must not be in the past",
                true);

    public static ValidationMessage SubjectConfirmationData_NotBefore = new ValidationMessage(
                "SubjectConfirmationData.NotBefore",
                "SubjectConfirmationData.NotBefore must not be in the future",
                true);

    public static ValidationMessage AuthnStatement_SessionNotOnOrAfter = new ValidationMessage(
                "AuthnStatement.SessionNotOnOrAfter",
                "AuthnStatement attribute SessionNotOnOrAfter MUST be in the future",
                    true);

    /// <summary>
    /// If both conditions.NotBefore and conditions.NotOnOrAfter are specified, NotBefore
    /// MUST BE less than NotOnOrAfter
    /// </summary>
    /// <param name="conditions">The conditions.</param>
    /// <exception cref="Saml20FormatException">If <param name="conditions"/>.NotBefore is not less than <paramref name="conditions"/>.NotOnOrAfter</exception>
    private static void ValidateConditionsInterval(Conditions conditions)
    {
        // No settings? No restrictions
        if (conditions.NotBefore == null && conditions.NotOnOrAfter == null)
        {
            return;
        }

        if (conditions.NotBefore != null && conditions.NotOnOrAfter != null && conditions.NotBefore.Value >= conditions.NotOnOrAfter.Value)
        {
            throw new Saml20FormatException(string.Format("NotBefore {0} MUST BE less than NotOnOrAfter {1} on Conditions", Saml20Utils.ToUtcString(conditions.NotBefore.Value), Saml20Utils.ToUtcString(conditions.NotOnOrAfter.Value)));
        }
    }

    /// <summary>
    /// Null fields are considered to be valid
    /// </summary>
    /// <param name="notBefore">The not before.</param>
    /// <param name="now">The now.</param>
    /// <param name="allowedClockSkew">The allowed clock skew.</param>
    /// <returns>True if the not before value is valid, else false.</returns>
    private static bool ValidateNotBefore(DateTime? notBefore, DateTime now, TimeSpan allowedClockSkew)
    {
        return notBefore == null || TimeRestrictionValidation.NotBeforeValid(notBefore.Value, now, allowedClockSkew);
    }

    /// <summary>
    /// Handle allowed clock skew by increasing notOnOrAfter with allowedClockSkew
    /// </summary>
    /// <param name="notOnOrAfter">The not on or after.</param>
    /// <param name="now">The now.</param>
    /// <param name="allowedClockSkew">The allowed clock skew.</param>
    /// <returns>True if the not on or after value is valid, else false.</returns>
    private static bool ValidateNotOnOrAfter(DateTime? notOnOrAfter, DateTime now, TimeSpan allowedClockSkew)
    {
        return notOnOrAfter == null || TimeRestrictionValidation.NotOnOrAfterValid(notOnOrAfter.Value, now, allowedClockSkew);
    }
}

public class Saml20AssertionValidatorOptions : IValidatorOptions<Assertion>
{
    public TimeSpan AllowedClockSkew { get; set; } = TimeSpan.Zero;
    public ValidationMessage[] IgnoredMessages { get; set; }
}

    // public void ValidateTimeRestrictions(Assertion assertion, TimeSpan allowedClockSkew)
    // {



    //
    // }








    // /// <summary>
    // /// Validates that all the required attributes are present on the assertion.
    // /// Furthermore it validates validity of the Issuer element.
    // /// </summary>
    // /// <param name="assertion">The assertion.</param>
    // private void ValidateAssertionAttributes(Assertion assertion)
    // {
    //     // There must be a Version
    //     if (!assertion.Version.ValidateRequiredString())
    //     {
    //         throw new Saml20FormatException("Assertion element must have the Version attribute set.");
    //     }

    //     // Version must be 2.0
    //     if (assertion.Version != Saml20Constants.Version)
    //     {
    //         throw new Saml20FormatException("Wrong value of version attribute on Assertion element");
    //     }

    //     // Assertion must have an ID
    //     if (!Saml20Utils.ValidateRequiredString(assertion.Id))
    //     {
    //         throw new Saml20FormatException("Assertion element must have the ID attribute set.");
    //     }

    //     // Make sure that the ID elements is at least 128 bits in length (SAML2.0 std section 1.3.4)
    //     if (!Saml20Utils.ValidateIdString(assertion.Id))
    //     {
    //         throw new Saml20FormatException("Assertion element must have an ID attribute with at least 16 characters (the equivalent of 128 bits)");
    //     }

    //     // IssueInstant must be set.
    //     if (!assertion.IssueInstant.HasValue)
    //     {
    //         throw new Saml20FormatException("Assertion element must have the IssueInstant attribute set.");
    //     }

    //     // There must be an Issuer
    //     if (assertion.Issuer == null)
    //     {
    //         throw new Saml20FormatException("Assertion element must have an issuer element.");
    //     }

    //     // The Issuer element must be valid
    //     _nameIdValidator.ValidateNameId(assertion.Issuer);
    // }

    // /// <summary>
    // /// Validates the Assertion's conditions
    // /// Audience restrictions processing rules are:
    // /// - Within a single audience restriction condition in the assertion, the service must be configured
    // /// with an audience-list that contains at least one of the restrictions in the assertion ("OR" filter)
    // /// - When multiple audience restrictions are present within the same assertion, all individual audience
    // /// restriction conditions must be met ("AND" filter)
    // /// </summary>
    // /// <param name="assertion">The assertion.</param>
    // private void ValidateConditions(Assertion assertion)
    // {
    //     // Conditions are not required
    //     if (assertion.Conditions == null)
    //     {
    //         return;
    //     }

    //     var oneTimeUseSeen = false;
    //     var proxyRestrictionsSeen = false;

    //     ValidateConditionsInterval(assertion.Conditions);

    //     foreach (var cat in assertion.Conditions.Items)
    //     {
    //         if (cat is OneTimeUse)
    //         {
    //             if (oneTimeUseSeen)
    //             {
    //                 throw new Saml20FormatException("Assertion contained more than one condition of type OneTimeUse");
    //             }

    //             oneTimeUseSeen = true;
    //             continue;
    //         }

    //         if (cat is ProxyRestriction)
    //         {
    //             if (proxyRestrictionsSeen)
    //             {
    //                 throw new Saml20FormatException("Assertion contained more than one condition of type ProxyRestriction");
    //             }

    //             proxyRestrictionsSeen = true;

    //             var proxyRestriction = (ProxyRestriction)cat;
    //             if (!string.IsNullOrEmpty(proxyRestriction.Count))
    //             {
    //                 uint res;
    //                 if (!uint.TryParse(proxyRestriction.Count, out res))
    //                 {
    //                     throw new Saml20FormatException("Count attribute of ProxyRestriction MUST BE a non-negative integer");
    //                 }
    //             }

    //             if (proxyRestriction.Audience != null)
    //             {
    //                 foreach (var audience in proxyRestriction.Audience)
    //                 {
    //                     if (!Uri.IsWellFormedUriString(audience, UriKind.Absolute))
    //                     {
    //                         throw new Saml20FormatException("ProxyRestriction Audience MUST BE a wellformed uri");
    //                     }
    //                 }
    //             }
    //         }

    //         // AudienceRestriction processing goes here (section 2.5.1.4 of [SAML2.0 standard])
    //         if (cat is AudienceRestriction)
    //         {
    //             // No audience restrictions? No problems...
    //             var audienceRestriction = (AudienceRestriction)cat;
    //             if (audienceRestriction.Audience == null || audienceRestriction.Audience.Count == 0)
    //             {
    //                 continue;
    //             }

    //             // If there are no allowed audience uris configured for the service, the assertion is not
    //             // valid for this service
    //             if (_allowedAudienceUris == null || _allowedAudienceUris.Count < 1)
    //             {
    //                 throw new Saml20FormatException("The service is not configured to meet any audience restrictions");
    //             }

    //             Uri match = null;
    //             foreach (var audience in audienceRestriction.Audience)
    //             {
    //                 //[Deprecated]
    //                 //This should be controlled by validation messages in validation object
    //                 // In QuirksMode this validation is omitted
    //                 //if (!_quirksMode)
    //                 //{
    //                 // The given audience value MUST BE a valid URI
    //                 if (!Uri.IsWellFormedUriString(audience, UriKind.Absolute))
    //                 {
    //                     throw new Saml20FormatException("Audience element has value which is not a wellformed absolute uri");
    //                 }
    //                 //}

    //                 match = _allowedAudienceUris.Find(allowedUri => allowedUri.Equals(new Uri(audience)));
    //                 if (match != null)
    //                 {
    //                     break;
    //                 }
    //             }

    //             // if (_logger.IsEnabled(LogLevel.Debug))
    //             // {
    //             //     var intended = string.Join(", ", audienceRestriction.Audience.ToArray());
    //             //     var allowed = string.Join(", ", _allowedAudienceUris.Select(u => u.ToString()).ToArray());
    //             //     _logger.LogDebug(TraceMessages.AudienceRestrictionValidated, intended, allowed);
    //             // }

    //             if (match == null)
    //             {
    //                 throw new Saml20FormatException("The service is not configured to meet the given audience restrictions");
    //             }
    //         }
    //     }
    // }


    //     public static ValidationMessage AudienceRestrictionValidationMessage = new ValidationMessage
    //     (
    //         nameof(Saml20Assertion),
    //         nameof(AudienceRestrictionValidationMessage),
    //         "The service is not configured to meet the given audience restrictions"
    //     );

    // }
}