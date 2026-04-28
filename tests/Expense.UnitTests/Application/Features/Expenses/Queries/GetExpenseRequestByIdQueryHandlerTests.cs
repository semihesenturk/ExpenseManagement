using AutoMapper;
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
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetExpenseRequestByIdQueryHandler _handler;

    public GetExpenseRequestByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IExpenseDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetExpenseRequestByIdQueryHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object);
    }

    private static ExpenseRequest CreateExpense(Guid tenantId, decimal amount = 1500m)
        => new ExpenseRequest(tenantId, Guid.NewGuid(), amount,
            "Yirmi karakterden uzun açıklama",
            ExpenseCategory.Travel, Currency.TRY);

    [Fact]
    public async Task Handle_WhenTenantIdIsNull_ShouldReturnNull()
    {
        _currentUserServiceMock.Setup(x => x.TenantId).Returns((Guid?)null);
        var query = new GetExpenseRequestByIdQuery { Id = Guid.NewGuid() };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
        _contextMock.Verify(x => x.ExpenseRequests, Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExpenseBelongsToSameTenant_ShouldReturnDto()
    {
        var tenantId = Guid.NewGuid();
        var expense = CreateExpense(tenantId, 1500m);
        var expenses = new List<ExpenseRequest> { expense };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);

        var expectedDto = new GetExpenseRequestByIdDto { Id = expense.Id, Amount = 1500m };
        _mapperMock.Setup(x => x.Map<GetExpenseRequestByIdDto>(expense)).Returns(expectedDto);

        var query = new GetExpenseRequestByIdQuery { Id = expense.Id };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(expense.Id);
        result.Amount.Should().Be(1500m);
    }

    [Fact]
    public async Task Handle_WhenExpenseBelongsToDifferentTenant_ShouldReturnNull()
    {
        var currentTenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var expense = CreateExpense(differentTenantId, 5000m);
        var expenses = new List<ExpenseRequest> { expense };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(currentTenantId);

        var query = new GetExpenseRequestByIdQuery { Id = expense.Id };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenExpenseDoesNotExist_ShouldReturnNull()
    {
        var tenantId = Guid.NewGuid();

        _contextMock.Setup(x => x.ExpenseRequests)
            .ReturnsDbSet(new List<ExpenseRequest>());
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);

        var query = new GetExpenseRequestByIdQuery { Id = Guid.NewGuid() };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}