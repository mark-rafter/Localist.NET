using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Localist.Shared
{
    public class NewPostModel
    {
        [Required, EnumDataType(typeof(PostType))]
        public PostType Type { get; set; } = PostType.Message;

        [EnumDataType(typeof(ExchangeType))]
        public ExchangeType ExchangeType { get; set; }

        [Required]
        [StringLength(120, ErrorMessage = "Title is too long.")]
        public string Title { get; set; } = default!;

        [Range(0, 999_999_999)]
        public decimal? Price { get; set; }

        [StringLength(25, ErrorMessage = "Unit is too long.")]
        public string? Unit { get; set; }

        public bool IsAnonymous { get; set; }

        [StringLength(2000, ErrorMessage = "Description is too long.")]
        public string? Description { get; set; }

        /// <remarks>
        /// Base64 data is only crudely validated, and could be malicious.
        /// Do not attempt to execute or render it.
        /// </remarks>
        [ImageValidation]
        public IList<string> Base64Images { get; set; } = new List<string>();

        public bool EnableNotifications { get; set; }
    }

    public class ImageValidationAttribute : ValidationAttribute
    {
        // for file signatures: https://en.wikipedia.org/wiki/List_of_file_signatures
        // convert to base64 with https://base64.guru/converter/encode/hex
        const int maxBase64Length = 2_000_000; // = 1_500_000 / 0.75 (base64 = 6 bits per char)

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return null;

            if (value is IList<string> base64Images)
            {
                foreach (var base64Image in base64Images)
                {
                    if (base64Image.Length > maxBase64Length)
                        return new ValidationResult("File is too large.", new List<string> { validationContext.MemberName! });

                    if (base64Image.Length < 6)
                        return new ValidationResult("File is empty.", new List<string> { validationContext.MemberName! });

                    var data = base64Image.Substring(0, 5);

                    switch (data.ToUpper())
                    {
                        case "IVBOR": // .png
                        case "/9J/4": // .jpg
                            continue;
                        default:
                            return new ValidationResult("File is not a valid format.", new List<string> { validationContext.MemberName! });
                    }
                }

                return ValidationResult.Success!;
            }

            return new ValidationResult($"File is invalid type: {value.GetType()}.", new List<string> { validationContext.MemberName! });
        }
    }
}
