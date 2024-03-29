using System.Data;
using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using MyCourse.Controllers;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using MyCourse.Models.ValueTypes;
using MyCourse.Models.ViewModels;
using MyCourse.Models.ViewModels.Courses;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Models.Services.Application.Courses;
public class AdoNetCourseService : ICourseService
{
    private readonly IDatabaseAccessor db;
    private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
    private readonly ILogger<AdoNetCourseService> logger;
    private readonly IImagePersister imagePersister;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IEmailClient emailClient;
    private readonly IPaymentGateway paymentGateway;
    private readonly LinkGenerator linkGenerator;
    private readonly ITransactionLogger transactionLogger;

    public AdoNetCourseService(IDatabaseAccessor db, IOptionsMonitor<CoursesOptions> coursesOptions, ILogger<AdoNetCourseService> logger, IImagePersister imagePersister, IHttpContextAccessor httpContextAccessor,
        IEmailClient emailClient, IPaymentGateway paymentGateway, LinkGenerator linkGenerator, ITransactionLogger transactionLogger)
    {
        this.emailClient = emailClient;
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
        this.coursesOptions = coursesOptions;
        this.db = db;
        this.imagePersister = imagePersister;
        this.paymentGateway = paymentGateway;
        this.linkGenerator = linkGenerator;
        this.transactionLogger = transactionLogger;
    }
    public async Task<CourseDetailViewModel> GetCourseAsync(int id)
    {
        logger.LogInformation("Course {id} requested", id);
        FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency FROM Courses WHERE Id={id};
                SELECT Id, Title, Description, Duration FROM Lessons WHERE CourseId={id} AND Status<>'{nameof(CourseStatus.Deleted)}'";
        DataSet dataSet = await db.QueryAsync(query);
        // Course
        var courseTable = dataSet.Tables[0];
        if (courseTable.Rows.Count != 1)
        {
            logger.LogWarning("Course {id} not found", id);
            throw new CourseNotFoundException(id);
        }
        var courseDetailViewModel = CourseDetailViewModel.FromDataRow(courseTable.Rows[0]);

        // Course lessons
        var lessonDataTable = dataSet.Tables[1];
        foreach (DataRow lessonRow in lessonDataTable.Rows)
        {
            courseDetailViewModel.Lessons.Add(LessonViewModel.FromDataRow(lessonRow));
        }
        return courseDetailViewModel;
    }
    public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
    {
        var orderBy = model.OrderBy;
        if (orderBy == "CurrentPrice")
        {
            orderBy = "CurrentPrice_Amount";
        }
        string direction = model.Ascending ? "ASC" : "DESC";

        FormattableString query = $@"SELECT Id, Title, ImagePath, Author, Rating, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency
                FROM Courses
                WHERE UPPER(Title) LIKE UPPER({"%" + model.Search + "%"})
                AND Status<>'{nameof(CourseStatus.Deleted)}'
                ORDER BY {(Sql)orderBy} {(Sql)direction}
                LIMIT {model.Limit}
                OFFSET {model.Offset};

                SELECT COUNT(*)
                FROM Courses
                WHERE UPPER(Title) LIKE UPPER({"%" + model.Search + "%"})
                AND Status<>'{nameof(CourseStatus.Deleted)}'";
        DataSet dataSet = await db.QueryAsync(query);
        var dataTable = dataSet.Tables[0];
        List<CourseViewModel> courseList = new();
        foreach (DataRow courseRow in dataTable.Rows)
        {
            var course = CourseViewModel.FromDataRow(courseRow);
            courseList.Add(course);
        }
        ListViewModel<CourseViewModel> result = new()
        {
            Results = courseList,
            TotalCount = Convert.ToInt32(dataSet.Tables[1].Rows[0][0])
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
        string title = inputModel.Title;
        string author = "Mario Rossi";
        FormattableString query = $@"INSERT INTO Courses(Title, Author, ImagePath, CurrentPrice_Currency, CurrentPrice_Amount, FullPrice_Currency, FullPrice_Amount) VALUES ({title}, {author}, '/Courses/default.png', 'EUR', 0, 'EUR', 0);
                SELECT last_insert_rowid();";
        try
        {
            int courseId = await db.QueryScalarAsync<int>(query);
            CourseDetailViewModel course = await GetCourseAsync(courseId);
            return course;
        }
        catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
        {
            throw new CourseTitleUnavailableException(title, exc);
        }
    }

    public async Task<bool> IsTitleAvailableAsync(string title, int id)
    {
        bool titleExists = await db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Courses WHERE Title LIKE {title} AND Id<>{id} AND Status<>'{nameof(CourseStatus.Deleted)}'");
        return !titleExists;
    }

    public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
    {
        FormattableString query = $@"SELECT Id, Title, Description, ImagePath, Email, FullPrice_Amount, FullPrice_Currency, CurrentPrice_Amount, CurrentPrice_Currency, RowVersion FROM Courses WHERE Id = {id} AND Status<>'{nameof(CourseStatus.Deleted)}'";
        DataSet dataSet = await db.QueryAsync(query);
        var courseTable = dataSet.Tables[0];
        if (courseTable.Rows.Count != 1)
        {
            logger.LogWarning("Course {id} not found", id);
            throw new CourseNotFoundException(id);
        }
        var courseRow = courseTable.Rows[0];
        return CourseEditInputModel.FromDataRow(courseRow);
    }

    public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
    {
        try
        {
            string? imagePath = null;
            if (inputModel.Image != null)
            {
                imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
            }
            var affectedRows = await db.CommandAsync($@"UPDATE Courses SET Title={inputModel.Title}, Description={inputModel.Description}, Email={inputModel.Email}, ImagePath=COALESCE({imagePath}, ImagePath), FullPrice_Amount={inputModel.FullPrice.Amount}, FullPrice_Currency={inputModel.FullPrice.Currency}, CurrentPrice_Amount={inputModel.CurrentPrice.Amount}, CurrentPrice_Currency={inputModel.CurrentPrice.Currency} WHERE Id={inputModel.Id} AND RowVersion={inputModel.RowVersion} AND Status<>'{nameof(CourseStatus.Deleted)}'");
            if (affectedRows == 0)
            {
                bool courseExists = await db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Courses WHERE Id={inputModel.Id}");
                if (courseExists)
                {
                    throw new OptimisticConcurrencyException();
                }
                throw new CourseNotFoundException(inputModel.Id);
            }
        }
        catch (ConstraintViolationException exc)
        {
            throw new CourseTitleUnavailableException(inputModel.Title, exc);
        }
        catch (ImagePersistenceException exc)
        {
            throw new CourseImageInvalidException(inputModel.Id, exc);
        }

        CourseDetailViewModel course = await GetCourseAsync(inputModel.Id);
        return course;
    }

    public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
    {
        var affectedRows = await db.CommandAsync($@"UPDATE Courses SET Status='{nameof(CourseStatus.Deleted)}' WHERE Id={inputModel.Id} AND Status<>'{nameof(CourseStatus.Deleted)}'");
        if (affectedRows == 0)
        {
            throw new CourseNotFoundException(inputModel.Id);
        }
    }

    public async Task SendQuestionToCourseAuthorAsync(int id, string question)
    {
        FormattableString query = $@"SELECT Title, Email FROM Courses WHERE Course.Id={id}";
        DataSet dataSet = await db.QueryAsync(query);

        if (dataSet.Tables[0].Rows.Count == 0)
        {
            logger.LogWarning("Course {id} not found", id);
            throw new CourseNotFoundException(id);
        }

        string courseTitle = Convert.ToString(dataSet.Tables[0].Rows[0]["Title"])!;
        string courseEmail = Convert.ToString(dataSet.Tables[0].Rows[0]["Email"])!;

        string userFullName;
        string userEmail;

        try
        {
            userFullName = httpContextAccessor.HttpContext!.User.FindFirst("FullName")!.Value;
            userEmail = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Email)!.Value;
        }
        catch (NullReferenceException)
        {
            throw new UserUnknownException();
        }

        string subject = $@"Domanda per il tuo corso ""{courseTitle}""";
        string message = $@"<p>L'utente {userFullName} (<a href=""{userEmail}"">{userEmail}</a>) ti ha inviato la seguente domanda:</p><p>{question}</p>";

        try
        {
            await emailClient.SendEmailAsync(courseEmail, userEmail, subject, message);
        }
        catch
        {
            throw new SendException();
        }
    }

    public Task<string> GetCourseAuthorIdAsync(int courseId)
    {
        return db.QueryScalarAsync<string>($"SELECT AuthorId FROM Courses WHERE Id={courseId}");
    }

    public Task<int> GetCourseCountByAuthorIdAsync(string userId)
    {
        return db.QueryScalarAsync<int>($"SELECT COUNT(*) FROM Courses WHERE AuthorId={userId}");
    }

    public async Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel)
    {
        try
        {
            await db.CommandAsync($"INSERT INTO Subscriptions (UserId, CourseId, PaymentDate, PaymentType, Paid_Currency, Paid_Amount, TransactionId) VALUES ({inputModel.UserId}, {inputModel.CourseId}, {inputModel.PaymentDate}, {inputModel.PaymentType}, {inputModel.Paid.Currency}, {inputModel.Paid.Amount}, {inputModel.TransactionId})");
        }
        catch (ConstraintViolationException)
        {
            throw new CourseSubscriptionException(inputModel.CourseId);
        }
        catch (Exception)
        {
            await transactionLogger.LogTransactionAsync(inputModel);
        }
    }

    public Task<bool> IsCourseSubscribedAsync(int courseId, string userId)
    {
        return db.QueryScalarAsync<bool>($"SELECT COUNT(*) FROM Subscriptions WHERE CourseId={courseId} AND UserId={userId}");
    }

    public async Task<string> GetPaymentUrlAsync(int courseId)
    {
        CourseDetailViewModel viewModel = await GetCourseAsync(courseId);
        CoursePayInputModel inputModel = new()
        {
            CourseId = courseId,
            UserId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            Title = viewModel.Title,
            Description = viewModel.Description,
            Price = viewModel.CurrentPrice,
            ReturnUrl = linkGenerator.GetUriByAction(
                httpContextAccessor.HttpContext!,
                action: nameof(CoursesController.Subscribe),
                controller: "Courses",
                values: new { id = courseId }
            )!,
            CancelUrl = linkGenerator.GetUriByAction(
                httpContextAccessor.HttpContext!,
                action: nameof(CoursesController.Detail),
                controller: "Courses",
                values: new { id = courseId }
            )!
        };
        return await paymentGateway.GetPaymentUrlAsync(inputModel);
    }

    public Task<CourseSubscribeInputModel> CapturePaymentAsync(int id, string token)
    {
        return paymentGateway.CapturePaymentAsync(token);
    }

    public async Task<int?> GetCourseVoteAsync(int courseId)
    {
        string userId = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        string vote = await db.QueryScalarAsync<string>($"SELECT Vote FROM Subscriptions WHERE CourseId={courseId} AND UserId={userId}");
        return string.IsNullOrEmpty(vote) ? null : Convert.ToInt32(vote);
    }

    public async Task VoteCourseAsync(CourseVoteInputModel inputModel)
    {
        if (inputModel.Vote < 1 || inputModel.Vote > 5)
        {
            throw new InvalidVoteException(inputModel.Vote);
        }

        string userId = httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        int updatedRows = await db.CommandAsync($"UPDATE Subscriptions SET Vote={inputModel.Vote} WHERE CourseId={inputModel.Id} AND UserId={userId}");
        if (updatedRows == 0)
        {
            throw new CourseSubscriptionNotFoundException(inputModel.Id);
        }
    }
}
