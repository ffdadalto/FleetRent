using System.ComponentModel.DataAnnotations;

namespace FleetRent.Application.Common.Validtaions
{
    public class DateTimeValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value) => value is null ? true : ((DateTime)value) > new DateTime(1800, 1, 1);
    }
}