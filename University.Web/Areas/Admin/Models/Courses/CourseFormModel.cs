namespace University.Web.Areas.Admin.Models.Courses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using University.Common.Mapping;
    using University.Data;
    using University.Services.Admin.Models.Courses;
    using University.Web.Models;

    public class CourseFormModel : IValidatableObject, IMapFrom<AdminCourseServiceModel>
    {
        [Required]
        [StringLength(DataConstants.CourseNameMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.CourseDescriptionMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Trainer")]
        public string TrainerId { get; set; }

        [IgnoreMap]
        public IEnumerable<SelectListItem> Trainers { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [IgnoreMap]
        public FormActionEnum Action { get; set; } = FormActionEnum.Create;

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            // Input & Input Validation in local time
            var hasStartBeforeToday = this.StartDate.Date < DateTime.Now.Date; // compare days only
            var hasEndBeforeStart = this.EndDate < this.StartDate;

            if (hasStartBeforeToday)
            {
                yield return new ValidationResult(DataConstants.CourseStartDate,
                    new[] { nameof(this.StartDate) });
            }

            if (hasEndBeforeStart)
            {
                yield return new ValidationResult(DataConstants.CourseEndDate,
                    new[] { nameof(this.EndDate) });
            }
        }
    }
}
