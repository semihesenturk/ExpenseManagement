// using Expense.Application.Common.Interfaces;
// using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
// using Expense.Domain.Entities;
// using FluentAssertions;
// using MassTransit;
// using Microsoft.EntityFrameworkCore;
// using Moq;
// using Shared.Contracts.Events;
//
// namespace Expense.UnitTests.Application.Features.Expenses.Commands;
//
// public class CreateExpenseRequestCommandHandlerTests
// {
//     private readonly Mock<IExpenseDbContext> _contextMock;
//     private readonly Mock<ICurrentUserService> _currentUserServiceMock;
//     private readonly Mock<IPublishEndpoint> _publishEndpointMock;
//     private readonly CreateExpenseRequestCommandHandler _handler;
//
//     public CreateExpenseRequestCommandHandlerTests()
//     {
//         _contextMock = new Mock<IExpenseDbContext>();
//         _currentUserServiceMock = new Mock<ICurrentUserService>();
//         _publishEndpointMock = new Mock<IPublishEndpoint>();
//
//         // DbContext üzerinden Add işlemi yapıldığı için ExpenseRequests DbSet'ini mockluyoruz
//         var dbSetMock = new Mock<DbSet<ExpenseRequest>>();
//         _contextMock.Setup(x => x.ExpenseRequests).Returns(dbSetMock.Object);
//
//         _handler = new CreateExpenseRequestCommandHandler(
//             _contextMock.Object,
//             _currentUserServiceMock.Object,
//             _publishEndpointMock.Object);
//     }
//
//     [Fact]
//     public async Task Handle_WhenValidRequest_ShouldCreateExpenseAndPublishEvent()
//     {
//         // Arrange: Geçerli kullanıcı ve tenant bilgileriyle harcama oluşturma
//         var tenantId = Guid.NewGuid();
//         var userId = Guid.NewGuid();
//         
//         _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenantId);
//
//         var command = new CreateExpenseRequestCommand(3000, "Yeni Monitör");
//
//         // Act
//         var result = await _handler.Handle(command, CancellationToken.None);
//
//         // Assert: DTO'nun doğru dönüp dönmediğini, DB'ye eklenip eklenmediğini ve Event'in gidip gitmediğini kontrol et
//         result.Should().NotBeNull();
//         result.Amount.Should().Be(3000);
//         result.Description.Should().Be("Yeni Monitör");
//         
//         // Veritabanına Add ve SaveChangesAsync çağrılmış mı?
//         _contextMock.Verify(x => x.ExpenseRequests.Add(It.IsAny<ExpenseRequest>()), Times.Once);
//         _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//         
//         // RabbitMQ'ya event fırlatılmış mı?
//         _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_WhenUserOrTenantIsMissing_ShouldThrowUnauthorizedAccessException()
//     {
//         // Arrange: Token'ın patlak olduğu, sistemde TenantId veya UserId'nin olmadığı senaryo
//         _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
//         _currentUserServiceMock.Setup(x => x.TenantId).Returns((Guid?)null);
//
//         var command = new CreateExpenseRequestCommand(1000, "Hayalet Harcama");
//
//         // Act
//         Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
//
//         // Assert: UnauthorizedAccessException fırlatmalı ve hiçbir işlem yapılmamalı
//         await act.Should().ThrowAsync<UnauthorizedAccessException>()
//             .WithMessage("Kullanıcı veya Tenant bilgisi bulunamadı.");
//         
//         // İşlem patladığı için DB'ye kayıt veya event fırlatma olmamalı
//         _contextMock.Verify(x => x.ExpenseRequests.Add(It.IsAny<ExpenseRequest>()), Times.Never);
//         _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ExpenseCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
//     }
// }