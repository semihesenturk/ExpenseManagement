using Expense.Application.Common.Interfaces;
using Expense.Application.Contracts.Persistence;
using Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;
using Expense.Domain.Entities;
using Expense.Domain.Enums;
using FluentAssertions;
using MassTransit;
using Moq;
using Shared.Contracts.Events;

namespace Expense.UnitTests.Application.Features.Expenses.Commands;

public class ApproveExpenseRequestCommandHandlerTests
{
    private readonly Mock<IExpenseRequestRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ApproveExpenseRequestCommandHandler _handler;

    public ApproveExpenseRequestCommandHandlerTests()
    {
        _repositoryMock = new Mock<IExpenseRequestRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _handler = new ApproveExpenseRequestCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _publishEndpointMock.Object);
    }

    private static ExpenseRequest CreateExpense(Guid tenantId, decimal amount = 2000m)
        => new ExpenseRequest(tenantId, Guid.NewGuid(), amount,
            "Yirmi karakterden uzun açıklama",
            ExpenseCategory.Travel, Currency.TRY);

    [Fact]
    public async Task Handle_WhenAmountUnder5000_AndUserIsHR_ShouldApproveImmediately()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var expense = CreateExpense(tenantId, 2000m);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
        _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        await _handler.Handle(new ApproveExpenseRequestCommand(expenseId, "Onay"), CancellationToken.None);

        expense.Status.Should().Be(ExpenseStatus.Approved);
        _repositoryMock.Verify(x => x.UpdateAsync(expense, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAmountOver5000_AndUserIsHR_ShouldSetPendingAdminApproval()
    {
        var tenantId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var expense = CreateExpense(tenantId, 7500m);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
        _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        await _handler.Handle(new ApproveExpenseRequestCommand(expenseId, "HR onayı"), CancellationToken.None);

        expense.Status.Should().Be(ExpenseStatus.PendingAdminApproval);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDifferentTenant_ShouldThrowUnauthorizedAccessException()
    {
        var myTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var expense = CreateExpense(otherTenantId, 1000m);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(myTenantId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
        _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        Func<Task> act = async () =>
            await _handler.Handle(new ApproveExpenseRequestCommand(expenseId, ""), CancellationToken.None);

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
            await _handler.Handle(new ApproveExpenseRequestCommand(expenseId, ""), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}