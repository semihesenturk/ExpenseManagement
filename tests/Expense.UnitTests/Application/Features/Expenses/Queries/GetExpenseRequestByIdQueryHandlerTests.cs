// using Expense.Application.Common.Interfaces;
// using Expense.Application.Features.Expenses.Queries.GetExpenseRequestById;
// using Expense.Domain.Entities;
// using FluentAssertions;
// using Moq;
// using Moq.EntityFrameworkCore;
//
// namespace Expense.UnitTests.Application.Features.Expenses.Queries;
//
// public class GetExpenseRequestByIdQueryHandlerTests
// {
//     private readonly Mock<IExpenseDbContext> _contextMock;
//     private readonly Mock<ICurrentUserService> _currentUserServiceMock;
//     private readonly GetExpenseRequestByIdQueryHandler _handler;
//
//     public GetExpenseRequestByIdQueryHandlerTests()
//     {
//         _contextMock = new Mock<IExpenseDbContext>();
//         _currentUserServiceMock = new Mock<ICurrentUserService>();
//         _handler = new GetExpenseRequestByIdQueryHandler(_contextMock.Object, _currentUserServiceMock.Object);
//     }
//
//     [Fact]
//     public async Task Handle_WhenTenantIdIsNull_ShouldReturnNull()
//     {
//         // Arrange
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns((Guid?)null);
//         var query = new GetExpenseRequestByIdQuery { Id = Guid.NewGuid() };
//
//         // Act
//         var result = await _handler.Handle(query, CancellationToken.None);
//
//         // Assert
//         result.Should().BeNull();
//         _contextMock.Verify(x => x.ExpenseRequests, Times.Never);
//     }
//
//     [Fact]
//     public async Task Handle_WhenExpenseBelongsToSameTenant_ShouldReturnDto()
//     {
//         // Arrange
//         var tenantId = Guid.NewGuid();
//         var expense = new ExpenseRequest(tenantId, Guid.NewGuid(), 1500, "Uçak Bileti");
//         
//         var expenses = new List<ExpenseRequest> { expense };
//         
//         // Temizlenen Kısım: Sadece ReturnsDbSet kullanıyoruz
//         _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
//         
//         var query = new GetExpenseRequestByIdQuery { Id = expense.Id };
//
//         // Act
//         var result = await _handler.Handle(query, CancellationToken.None);
//
//         // Assert
//         result.Should().NotBeNull();
//         result!.Id.Should().Be(expense.Id);
//         result.Amount.Should().Be(1500);
//         result.Description.Should().Be("Uçak Bileti");
//     }
//
//     [Fact]
//     public async Task Handle_WhenExpenseBelongsToDifferentTenant_ShouldReturnNull()
//     {
//         // Arrange
//         var currentTenantId = Guid.NewGuid();
//         var differentTenantId = Guid.NewGuid();
//         
//         var expense = new ExpenseRequest(differentTenantId, Guid.NewGuid(), 5000, "Gizli Şirket Harcaması");
//         
//         var expenses = new List<ExpenseRequest> { expense };
//         
//         // Temizlenen Kısım: Sadece ReturnsDbSet kullanıyoruz
//         _contextMock.Setup(x => x.ExpenseRequests).ReturnsDbSet(expenses);
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(currentTenantId); 
//         
//         var query = new GetExpenseRequestByIdQuery { Id = expense.Id };
//
//         // Act
//         var result = await _handler.Handle(query, CancellationToken.None);
//
//         // Assert
//         result.Should().BeNull();
//     }
// }