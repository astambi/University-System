﻿namespace LearningSystem.Tests
{
    using System;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using Microsoft.EntityFrameworkCore;

    public class Tests
    {
        static Tests()
        {
            InitializeMapper();
        }

        public static IMapper Mapper { get; private set; }

        public static LearningSystemDbContext InitializeDatabase()
        {
            var dbOptions = new DbContextOptionsBuilder<LearningSystemDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LearningSystemDbContext(dbOptions);
        }

        private static void InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                //cfg.AddProfile<AutoMapperProfile>();
                cfg.CreateMap<Course, CourseServiceModel>();
                cfg.CreateMap<Course, CourseWithDescriptionServiceModel>();
                cfg.CreateMap<User, UserBasicServiceModel>();
                cfg.CreateMap<User, UserServiceModel>();
            });

            Mapper = config.CreateMapper();
        }
    }
}