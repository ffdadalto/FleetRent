using System.ComponentModel.DataAnnotations;

namespace FleetRent.Application.Common.Validtaions
{
    public class DateValidation : ValidationAttribute
    {
        public override bool IsValid(object value) => value is null ? true : ((DateOnly)value) > new DateOnly(1800, 1, 1);
    }
}