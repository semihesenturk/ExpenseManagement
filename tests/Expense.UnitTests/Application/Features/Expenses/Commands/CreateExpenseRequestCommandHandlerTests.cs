using AutoMapper;
using Expense.Application.Common.Interfaces;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.Domain.Entities;
using Expense.Domain.Enums;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Contracts.Events;

namespace Expense.UnitTests.Application.Features.Expenses.Commands;

public class CreateExpenseRequestCommandHandlerTests
{
    private readonly Mock<IExpenseDbContext> _contextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateExpenseRequestCommandHandler _handler;

    public CreateExpenseRequestCommandHandlerTests()
    {
        _contextMock = new Mock<IExpenseDbContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _mapperMock = new Mock<IMapper>();

        var dbSetMock = new Mock<DbSet<ExpenseRequest>>();
        _contextMock.Setup(x => x.ExpenseRequests).Returns(dbSetMock.Object);

        _handler = new CreateExpenseRequestCommandHandler(
            _contextMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _publishEndpointMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateExpenseAndPublishEvent()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);

        var expectedDto = new CreateExpenseRequestDto { Amount = 3000, Currency = "TRY", Category = "Travel" };
        _mapperMock.Setup(x => x.Map<CreateExpenseRequestDto>(It.IsAny<ExpenseRequest>()))
            .Returns(expectedDto);

        var command = new CreateExpenseRequestCommand
        {
            Amount = 3000,
            Description = "Yirmi karakterden uzun açıklama",
            Category = ExpenseCategory.Travel,
            Currency = Currency.TRY
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Amount.Should().Be(3000);
        _contextMock.Verify(x => x.ExpenseRequests.Add(It.IsAny<ExpenseRequest>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserOrTenantIsMissing_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns((Guid?)null);

        var command = new CreateExpenseRequestCommand
        {
            Amount = 1000,
            Description = "Hayalet harcama açıklaması buraya",
            Category = ExpenseCategory.Other,
            Currency = Currency.TRY
        };

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Kullanıcı veya Tenant bilgisi bulunamadı.");

        _contextMock.Verify(x => x.ExpenseRequests.Add(It.IsAny<ExpenseRequest>()), Times.Never);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}