using System;
using System.ComponentModel.DataAnnotations;

namespace API.Attributes
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        public MinimumAgeAttribute(int minAge)
        {
            _minAge = minAge;
            ErrorMessage = $"The Minimum Allowed Age is {minAge}";

        }
        public override bool IsValid(object value)
        {
            return (DateTime)value <= DateTime.UtcNow.AddYears(-_minAge);
        }
    }
}