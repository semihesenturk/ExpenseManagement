using Expense.Domain.Entities;
using Expense.Domain.Enums;
using FluentAssertions;

namespace Expense.UnitTests.Domain.Entities;

public class ExpenseRequestTests
{
    [Fact]
    public void Constructor_WhenCreated_ShouldSetInitialStatusToPending()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var amount = 1500m;
        var description = "Yeni Monitör";

        // Act
        var expense = new ExpenseRequest(tenantId, userId, amount, description);

        // Assert: Yeni yaratılan bir harcamanın statüsü her zaman Pending olmalıdır.
        expense.Status.Should().Be(ExpenseStatus.Pending);
        expense.Amount.Should().Be(amount);
        expense.Description.Should().Be(description);
        expense.TenantId.Should().Be(tenantId);
        expense.RequestDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1)); 
    }

    [Fact]
    public void Approve_WhenCalled_ShouldSetStatusToApproved()
    {
        // Arrange
        var expense = new ExpenseRequest(Guid.NewGuid(), Guid.NewGuid(), 1000, "Taksi Ücreti");
        var approverId = Guid.NewGuid();
        var note = "Fiş ektedir, onaylandı.";

        // Act
        expense.Approve(approverId, note);

        // Assert
        expense.Status.Should().Be(ExpenseStatus.Approved);
        // Eğer entity içinde ApproverId ve Note gibi alanları tutuyorsan (örneğin Audit log için), 
        // onları da burada Assert edebilirsin.
    }

    [Fact]
    public void Reject_WhenCalled_ShouldSetStatusToRejected()
    {
        // Arrange
        var expense = new ExpenseRequest(Guid.NewGuid(), Guid.NewGuid(), 500, "Yemek");
        var approverId = Guid.NewGuid();
        var note = "Şirket dışı yemekler karşılanmıyor.";

        // Act
        expense.Reject(approverId, note);

        // Assert
        expense.Status.Should().Be(ExpenseStatus.Rejected);
    }

    [Fact]
    public void SendToAdminApproval_WhenCalled_ShouldSetStatusToPendingAdminApproval()
    {
        // Arrange
        var expense = new ExpenseRequest(Guid.NewGuid(), Guid.NewGuid(), 8000, "Uluslararası Uçak Bileti");
        var hrUserId = Guid.NewGuid();
        var note = "Tutar 5000 üzeri, Admin onayı bekleniyor.";

        // Act
        expense.SendToAdminApproval(hrUserId, note);

        // Assert
        expense.Status.Should().Be(ExpenseStatus.PendingAdminApproval);
    }
}