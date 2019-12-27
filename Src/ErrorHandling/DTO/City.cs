using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.DTO
{
    public class City
    {
        [Required(ErrorMessage = "MandatoryField")]
        [MinLength(2,ErrorMessage = "FieldNotLongEnough")]
        [MaxLength(255, ErrorMessage = "FieldTooLong")]
        public string Name { get; set; }
        [Required(ErrorMessageResourceName = "MandatoryField")]
        [MinLength(2, ErrorMessage = "FieldNotLongEnough")]
        [MaxLength(3, ErrorMessage = "FieldTooLong")]
        public string CountryCode { get; set; }
    }
}
