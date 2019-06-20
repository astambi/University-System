namespace LearningSystem.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Course : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.CourseNameMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.CourseDescriptionMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string Description { get; set; }

        [Required]
        public string TrainerId { get; set; }

        public User Trainer { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        public ICollection<StudentCourse> Students { get; set; } = new List<StudentCourse>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasStartBeforeToday = DateTime.Compare(this.StartDate, DateTime.Now.Date) == -1;
            var hasEndBeforeStart = DateTime.Compare(this.EndDate, this.StartDate) == -1;

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
