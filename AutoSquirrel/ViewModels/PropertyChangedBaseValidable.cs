namespace AutoSquirrel
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;
    using FluentValidation.Results;

    /// <summary>
    /// Extend PropertyChangedBase with Validation Behaviour
    /// </summary>
    public class PropertyChangedBaseValidable : PropertyChangedBase, IDataErrorInfo
    {
        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error => GetError(Validate());

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => Validate().IsValid;

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified column name.
        /// </summary>
        /// <value>The <see cref="System.String"/>.</value>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                ValidationResult __ValidationResults = Validate();
                if (__ValidationResults == null) {
                    return string.Empty;
                }

                ValidationFailure __ColumnResults = __ValidationResults.Errors.FirstOrDefault(x => string.Compare(x.PropertyName, columnName, true) == 0);
                return __ColumnResults != null ? __ColumnResults.ErrorMessage : string.Empty;
            }
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static string GetError(ValidationResult result)
        {
            var __ValidationErrors = new StringBuilder();
            foreach (ValidationFailure validationFailure in result.Errors) {
                __ValidationErrors.Append(validationFailure.ErrorMessage);
                __ValidationErrors.Append(Environment.NewLine);
            }
            return __ValidationErrors.ToString();
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public virtual ValidationResult Validate() => new ValidationResult();
    }
}