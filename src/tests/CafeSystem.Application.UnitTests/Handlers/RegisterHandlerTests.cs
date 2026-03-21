using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class RegisterHandlerTests
    {
        [Fact]
        public async Task Should_Create_User_When_Request_Is_Valid()
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();

            userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            passwordHasherMock
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns(new PasswordHashResult
                {
                    Hash = "hashed",
                    Salt = "salt"
                });

            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = new CreateUserRequest
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "password",
                BirthDate = "1990-10-01"
            };

            // Act
            User user = await handler.HandleAsync(request);

            // Assert
            user.Email.Should().Be("test@example.com");
            user.FullName.Should().Be("Test User");
            user.PasswordHash.Should().Be("hashed");
            user.PasswordSalt.Should().Be("salt");
            user.BirthDate.Should().Be(new System.DateOnly(1990, 10, 1));
            userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Email_Already_Exists()
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();

            userRepositoryMock
                .Setup(x => x.GetByEmailAsync("duplicated@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = System.Guid.NewGuid(), Email = "duplicated@example.com" });

            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = new CreateUserRequest
            {
                FullName = "Test User",
                Email = "duplicated@example.com",
                Password = "password"
            };

            // Act
            System.Func<Task> act = async () => await handler.HandleAsync(request);

            // Assert
            await act.Should().ThrowAsync<System.InvalidOperationException>()
                .WithMessage("E-mail já cadastro");
        }

        [Theory]
        [InlineData(null, "Campo nome é obrigatório")]
        [InlineData("", "Campo nome é obrigatório")]
        [InlineData("    ", "Campo nome é obrigatório")]
        [InlineData("Abcd", "Nome deve conter 5 ou mais caracteres")]
        public async Task Should_Fail_When_FullName_Is_Invalid(string? fullName, string expectedMessage)
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = BuildValidRequest();
            request.FullName = fullName ?? string.Empty;

            // Act
            System.Func<Task> act = async () => await handler.HandleAsync(request);

            // Assert
            await act.Should().ThrowAsync<System.ArgumentException>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public async Task Should_Fail_When_FullName_Has_More_Than_250_Characters()
        {
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = BuildValidRequest();
            request.FullName = new string('a', 251);

            System.Func<Task> act = async () => await handler.HandleAsync(request);

            await act.Should().ThrowAsync<System.ArgumentException>()
                .WithMessage("Nome deve conter no máximo 250 caracteres");
        }

        [Theory]
        [InlineData(null, "Campo e-mail é obrigatório")]
        [InlineData("", "Campo e-mail é obrigatório")]
        [InlineData("   ", "Campo e-mail é obrigatório")]
        [InlineData("email-invalido", "Campo e-mail está em um formato inválido")]
        [InlineData("teste@", "Campo e-mail está em um formato inválido")]
        public async Task Should_Fail_When_Email_Is_Invalid(string? email, string expectedMessage)
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = BuildValidRequest();
            request.Email = email ?? string.Empty;

            // Act
            System.Func<Task> act = async () => await handler.HandleAsync(request);

            // Assert
            await act.Should().ThrowAsync<System.ArgumentException>()
                .WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData(null, "O campo senha é obrigatório")]
        [InlineData("", "O campo senha é obrigatório")]
        [InlineData("   ", "O campo senha é obrigatório")]
        [InlineData("1234", "Senha deve conter 5 ou mais caracteres")]
        [InlineData("123456789012345678901", "Senha deve conter no máximo 20 caracteres")]
        public async Task Should_Fail_When_Password_Is_Invalid(string? password, string expectedMessage)
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = BuildValidRequest();
            request.Password = password ?? string.Empty;

            // Act
            System.Func<Task> act = async () => await handler.HandleAsync(request);

            // Assert
            await act.Should().ThrowAsync<System.ArgumentException>()
                .WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("3000-01-01")]
        [InlineData("2023-02-29")]
        [InlineData("2024-13-10")]
        [InlineData("31/01/2020")]
        public async Task Should_Fail_When_BirthDate_Is_Invalid(string birthDate)
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = BuildValidRequest();
            request.BirthDate = birthDate;

            // Act
            System.Func<Task> act = async () => await handler.HandleAsync(request);

            // Assert
            await act.Should().ThrowAsync<System.ArgumentException>()
                .WithMessage("Data de nascimento inválida");
        }

        private static CreateUserRequest BuildValidRequest()
        {
            return new CreateUserRequest
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "password",
                BirthDate = "1990-01-01"
            };
        }
    }
}
