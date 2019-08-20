namespace University.Web.Models.Trainers
{
    using System.ComponentModel.DataAnnotations;
    using University.Data.Models;
    using Microsoft.AspNetCore.Mvc;

    public class StudentCourseGradeFormModel
    {
        [Required]
        [HiddenInput]
        public string StudentId { get; set; }

        [HiddenInput]
        public int CourseId { get; set; }

        [Required]
        public Grade? Grade { get; set; }
    }
}
