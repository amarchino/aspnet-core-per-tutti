﻿using MyCourse.Models.ValueTypes;
using MyCourse.Models.Enums;

namespace MyCourse.Models.Entities;

public partial class Course
{
    public Course(string title, string author, string authorId)
    {
        ChangeTitle(title);
        ChangeAuthor(author, authorId);
        ImagePath = "/Courses/default.png";
        Lessons = new HashSet<Lesson>();
        SubscribedUsers = new HashSet<ApplicationUser>();
        Status = CourseStatus.Draft;
    }

    public int Id { get; private set; }
    public string Title { get; private set; } = "";
    public string? Description { get; private set; }
    public string ImagePath { get; private set; }
    public string Author { get; private set; } = "";
    public string? Email { get; private set; }
    public double Rating { get; private set; }
    public Money? FullPrice { get; private set; }
    public Money? CurrentPrice { get; private set; }
    public string? RowVersion { get; private set; }
    public CourseStatus Status { get; private set; }
    public string AuthorId { get; private set; } = "";

    public virtual ICollection<Lesson> Lessons { get; private set; }
    public virtual ApplicationUser? AuthorUser { get; private set; }
    public virtual ICollection<ApplicationUser> SubscribedUsers { get; private set; }

    public void ChangeTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("The course must have a title");
        }
        Title = newTitle;
    }

    public void ChangeAuthor(string newAuthor, string newAuthorId)
    {
        if (string.IsNullOrWhiteSpace(newAuthor) || string.IsNullOrWhiteSpace(newAuthorId))
        {
            throw new ArgumentException("The course must have an author");
        }
        Author = newAuthor;
        AuthorId = newAuthorId;
    }

    public void ChangePrices(Money newFullPrice, Money newDiscountPrice)
    {
        if (newFullPrice == null || newDiscountPrice == null)
        {
            throw new ArgumentException("Prices can't be null");
        }
        if (newFullPrice.Currency != newDiscountPrice.Currency)
        {
            throw new ArgumentException("Currencies don't match");
        }
        if (newFullPrice.Amount < newDiscountPrice.Amount)
        {
            throw new ArgumentException("Full price can't be less than the current price");
        }
        FullPrice = newFullPrice;
        CurrentPrice = newDiscountPrice;
    }

    public void changeDescription(string newDescription)
    {
        if (string.IsNullOrEmpty(newDescription))
        {
            throw new ArgumentException("Desciption can't be empty");
        }
        Description = newDescription;
    }

    public void changeEmail(string newEmail)
    {
        if (string.IsNullOrEmpty(newEmail))
        {
            throw new ArgumentException("Email can't be empty");
        }
        Email = newEmail;
    }

    public void ChangeImagePath(string newImagePath)
    {
        if (string.IsNullOrEmpty(newImagePath))
        {
            throw new ArgumentException("Image path can't be empty");
        }
        ImagePath = newImagePath;
    }

    public void ChangeStatus(CourseStatus newStatus)
    {
        Status = newStatus;
    }
}
