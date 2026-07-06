using System.ComponentModel.DataAnnotations;
using cimerko_app.Areas.Identity.Pages.Account;
using cimerko_app.Models;

namespace Tests;

public class RoommateProfileTests {
    [Theory]
    [InlineData("2000-07-04", "2026-07-05", 26)]
    [InlineData("2000-07-05", "2026-07-05", 26)]
    [InlineData("2000-07-06", "2026-07-05", 25)]
    public void CalculateAge_accounts_for_whether_the_birthday_has_passed(
        string dateOfBirthValue,
        string currentDateValue,
        int expectedAge) {
        var dateOfBirth = DateOnly.Parse(dateOfBirthValue);
        var currentDate = DateOnly.Parse(currentDateValue);

        var age = RoommateProfile.CalculateAge(dateOfBirth, currentDate);

        Assert.Equal(expectedAge, age);
    }

    [Fact]
    public void Registration_rejects_a_user_younger_than_18() {
        var input = ValidRegistrationInput();
        input.DateOfBirth = DateOnly.FromDateTime(DateTime.Today).AddYears(-18).AddDays(1);

        var validationResults = Validate(input);

        Assert.Contains(
            validationResults,
            result => result.MemberNames.Contains(
                nameof(LoginModel.RegistrationInputModel.DateOfBirth)));
    }

    [Fact]
    public void Registration_accepts_a_user_who_is_18_today() {
        var input = ValidRegistrationInput();
        input.DateOfBirth = DateOnly.FromDateTime(DateTime.Today).AddYears(-18);

        var validationResults = Validate(input);

        Assert.DoesNotContain(
            validationResults,
            result => result.MemberNames.Contains(
                nameof(LoginModel.RegistrationInputModel.DateOfBirth)));
    }

    private static LoginModel.RegistrationInputModel ValidRegistrationInput() {
        return new LoginModel.RegistrationInputModel {
            FirstName = "Test",
            LastName = "Resident",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today).AddYears(-20),
            City = "Berlin",
            AccountType = "Student",
            Email = "resident@example.test",
            Password = "password",
            ConfirmPassword = "password"
        };
    }

    private static List<ValidationResult> Validate(object model) {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
