namespace LearningSystem.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Admin.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Exams;
    using LearningSystem.Services.Models.Orders;
    using LearningSystem.Services.Models.Resources;
    using LearningSystem.Services.Models.Users;
    using LearningSystem.Web;
    using LearningSystem.Web.Areas.Admin.Models.Courses;
    using LearningSystem.Web.Models;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class Tests
    {
        static Tests()
        {
            InitializeMapper();
        }

        public static IMapper Mapper { get; private set; }

        private static void InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                //cfg.AddProfile<AutoMapperProfile>();

                cfg.CreateMap<AdminCourseServiceModel, CourseFormModel>();
                cfg.CreateMap<Course, CourseDetailsServiceModel>();
                cfg.CreateMap<Course, CourseServiceModel>();
                cfg.CreateMap<Course, CourseWithDescriptionServiceModel>();
                cfg.CreateMap<ExamSubmission, ExamDownloadServiceModel>();
                cfg.CreateMap<ExamSubmission, ExamSubmissionServiceModel>();
                cfg.CreateMap<Order, OrderListingServiceModel>();
                cfg.CreateMap<OrderItem, OrderItemServiceModel>();
                cfg.CreateMap<Resource, CourseResourceServiceModel>();
                cfg.CreateMap<Resource, ResourceDownloadServiceModel>();
                cfg.CreateMap<User, UserBasicServiceModel>();
                cfg.CreateMap<User, UserServiceModel>();
            });

            Mapper = config.CreateMapper();
        }

        public static LearningSystemDbContext InitializeDatabase()
        {
            var dbOptions = new DbContextOptionsBuilder<LearningSystemDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LearningSystemDbContext(dbOptions);
        }

        public static IEnumerable<CourseServiceModel> GetCourseServiceModelCollection()
            => new List<CourseServiceModel>()
            {
                new CourseServiceModel{ Id = 1, Name ="Name1", StartDate = new DateTime(2019, 1, 12), EndDate = new DateTime(2019, 3, 11), TrainerId = "1", TrainerName="Trainer1" },
                new CourseServiceModel{ Id = 2, Name ="Name1", StartDate = new DateTime(2019, 3, 10), EndDate = new DateTime(2019, 4, 13), TrainerId = "2", TrainerName="Trainer2" },
                new CourseServiceModel{ Id = 3, Name ="Name1", StartDate = new DateTime(2019, 5, 11), EndDate = new DateTime(2019, 5, 17), TrainerId = "1", TrainerName="Trainer1" },
            };

        public static PaginationViewModel GetPaginationViewModel(int requestedPage, int totalItems, string searchTerm)
            => new PaginationViewModel
            {
                RequestedPage = requestedPage,
                TotalItems = totalItems,
                SearchTerm = searchTerm,
            };

        public static void AssertCourseServiceModel(CourseServiceModel expectedCourse, CourseServiceModel actualCourse)
        {
            Assert.NotNull(actualCourse);

            Assert.Equal(expectedCourse.Name, actualCourse.Name);
            Assert.Equal(expectedCourse.StartDate, actualCourse.StartDate);
            Assert.Equal(expectedCourse.EndDate, actualCourse.EndDate);
            Assert.Equal(expectedCourse.TrainerId, actualCourse.TrainerId);
            Assert.Equal(expectedCourse.TrainerName, actualCourse.TrainerName);
        }

        public static void AssertCourseServiceModelCollection(IEnumerable<CourseServiceModel> courses)
        {
            var expectedCourses = GetCourseServiceModelCollection();

            Assert.NotNull(courses);
            Assert.IsAssignableFrom<IEnumerable<CourseServiceModel>>(courses);
            Assert.Equal(expectedCourses.Count(), courses.Count());

            foreach (var expectedCourse in expectedCourses)
            {
                var actualCourse = courses.FirstOrDefault(c => c.Id == expectedCourse.Id);
                AssertCourseServiceModel(expectedCourse, actualCourse);
            }
        }

        public static void AssertPagination(PaginationViewModel expectedPagination, PaginationViewModel pagination)
        {
            Assert.NotNull(pagination);
            Assert.IsType<PaginationViewModel>(pagination);

            Assert.Equal(WebConstants.Index, pagination.Action);
            Assert.Equal(expectedPagination.SearchTerm, pagination.SearchTerm);
            Assert.Equal(expectedPagination.TotalItems, pagination.TotalItems);
            Assert.Equal(expectedPagination.RequestedPage, pagination.RequestedPage);
            Assert.Equal(expectedPagination.TotalPages, pagination.TotalPages);
            Assert.Equal(expectedPagination.CurrentPage, pagination.CurrentPage);
            Assert.Equal(expectedPagination.PreviousPage, pagination.PreviousPage);
            Assert.Equal(expectedPagination.NextPage, pagination.NextPage);
        }

        public static void AssertSearchViewModel(string expectedSearchTerm, SearchViewModel search)
        {
            Assert.NotNull(search);
            Assert.IsType<SearchViewModel>(search);

            Assert.Equal(expectedSearchTerm, search.SearchTerm);
            Assert.Equal(FormActionEnum.Search, search.Action);
            Assert.Equal(WebConstants.SearchByCourseName, search.Placeholder);
        }
    }
}