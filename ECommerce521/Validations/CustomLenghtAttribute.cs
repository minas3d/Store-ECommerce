using System.ComponentModel.DataAnnotations;

namespace ECommerce521.Validations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CustomLenghtAttribute : ValidationAttribute
    {
        private readonly int _minLenght;
        private readonly int _maxLenght;

        public CustomLenghtAttribute(int minLenght, int maxLenght)
        {
            _minLenght = minLenght;
            _maxLenght = maxLenght;
        }

        public override bool IsValid(object? value)
        {
            if(value is string s)
            {
                if (s.Length > _minLenght && s.Length < _maxLenght)
                    return true;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The field {name}, length must be between {_minLenght} and {_maxLenght}";
        }
    }
}
