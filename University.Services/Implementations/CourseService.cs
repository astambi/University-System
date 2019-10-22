namespace University.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services.Models.Courses;
    using University.Services.Models.ShoppingCart;

    public class CourseService : ICourseService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public CourseService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<CourseServiceModel>> AllActiveAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.GetAll(search, true, page, pageSize);

        public async Task<IEnumerable<CourseServiceModel>> AllArchivedAsync(
            string search = null,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
            => await this.GetAll(search, false, page, pageSize);

        public async Task<bool> CanEnrollAsync(int id)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }

            var hasStarted = course.StartDate.HasEnded();

            return !hasStarted;
        }

        public async Task<bool> CancellUserEnrollmentInCourseAsync(int courseId, string userId)
        {
            if (!await this.CanEnrollAsync(courseId)
                || !await this.db.Users.AnyAsync(u => u.Id == userId)
                || !await this.IsUserEnrolledInCourseAsync(courseId, userId))
            {
                return false;
            }

            //var studentCourse = await this.db
            //    .Courses
            //    .Where(c => c.Id == courseId)
            //    .SelectMany(c => c.Students)
            //    .Where(sc => sc.StudentId == userId)
            //    .FirstOrDefaultAsync();

            // NB! observe primary key order in StudentCourse table
            var studentCourse = await this.db.FindAsync<StudentCourse>(userId, courseId);

            this.db.Remove(studentCourse);
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> CancelUserEnrollmentInOrderCoursesAsync(int orderId, string userId)
        {
            var userExists = await this.db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return false;
            }

            var courseIds = this.GetCoursesIdsForUserOrder(orderId, userId);
            if (!courseIds.Any())
            {
                return false;
            }

            var studentCourses = await this.db
                .Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u
                    .Courses
                    .Where(sc => courseIds.Contains(sc.CourseId)) // enrolled courses
                    .Where(sc => DateTime.UtcNow < sc.Course.StartDate)) // courses have not started
                .ToListAsync();

            foreach (var studentCourse in studentCourses)
            {
                this.db.Remove(studentCourse);
            }

            var result = await this.db.SaveChangesAsync();

            return result == courseIds.Count();
        }

        public async Task<bool> EnrollUserInCourseAsync(int courseId, string userId)
        {
            if (!await this.CanEnrollAsync(courseId)
                || !await this.db.Users.AnyAsync(u => u.Id == userId)
                || await this.IsUserEnrolledInCourseAsync(courseId, userId))
            {
                return false;
            }

            await this.db.AddAsync(new StudentCourse { CourseId = courseId, StudentId = userId });
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> EnrollUserInOrderCoursesAsync(int orderId, string userId)
        {
            var userExists = await this.db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return false;
            }

            var courseIds = this.GetCoursesIdsForUserOrder(orderId, userId);
            if (!courseIds.Any())
            {
                return false;
            }

            var couseIdsToEnroll = this.GetCoursesToEnroll(courseIds)
                .Where(c => !c.Students.Any(sc => sc.StudentId == userId)) // user is not enrolled in course
                .Select(c => c.Id)
                .ToList();

            foreach (var courseId in couseIdsToEnroll)
            {
                await this.db.AddAsync(new StudentCourse { CourseId = courseId, StudentId = userId });
            }

            var result = await this.db.SaveChangesAsync();

            return result == courseIds.Count();
        }

        public bool Exists(int id)
            => this.db.Courses.Any(c => c.Id == id);

        public async Task<CourseServiceModel> GetByIdBasicAsync(int id)
            => await this.GetByIdAsync<CourseServiceModel>(id);

        public async Task<CourseDetailsServiceModel> GetByIdAsync(int id)
            => await this.GetByIdAsync<CourseDetailsServiceModel>(id);

        public async Task<IEnumerable<CartItemDetailsServiceModel>> GetCartItemsDetailsForUserAsync(
           IEnumerable<CartItem> cartItems,
           string userId = null)
        {
            var courseIds = cartItems.Select(i => i.CourseId);
            var coursesToEnroll = this.GetCoursesToEnroll(courseIds);

            if (userId != null)
            {
                coursesToEnroll = coursesToEnroll
                    .Where(c => !c.Students.Any(sc => sc.StudentId == userId)); // user not enrolled in course
            }

            return await coursesToEnroll
                .ProjectTo<CartItemDetailsServiceModel>(this.mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<bool> IsUserEnrolledInCourseAsync(int courseId, string userId)
            => await this.db
            .Courses
            .Where(c => c.Id == courseId)
            .Where(c => c.Students.Any(sc => sc.StudentId == userId))
            .AnyAsync();

        public async Task<int> TotalActiveAsync(string search = null)
            => await this.GetByStatus(this.GetBySearch(search), true)
            .CountAsync();

        public async Task<int> TotalArchivedAsync(string search = null)
            => await this.GetByStatus(this.GetBySearch(search), false)
            .CountAsync();

        private async Task<IEnumerable<CourseServiceModel>> GetAll(
            string search,
            bool isActive,
            int page = 1,
            int pageSize = ServicesConstants.PageSize)
        {
            // Negative page & page size
            page = Math.Max(1, page);

            if (pageSize < 1)
            {
                pageSize = ServicesConstants.PageSize;
            }

            var coursesBySearch = this.GetBySearch(search);
            var coursesByStatus = this.GetByStatus(coursesBySearch, isActive);

            return await coursesByStatus
                .OrderByDescending(c => c.StartDate)
                .ThenByDescending(c => c.EndDate)
                .ProjectTo<CourseServiceModel>(this.mapper.ConfigurationProvider)
                .GetPageItems(page, pageSize)
                .ToListAsync();
        }

        private async Task<TModel> GetByIdAsync<TModel>(int id)
            where TModel : class
            => await this.db
            .Courses
            .Where(c => c.Id == id)
            .ProjectTo<TModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        private IQueryable<Course> GetBySearch(string search)
           => string.IsNullOrWhiteSpace(search)
           ? this.db.Courses
           : this.db.Courses.Where(c => c.Name.ToLower().Contains(search.Trim().ToLower()));

        private IQueryable<Course> GetByStatus(IQueryable<Course> coursesAsQueryable, bool? isActive)
            => isActive == null // all
            ? coursesAsQueryable
            : (bool)isActive
                ? coursesAsQueryable.Where(c => DateTime.UtcNow <= c.EndDate).AsQueryable() // active
                : coursesAsQueryable.Where(c => c.EndDate < DateTime.UtcNow).AsQueryable(); // archive

        private IEnumerable<int> GetCoursesIdsForUserOrder(int orderId, string userId)
            => this.db
            .OrderItems
            .Where(oi => oi.OrderId == orderId)
            .Where(oi => oi.Order.UserId == userId)
            .Select(oi => oi.CourseId)
            .ToList();

        private IQueryable<Course> GetCoursesToEnroll(IEnumerable<int> courseIds)
            => this.db
            .Courses
            .Where(c => courseIds.Contains(c.Id))
            .Where(c => DateTime.UtcNow < c.StartDate);
    }
}
