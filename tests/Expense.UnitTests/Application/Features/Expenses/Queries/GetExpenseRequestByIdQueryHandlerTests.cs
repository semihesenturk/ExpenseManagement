using Expense.Application.Common.Interfaces;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequestById;
using Expense.Domain.Entities;
using Expense.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace Expense.UnitTests.Application.Features.Expenses.Queries;

public class GetExpenseRequestByIdQueryHandlerTests
{
    private readonly Mock<IExpenseDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetExpenseRequestByIdQueryHandler _handler;

    public GetExpenseRequestByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IExpenseDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetExpenseRequestByIdQueryHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object);
    }

    private static ExpenseRequest CreateExpense(Guid tenantId, decimal amount = 1500m)
        => new ExpenseRequest(tenantId, Guid.NewGuid(), amount,
            "Yirmi karakterden uzun açıklama",
            ExpenseCategory.Travel, Currency.TRY);

    [Fact]
    public async Task Handle_WhenTenantIdIsNull_ShouldReturnNull()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.TenantId).Returns((Guid?)null);
        var query = new GetExpenseRequestByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert — TenantId olmadan DB'ye hiç gidilmemeli
        result.Should().BeNull();
        _contextMock.Verify(x => x.ExpenseRequests, Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExpenseBelongsToSameTenant_ShouldReturnDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var expense = CreateExpense(tenantId, 1500m);
        var expenses = new List<ExpenseRequest> { expense };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetExpenseRequestByIdQuery { Id = expense.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expense.Id);
        result.Amount.Should().Be(1500m);
    }

    [Fact]
    public async Task Handle_WhenExpenseBelongsToDifferentTenant_ShouldReturnNull()
    {
        // Arrange — farklı tenant'a ait harcama, mevcut kullanıcı görememeli
        var currentTenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var expense = CreateExpense(differentTenantId, 5000m);
        var expenses = new List<ExpenseRequest> { expense };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(currentTenantId);

        var query = new GetExpenseRequestByIdQuery { Id = expense.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenExpenseDoesNotExist_ShouldReturnNull()
    {
        // Arrange — DB'de hiç kayıt yok
        var tenantId = Guid.NewGuid();

        _contextMock.Setup(x => x.ExpenseRequests)
            .ReturnsDbSet(new List<ExpenseRequest>());
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetExpenseRequestByIdQuery { Id = Guid.NewGuid() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}