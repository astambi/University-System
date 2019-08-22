namespace University.Web.Models.Trainers
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc;
    using University.Data;

    public class StudentCourseGradeFormModel
    {
        [Required]
        [HiddenInput]
        public string StudentId { get; set; }

        [HiddenInput]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Grade")]
        [Range(2, 6,
            ErrorMessage = DataConstants.RangeMinMaxValues)]
        public decimal? GradeBg { get; set; }
    }
}
