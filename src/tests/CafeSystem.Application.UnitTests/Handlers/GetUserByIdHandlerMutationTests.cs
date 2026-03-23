using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;
using System.Globalization;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class GetUserByIdHandlerMutationTests
    {
        [Theory]
        [InlineData(true, null, true)]
        [InlineData(false, null, false)]
        [InlineData(true, "2024-01-01", false)]
        public async Task Should_Validate_User_Status_Before_Returning(bool isActive, string? deletedAtIso, bool shouldSucceed)
        {
            Guid userId = Guid.NewGuid();
            User user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Mutation User",
                BirthDate = new DateOnly(1990, 1, 1),
                IsActive = isActive,
                DeletedAt = deletedAtIso == null ? null : DateTime.Parse(deletedAtIso, CultureInfo.InvariantCulture)
            };

            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetByIdNoTrackingAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            GetUserByIdHandler handler = new GetUserByIdHandler(repositoryMock.Object);

            if (shouldSucceed)
            {
                var response = await handler.HandleAsync(userId);
                response.Code.Should().Be(userId);
                return;
            }

            Func<Task> act = async () => await handler.HandleAsync(userId);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }
    }
}
