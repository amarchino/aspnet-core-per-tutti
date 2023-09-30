using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Controllers;
using MyCourse.Models.Entities;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Courses
{
    public class EfCoreCourseService : ICourseService
    {
        private readonly MyCourseDbContext dbContext;
        private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
        private readonly IImagePersister imagePersister;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IEmailClient emailClient;
        private readonly IPaymentGateway paymentGateway;
        private readonly LinkGenerator linkGenerator;
        private readonly ITransactionLogger transactionLogger;

        public EfCoreCourseService(
            MyCourseDbContext dbContext,
            IOptionsMonitor<CoursesOptions> coursesOptions,
            IImagePersister imagePersister,
            IHttpContextAccessor httpContextAccessor,
            IEmailClient emailClient,
            IPaymentGateway paymentGateway,
            LinkGenerator linkGenerator,
            ITransactionLogger transactionLogger)
        {
            this.coursesOptions = coursesOptions;
            this.dbContext = dbContext;
            this.imagePersister = imagePersister;
            this.httpContextAccessor = httpContextAccessor;
            this.emailClient = emailClient;
            this.paymentGateway = paymentGateway;
            this.linkGenerator = linkGenerator;
            this.transactionLogger = transactionLogger;
        }

        public async Task<CourseDetailViewModel> GetCourseAsync(int id)
        {
            CourseDetailViewModel viewModel = await dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Id == id)
                .Select(course => new CourseDetailViewModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Author = course.Author,
                    Rating = course.Rating,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice,
                    Description = course.Description,
                    Lessons = course.Lessons.Select(lesson => LessonViewModel.FromEntity(lesson)).ToList()
                }).SingleAsync();

            return viewModel;
        }

        public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
        {
            IQueryable<Course> baseQuery = dbContext.Courses;
            switch (model.OrderBy)
            {
                case "Id":
                    baseQuery = model.Ascending ? baseQuery.OrderBy(course => course.Id) : baseQuery.OrderByDescending(course => course.Id);
                    break;
                case "Title":
                    baseQuery = model.Ascending ? baseQuery.OrderBy(course => course.Title) : baseQuery.OrderByDescending(course => course.Title);
                    break;
                case "Rating":
                    baseQuery = model.Ascending ? baseQuery.OrderBy(course => course.Rating) : baseQuery.OrderByDescending(course => course.Rating);
                    break;
                case "CurrentPrice":
                    baseQuery = model.Ascending ? baseQuery.OrderBy(course => course.CurrentPrice.Amount) : baseQuery.OrderByDescending(course => course.CurrentPrice.Amount);
                    break;
            }

            IQueryable<CourseViewModel> queryLinq = baseQuery
                .AsNoTracking()
                .Where(course => course.Title.ToUpper().Contains(model.Search.ToUpper()))
                .Select(course => new CourseViewModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    ImagePath = course.ImagePath,
                    Author = course.Author,
                    Rating = course.Rating,
                    CurrentPrice = course.CurrentPrice,
                    FullPrice = course.FullPrice
                })
                ;
            List<CourseViewModel> courses = await queryLinq
                .Skip(model.Offset)
                .Take(model.Limit)
                .ToListAsync();
            int TotalCount = await queryLinq.CountAsync();
            ListViewModel<CourseViewModel> result = new ListViewModel<CourseViewModel>
            {
                Results = courses,
                TotalCount = TotalCount
            };
            return result;
        }

        public async Task<List<CourseViewModel>> getBestRatingCoursesAsync()
        {
            return (await GetCoursesAsync(new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Rating",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order
            ))).Results;
        }

        public async Task<List<CourseViewModel>> getMostRecentCoursesAsync()
        {
            return (await GetCoursesAsync(new CourseListInputModel(
                search: "",
                page: 1,
                orderby: "Id",
                ascending: false,
                limit: coursesOptions.CurrentValue.InHome,
                orderOptions: coursesOptions.CurrentValue.Order
            ))).Results;
        }

        public async Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel)
        {
            string author;
            string authorId;
            try
            {
                author = httpContextAccessor.HttpContext.User.FindFirst("FullName").Value;
                authorId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            catch(NullReferenceException)
            {
                throw new UserUnknownException();
            }
            var entity = new Course(inputModel.Title, author, authorId);
            dbContext.Courses.Add(entity);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }

            return CourseDetailViewModel.FromEntity(entity);
        }

        public async Task<bool> IsTitleAvailableAsync(string title, int id)
        {
            bool titleExists = await dbContext.Courses
                .Where(course => course.Id != id)
                .AnyAsync(course => EF.Functions.Like(course.Title, title));
            return !titleExists;
        }

        public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
        {
            CourseEditInputModel viewModel = await dbContext.Courses
                .AsNoTracking()
                .Where(course => course.Id == id)
                .Select(course => new CourseEditInputModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    ImagePath = course.ImagePath,
                    Email = course.Email,
                    FullPrice = course.FullPrice,
                    CurrentPrice = course.CurrentPrice,
                    RowVersion = course.RowVersion
                }).SingleAsync();

            return viewModel;
        }

        public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);
            course.ChangeTitle(inputModel.Title);
            course.ChangePrices(inputModel.FullPrice, inputModel.CurrentPrice);
            course.changeDescription(inputModel.Description);
            course.changeEmail(inputModel.Email);
            // dbContext.Courses.Update(entity);

            dbContext.Entry(course).Property(course => course.RowVersion).OriginalValue = inputModel.RowVersion;

            try
            {
                if (inputModel.Image != null)
                {
                    string imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
                    course.ChangeImagePath(imagePath);
                }
            }
            catch (Exception exc)
            {
                throw new CourseImageInvalidException(inputModel.Id, exc);
            }

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
            {
                throw new CourseTitleUnavailableException(inputModel.Title, exc);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new OptimisticConcurrencyException();
            }

            return CourseDetailViewModel.FromEntity(course);
        }

        public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
        {
            Course course = await dbContext.Courses.FindAsync(inputModel.Id);
            if(course == null)
            {
                throw new CourseNotFoundException(inputModel.Id);
            }
            course.ChangeStatus(Enums.CourseStatus.Deleted);
            await dbContext.SaveChangesAsync();
        }

        public async Task SendQuestionToCourseAuthorAsync(int id, string question)
        {
            Course course = await dbContext.Courses.FindAsync(id);
            if(course == null)
            {
                throw new CourseNotFoundException(id);
            }

            string userFullName;
            string userEmail;

            try
            {
                userFullName = httpContextAccessor.HttpContext.User.FindFirst("FullName").Value;
                userEmail = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            }
            catch(NullReferenceException)
            {
                throw new UserUnknownException();
            }

            // Sanitizzo la domanda dell'utente
            question = new HtmlSanitizer(new HtmlSanitizerOptions { AllowedTags = new HashSet<string>() }).Sanitize(question);

            string subject = $@"Domanda per il tuo corso ""{course.Title}""";
            string message = $@"<p>L'utente {userFullName} (<a href=""{userEmail}"">{userEmail}</a>) ti ha inviato la seguente domanda:</p><p>{question}</p>";
            try
            {
                await emailClient.SendEmailAsync(course.Email, userEmail, subject, message);
            }
            catch
            {
                throw new SendException();
            }
        }

        public Task<string> GetCourseAuthorIdAsync(int courseId)
        {
            return dbContext.Courses
                .Where(course => course.Id == courseId)
                .Select(course => course.AuthorId)
                .FirstOrDefaultAsync();
        }

        public Task<int> GetCourseCountByAuthorIdAsync(string userId)
        {
            return dbContext.Courses
                .Where(course => course.AuthorId == userId)
                .CountAsync();
        }

        public async Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel)
        {
            Subscription subscription = new Subscription(inputModel.UserId, inputModel.CourseId)
            {
                PaymentDate = inputModel.PaymentDate,
                PaymentType = inputModel.PaymentType,
                Paid = inputModel.Paid,
                TransactionId = inputModel.TransactionId
            };
            dbContext.Subscriptions.Add(subscription);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new CourseSubscriptionException(inputModel.CourseId);
            }
            catch(Exception)
            {
                await transactionLogger.LogTransactionAsync(inputModel);
            }
        }

        public Task<bool> IsCourseSubscribedAsync(int courseId, string userId)
        {
            return dbContext.Subscriptions
                .Where(subscription => subscription.CourseId == courseId && subscription.UserId == userId)
                .AnyAsync();
        }

        public async Task<string> GetPaymentUrlAsync(int courseId)
        {
            CourseDetailViewModel viewModel = await GetCourseAsync(courseId);
            CoursePayInputModel inputModel = new()
            {
                CourseId = courseId,
                UserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                Title = viewModel.Title,
                Description = viewModel.Description,
                Price = viewModel.CurrentPrice,
                ReturnUrl = linkGenerator.GetUriByAction(
                    httpContextAccessor.HttpContext,
                    action: nameof(CoursesController.Subscribe),
                    controller: "Courses",
                    values: new { id = courseId }
                ),
                CancelUrl = linkGenerator.GetUriByAction(
                    httpContextAccessor.HttpContext,
                    action: nameof(CoursesController.Detail),
                    controller: "Courses",
                    values: new { id = courseId }
                )
            };
            return await paymentGateway.GetPaymentUrlAsync(inputModel);
        }

        public Task<CourseSubscribeInputModel> CapturePaymentAsync(int id, string token)
        {
            return paymentGateway.CapturePaymentAsync(token);
        }

        public async Task<int?> GetCourseVoteAsync(int courseId)
        {
            string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Subscription subscription = await dbContext.Subscriptions.SingleOrDefaultAsync(subscription => subscription.CourseId == courseId && subscription.UserId == userId);
            if(subscription == null)
            {
                throw new CourseSubscriptionNotFoundException(courseId);
            }
            return subscription.Vote;
        }

        public async Task VoteCourseAsync(CourseVoteInputModel inputModel)
        {
            if(inputModel.Vote < 1 || inputModel.Vote > 5)
            {
                throw new InvalidVoteException(inputModel.Vote);
            }
            string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Subscription subscription = await dbContext.Subscriptions.SingleOrDefaultAsync(subscription => subscription.CourseId == inputModel.Id && subscription.UserId == userId);
            if(subscription == null)
            {
                throw new CourseSubscriptionNotFoundException(inputModel.Id);
            }
            subscription.Vote = inputModel.Vote;
            await dbContext.SaveChangesAsync();
        }
    }
}
