using FluentValidation;
using IdentityService.Application.Commands;

namespace IdentityService.Application.Validators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống")
            .MinimumLength(3).WithMessage("Tên đăng nhập phải có ít nhất 3 ký tự")
            .MaximumLength(50).WithMessage("Tên đăng nhập không được quá 50 ký tự")
            .Matches("^[a-zA-Z0-9._@+-]+$").WithMessage("Tên đăng nhập chỉ được chứa chữ cái, số và các ký tự đặc biệt: ._@+-");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không đúng định dạng");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống")
            .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự")
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ thường")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 chữ số")
            .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải có ít nhất 1 ký tự đặc biệt");

        RuleFor(x => x.PhoneNumber)
            .Matches("^[0-9]*$").WithMessage("Số điện thoại chỉ được chứa chữ số")
            .Length(10).WithMessage("Số điện thoại phải có 10 chữ số")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
