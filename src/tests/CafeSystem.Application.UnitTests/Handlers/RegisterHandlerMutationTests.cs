using System;
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
    public class RegisterHandlerMutationTests
    {
        [Theory]
        [InlineData(4, "Nome deve conter 5 ou mais caracteres")]
        [InlineData(5, null)]
        [InlineData(250, null)]
        [InlineData(251, "Nome deve conter no máximo 250 caracteres")]
        public async Task Should_Protect_Name_Length_Boundaries(int nameLength, string? expectedError)
        {
            Mock<IUserRepository> userRepositoryMock = BuildRepositoryMock();
            Mock<IPasswordHasher> passwordHasherMock = BuildPasswordHasherMock();
            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = BuildValidRequest();
            request.FullName = new string('a', nameLength);

            if (expectedError == null)
            {
                User user = await handler.HandleAsync(request);
                user.FullName.Should().HaveLength(nameLength);
                return;
            }

            Func<Task> act = async () => await handler.HandleAsync(request);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage(expectedError);
        }

        [Theory]
        [InlineData("1234", "Senha deve conter 5 ou mais caracteres")]
        [InlineData("12345", null)]
        [InlineData("123456789012345678901", "Senha deve conter no máximo 20 caracteres")]
        [InlineData("12345678901234567890", null)]
        public async Task Should_Protect_Password_Length_Boundaries(string password, string? expectedError)
        {
            Mock<IUserRepository> userRepositoryMock = BuildRepositoryMock();
            Mock<IPasswordHasher> passwordHasherMock = BuildPasswordHasherMock();
            RegisterHandler handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            CreateUserRequest request = BuildValidRequest();
            request.Password = password;

            if (expectedError == null)
            {
                User user = await handler.HandleAsync(request);
                user.PasswordHash.Should().NotBeNullOrWhiteSpace();
                return;
            }

            Func<Task> act = async () => await handler.HandleAsync(request);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage(expectedError);
        }

        private static Mock<IUserRepository> BuildRepositoryMock()
        {
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            userRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return userRepositoryMock;
        }

        private static Mock<IPasswordHasher> BuildPasswordHasherMock()
        {
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns(new PasswordHashResult
                {
                    Hash = "hashed-password",
                    Salt = "generated-salt"
                });

            return passwordHasherMock;
        }

        private static CreateUserRequest BuildValidRequest()
        {
            return new CreateUserRequest
            {
                FullName = "Valid Name",
                Email = $"{Guid.NewGuid()}@example.com",
                Password = "valid1",
                BirthDate = "1990-01-01"
            };
        }
    }
}
