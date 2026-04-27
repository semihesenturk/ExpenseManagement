using Expense.Application.Common.Interfaces;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequests;
using Expense.Domain.Entities;
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
        _handler = new GetExpenseRequestsQueryHandler(_contextMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsEmployee_ShouldReturnOnlyOwnExpenses()
    {
        // Arrange: Kullanıcı düz çalışan (Employee). Sadece kendi 1 harcamasını görmeli.
        var tenantId = Guid.NewGuid();
        var myUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var expenses = new List<ExpenseRequest>
        {
            new ExpenseRequest(tenantId, myUserId, 1000, "Benim Harcamam"),
            new ExpenseRequest(tenantId, otherUserId, 5000, "Başkasının Harcaması")
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
        result.TotalCount.Should().Be(1); // Sadece kendi harcamasını saymalı
        result.Items.First().Description.Should().Be("Benim Harcamam");
    }

    [Fact]
    public async Task Handle_WhenUserIsApprover_ShouldReturnAllExpensesInTenant()
    {
        // Arrange: Kullanıcı Approver. Kendi tenant'ındaki herkesin harcamasını (2 adet) görmeli.
        var tenantId = Guid.NewGuid();
        var myUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var expenses = new List<ExpenseRequest>
        {
            new ExpenseRequest(tenantId, myUserId, 1000, "Benim Harcamam"),
            new ExpenseRequest(tenantId, otherUserId, 5000, "Başkasının Harcaması")
        };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(myUserId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "Approver" }); // Yetkili Rol

        var query = new GetExpenseRequestsQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2); // Yetkili olduğu için her ikisini de görmeli
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPageSize()
    {
        // Arrange: Sayfalama (Pagination) test ediliyor. Toplam 3 kayıt var ama 2'si isteniyor.
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var expenses = new List<ExpenseRequest>
        {
            new ExpenseRequest(tenantId, userId, 100, "Harcama 1"),
            new ExpenseRequest(tenantId, userId, 200, "Harcama 2"),
            new ExpenseRequest(tenantId, userId, 300, "Harcama 3")
        };

        _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
        
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });

        var query = new GetExpenseRequestsQuery { PageNumber = 1, PageSize = 2 }; // Sadece ilk 2'sini getir

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3); // Toplam veri sayısı 3
        result.Items.Count.Should().Be(2); // Ama dönen liste (sayfa) 2 elemanlı olmalı
    }
}