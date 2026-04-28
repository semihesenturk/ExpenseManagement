// using Expense.Application.Common.Interfaces;
// using Expense.Application.Contracts.Persistence;
// using Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;
// using Expense.Domain.Entities;
// using Expense.Domain.Enums;
// using FluentAssertions;
// using MassTransit;
// using Moq;
// using Shared.Contracts.Events;
//
// namespace Expense.UnitTests.Application.Features.Expenses.Commands;
//
// public class RejectExpenseRequestCommandHandlerTests
// {
//     private readonly Mock<IExpenseRequestRepository> _repositoryMock;
//     private readonly Mock<ICurrentUserService> _currentUserServiceMock;
//     private readonly Mock<IPublishEndpoint> _publishEndpointMock;
//     private readonly RejectExpenseRequestCommandHandler _handler;
//
//     public RejectExpenseRequestCommandHandlerTests()
//     {
//         _repositoryMock = new Mock<IExpenseRequestRepository>();
//         _currentUserServiceMock = new Mock<ICurrentUserService>();
//         _publishEndpointMock = new Mock<IPublishEndpoint>();
//
//         _handler = new RejectExpenseRequestCommandHandler(
//             _repositoryMock.Object,
//             _currentUserServiceMock.Object,
//             _publishEndpointMock.Object);
//     }
//
//     [Fact]
//     public async Task Handle_WhenUserHasPermission_ShouldRejectAndPublishEvent()
//     {
//         // Arrange: Her şeyin yolunda olduğu (Happy Path) senaryo
//         var tenantId = Guid.NewGuid();
//         var userId = Guid.NewGuid();
//         var expenseId = Guid.NewGuid();
//         
//         var expense = new ExpenseRequest(tenantId, userId, 1500, "Test Harcaması");
//         
//         _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
//         _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
//         
//         _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
//             .ReturnsAsync(expense);
//
//         var command = new RejectExpenseRequestCommand(expenseId, "Fatura eksik yüklenmiş.");
//
//         // Act
//         await _handler.Handle(command, CancellationToken.None);
//
//         // Assert: Statü değişmeli, DB güncellenmeli ve RabbitMQ'ya mesaj gitmeli
//         expense.Status.Should().Be(ExpenseStatus.Rejected);
//         _repositoryMock.Verify(x => x.UpdateAsync(expense, It.IsAny<CancellationToken>()), Times.Once);
//         _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseRejectedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_WhenDifferentTenant_ShouldThrowUnauthorizedAccessException()
//     {
//         // Arrange: Başka bir şirketin verisine sızmaya çalışan kullanıcı
//         var myTenantId = Guid.NewGuid();
//         var otherTenantId = Guid.NewGuid();
//         var expenseId = Guid.NewGuid();
//         
//         var expense = new ExpenseRequest(otherTenantId, Guid.NewGuid(), 1000, "Diğer Şirket Harcaması");
//         
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(myTenantId);
//         _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
//             .ReturnsAsync(expense);
//
//         var command = new RejectExpenseRequestCommand(expenseId, "Ben reddediyorum!");
//
//         // Act
//         Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
//
//         // Assert: Sistem buna izin vermeyip exception fırlatmalı
//         await act.Should().ThrowAsync<UnauthorizedAccessException>();
//     }
//
//     [Fact]
//     public async Task Handle_WhenUserHasNoManagerialRole_ShouldThrowUnauthorizedAccessException()
//     {
//         // Arrange: Kendi tenant'ında ama yetkisi olmayan düz çalışan
//         var tenantId = Guid.NewGuid();
//         var userId = Guid.NewGuid();
//         var expenseId = Guid.NewGuid();
//         
//         var expense = new ExpenseRequest(tenantId, Guid.NewGuid(), 1000, "Düz Çalışan Harcaması");
//         
//         _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
//         _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "Employee" }); // Yetkisiz rol
//         
//         _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
//             .ReturnsAsync(expense);
//
//         var command = new RejectExpenseRequestCommand(expenseId, "Sevmedim reddediyorum.");
//
//         // Act
//         Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
//
//         // Assert: Rolü uymadığı için patlamalı
//         await act.Should().ThrowAsync<UnauthorizedAccessException>();
//     }
//
//     [Fact]
//     public async Task Handle_WhenExpenseNotFound_ShouldThrowKeyNotFoundException()
//     {
//         // Arrange: Olmayan bir ID ile istek atılması
//         var expenseId = Guid.NewGuid();
//         
//         _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());
//         _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
//         
//         _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
//             .ReturnsAsync((ExpenseRequest)null!);
//
//         var command = new RejectExpenseRequestCommand(expenseId, "Bulamadığım şeyi reddediyorum.");
//
//         // Act
//         Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         await act.Should().ThrowAsync<KeyNotFoundException>();
//     }
// }