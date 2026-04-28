// using Expense.Application.Common.Interfaces;
// using Expense.Application.Contracts.Persistence;
// using Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;
// using Expense.Domain.Entities;
// using Expense.Domain.Enums;
// using FluentAssertions;
// using MassTransit;
// using Moq;
// using Shared.Contracts.Events;
//
// namespace Expense.UnitTests.Application.Features.Expenses.Commands;
//
// public class ApproveExpenseRequestCommandHandlerTests
// {
//     private readonly Mock<IExpenseRequestRepository> _repositoryMock;
//     private readonly Mock<ICurrentUserService> _currentUserServiceMock;
//     private readonly Mock<IPublishEndpoint> _publishEndpointMock;
//     private readonly ApproveExpenseRequestCommandHandler _handler;
//
//     public ApproveExpenseRequestCommandHandlerTests()
//     {
//         _repositoryMock = new Mock<IExpenseRequestRepository>();
//         _currentUserServiceMock = new Mock<ICurrentUserService>();
//         _publishEndpointMock = new Mock<IPublishEndpoint>();
//
//         _handler = new ApproveExpenseRequestCommandHandler(
//             _repositoryMock.Object,
//             _currentUserServiceMock.Object,
//             _publishEndpointMock.Object);
//     }
//
//     [Fact]
//     public async Task Handle_WhenAmountIsUnder5000_AndUserIsHr_ShouldApproveImmediately()
//     {
//         // Arrange
//         var tenantId = Guid.NewGuid();
//         var userId = Guid.NewGuid();
//         var expenseId = Guid.NewGuid();
//         
//         var expense = new ExpenseRequest(tenantId, userId, 2000, "Test Expense");
//         
//         _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
//         _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
//         
//         _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
//             .ReturnsAsync(expense);
//
//         var command = new ApproveExpenseRequestCommand(expenseId, "Approval Note");
//
//         // Act
//         await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         expense.Status.Should().Be(ExpenseStatus.Approved);
//         _repositoryMock.Verify(x => x.UpdateAsync(expense, It.IsAny<CancellationToken>()), Times.Once);
//         _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_WhenDifferentTenant_ShouldThrowUnauthorizedAccessException()
//     {
//         // Arrange
//         var myTenantId = Guid.NewGuid();
//         var otherTenantId = Guid.NewGuid();
//         var expenseId = Guid.NewGuid();
//         
//         var expense = new ExpenseRequest(otherTenantId, Guid.NewGuid(), 1000, "Other Company Expense");
//         
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(myTenantId);
//         _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
//             .ReturnsAsync(expense);
//
//         var command = new ApproveExpenseRequestCommand(expenseId, "Hacker Note");
//
//         // Act
//         Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         await act.Should().ThrowAsync<UnauthorizedAccessException>();
//     }
//
//     [Fact]
//     public async Task Handle_WhenAmountIsOver5000_AndUserIsHr_ShouldSetStatusToPendingAdminApproval()
//     {
//         // Arrange
//         var tenantId = Guid.NewGuid();
//         var expenseId = Guid.NewGuid();
//         
//         var expense = new ExpenseRequest(tenantId, Guid.NewGuid(), 7500, "High Amount Expense");
//         
//         _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
//         _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "HR" });
//         
//         _repositoryMock.Setup(x => x.GetByIdAsync(expenseId, It.IsAny<CancellationToken>()))
//             .ReturnsAsync(expense);
//
//         var command = new ApproveExpenseRequestCommand(expenseId, "HR Approved, waiting for Admin");
//
//         // Act
//         await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         expense.Status.Should().Be(ExpenseStatus.PendingAdminApproval);
//         _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
//     }
// }