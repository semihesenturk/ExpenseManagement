using Expense.Domain.Entities;
using Expense.Domain.Enums;
using FluentAssertions;

namespace Expense.UnitTests.Domain.Entites;

public class ExpenseRequestTests
{
    private static ExpenseRequest CreateExpense(decimal amount = 1500m)
        => new ExpenseRequest(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            amount, 
            "Yirmi karakterden uzun açıklama",
            ExpenseCategory.Travel,
            Currency.TRY);

    [Fact]
    public void Constructor_WhenCreated_ShouldSetInitialStatusToPending()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var expense = new ExpenseRequest(
            tenantId, userId, 1500m,
            "Yirmi karakterden uzun açıklama",
            ExpenseCategory.Travel,
            Currency.TRY);

        expense.Status.Should().Be(ExpenseStatus.Pending);
        expense.Amount.Should().Be(1500m);
        expense.TenantId.Should().Be(tenantId);
        expense.RequestDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Approve_WhenCalled_ShouldSetStatusToApproved()
    {
        var expense = CreateExpense();
        expense.Approve(Guid.NewGuid(), "Onaylandı.");

        expense.Status.Should().Be(ExpenseStatus.Approved);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldThrowInvalidOperationException()
    {
        var expense = CreateExpense();
        expense.Approve(Guid.NewGuid());

        Action act = () => expense.Approve(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_WhenCalled_ShouldSetStatusToRejected()
    {
        var expense = CreateExpense();
        expense.Reject(Guid.NewGuid(), "Fatura eksik.");

        expense.Status.Should().Be(ExpenseStatus.Rejected);
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldThrowInvalidOperationException()
    {
        var expense = CreateExpense();
        expense.Reject(Guid.NewGuid());

        Action act = () => expense.Reject(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SendToAdminApproval_WhenPending_ShouldSetStatusToPendingAdminApproval()
    {
        var expense = CreateExpense(8000m);
        expense.SendToAdminApproval(Guid.NewGuid(), "Admin onayı bekleniyor.");

        expense.Status.Should().Be(ExpenseStatus.PendingAdminApproval);
    }

    [Fact]
    public void SendToAdminApproval_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        var expense = CreateExpense();
        expense.Approve(Guid.NewGuid());

        Action act = () => expense.SendToAdminApproval(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }
}