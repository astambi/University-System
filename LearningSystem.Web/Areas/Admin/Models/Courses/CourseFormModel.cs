namespace LearningSystem.Web.Areas.Admin.Models.Courses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data;
    using LearningSystem.Services.Admin.Models;
    using LearningSystem.Web.Models;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class CourseFormModel : IValidatableObject, IMapFrom<CourseEditServiceModel>
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

        [IgnoreMap]
        public FormAction Action { get; set; } = FormAction.Create;

        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var compare = DateTime.Compare(this.StartDate, this.EndDate);
            if (compare == 1) // End date before Start date
            {
                yield return new ValidationResult(DataConstants.CourseEndDate,
                    new[] { nameof(this.EndDate) });
            }
        }
    }
}
