using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;
using Expense.Domain.Entities;
using Expense.Domain.Enums;
using FluentAssertions;
using MassTransit;
using Moq;
using Shared.Contracts.Events;

namespace Expense.UnitTests.Application.Features.Expenses.Commands;

public class RejectExpenseRequestCommandHandlerTests
{
    private readonly Mock<IExpenseRequestRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly RejectExpenseRequestCommandHandler _handler;

    public RejectExpenseRequestCommandHandlerTests()
    {
        _repositoryMock = new Mock<IExpenseRequestRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _handler = new RejectExpenseRequestCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _publishEndpointMock.Object);
    }

    private static ExpenseRequest CreateExpense(Guid tenantId)
        => new ExpenseRequest(tenantId, Guid.NewGuid(), 1500m,
            "Yirmi karakterden uzun açıklama",
            ExpenseCategory.Supply, Currency.TRY);

    [Fact]
    public async Task Handle_WhenUserIsHR_ShouldRejectAndPublishEvent()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var expense = CreateExpense(tenantId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
        _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        await _handler.Handle(new RejectExpenseRequestCommand(expenseId, "Fatura eksik yüklenmiş."), CancellationToken.None);

        expense.Status.Should().Be(ExpenseStatus.Rejected);
        _repositoryMock.Verify(x => x.UpdateAsync(expense, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseRejectedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDifferentTenant_ShouldThrowUnauthorizedAccessException()
    {
        var myTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var expense = CreateExpense(otherTenantId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(myTenantId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
        _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        Func<Task> act = async () =>
            await _handler.Handle(new RejectExpenseRequestCommand(expenseId, "Red"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenUserHasNoManagerialRole_ShouldThrowUnauthorizedAccessException()
    {
        var tenantId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var expense = CreateExpense(tenantId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "Employee" });
        _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        Func<Task> act = async () =>
            await _handler.Handle(new RejectExpenseRequestCommand(expenseId, "Red"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenExpenseNotFound_ShouldThrowKeyNotFoundException()
    {
        var expenseId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
        _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExpenseRequest)null!);

        Func<Task> act = async () =>
            await _handler.Handle(new RejectExpenseRequestCommand(expenseId, "Red"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}