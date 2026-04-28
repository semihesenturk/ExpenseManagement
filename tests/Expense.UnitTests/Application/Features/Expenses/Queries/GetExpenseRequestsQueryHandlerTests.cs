using Expense.Application.Common.Interfaces;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequests;
using Expense.Domain.Entities;
using Expense.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace Expense.UnitTests.Application.Features.Expenses.Queries;

public class GetExpenseRequestsQueryHandlerTests
{
    private readonly Mock<IExpenseDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetExpenseRequestsQueryHandler _handler;

    public GetExpenseRequestsQueryHandlerTests()
    {
        _contextMock = new Mock<IExpenseDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetExpenseRequestsQueryHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object);
    }

    private static ExpenseRequest CreateExpense(Guid tenantId, Guid userId, decimal amount = 1000m)
        => new ExpenseRequest(tenantId, userId, amount,
            "Yirmi karakterden uzun açıklama",
            ExpenseCategory.Travel, Currency.TRY);

    [Fact]
    public async Task Handle_WhenUserIsEmployee_ShouldReturnOnlyOwnExpenses()
    {
        // Arrange — Employee sadece kendi harcamasını görmeli
        var tenantId = Guid.NewGuid();
        var myUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var expenses = new List<ExpenseRequest>
        {
            CreateExpense(tenantId, myUserId, 1000m),
            CreateExpense(tenantId, otherUserId, 5000m)
        };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(myUserId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "Employee" });

        var query = new GetExpenseRequestsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenUserIsHR_ShouldReturnAllExpensesInTenant()
    {
        // Arrange — HR tüm tenant harcamalarını görmeli
        var tenantId = Guid.NewGuid();
        var myUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var expenses = new List<ExpenseRequest>
        {
            CreateExpense(tenantId, myUserId, 1000m),
            CreateExpense(tenantId, otherUserId, 5000m)
        };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(myUserId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });

        var query = new GetExpenseRequestsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange — 3 kayıt var, 2'si isteniyor
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var expenses = new List<ExpenseRequest>
        {
            CreateExpense(tenantId, userId, 100m),
            CreateExpense(tenantId, userId, 200m),
            CreateExpense(tenantId, userId, 300m)
        };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });

        var query = new GetExpenseRequestsQuery { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnOnlyMatchingExpenses()
    {
        // Arrange — sadece Pending olanlar filtrelenmeli
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var pending = CreateExpense(tenantId, userId, 1000m);
        var approved = CreateExpense(tenantId, userId, 2000m);
        approved.Approve(Guid.NewGuid());

        var expenses = new List<ExpenseRequest> { pending, approved };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });

        var query = new GetExpenseRequestsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Status = ExpenseStatus.Pending
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().Status.Should().Be("Pending");
    }
}