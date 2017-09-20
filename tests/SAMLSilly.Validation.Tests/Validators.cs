using System;
using System.Collections.Generic;
using System.Linq;

namespace SAMLSilly.Validation.Tests
{
    public class ValidationObject<T>
    {
        private ValidationResult<T>[] _validationResults;

        public ValidationResult<T>[] ValidationResults => _validationResults;

        public ValidationObject<T> Validate(T @object)
        {
            return this;
        }

        private IValidate<T>[] GetObjectValidators()
        {
            return new IValidate<T>[] { };
        }
    }


    public interface IValidate<T>
    {
        // ValidationResult<T> Validate(T @object, IValidatorOptions<T> options);
    }

    public interface IValidate<T, TOptions> : IValidate<T> where TOptions : IValidatorOptions<T>
    {
        ValidationResult<T> Validate(T @object, TOptions options);
    }

    public interface IValidatorOptions<T>
    {
        ValidationMessage[] IgnoredMessages { get; }
    }

    public class ValidationResult<T>
    {
        private bool? _isValid = null;
        public ValidationResult(T @object)
        {
            ValidatedObject = @object;
        }

        public ValidationResult(T @object, bool failOnFirstError) : this(@object)
        {
            FailOnFirstError = failOnFirstError;
        }

        public ValidationResult<T> SetOptions(IValidatorOptions<T> options)
        {
            this.Options = options;
            return this;
        }

        public bool FailOnFirstError { get; }
        public T ValidatedObject { get; }
        public List<ValidationMessage> ValidationMessages { get; } = new List<ValidationMessage>();
        public virtual bool IsValid
        {
            get
            {

                return !ValidationMessages.Where(y => !this.Options.IgnoredMessages.Contains(y)).Any(x => x.Failed);
            }
        }

        public bool Failed { get; private set; }

        public IValidatorOptions<T> Options { get; private set; }

        public ValidationResult<T> AddValidationMessage(ValidationMessage message)
        {
            ValidationMessages.Add(message);

            if (FailOnFirstError)
            {
                if (message.Failed && IsValid) Failed = false;
            }

            return this;
        }
    }

    public class ValidationMessage
    {
        public ValidationMessage(string name, string friendlyMessage, bool failed)
        {
            Name = name;
            Reason = friendlyMessage;
            FriendlyMessage = friendlyMessage;
            Failed = failed;
        }
        public ValidationMessage(string name, string friendlyMessage, string reason, bool failed)
            : this(name, friendlyMessage, failed)
        {
            Reason = reason;
        }

        public string FriendlyMessage { get; private set; }
        public string Reason { get; private set; }
        public string Name { get; private set; }
        public bool Failed { get; private set; }
    }
}
