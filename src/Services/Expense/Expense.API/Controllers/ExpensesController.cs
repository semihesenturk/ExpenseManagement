using Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequests;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequestById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Expense.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Yeni harcama talebi oluşturur.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequestCommand command)
        => Ok(await _mediator.Send(command));

    /// <summary>
    /// Harcamaları listeler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetExpenseRequestsQuery query)
        => Ok(await _mediator.Send(query));

    /// <summary>
    /// Tekil harcama detayı getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetExpenseRequestByIdQuery { Id = id });
        return result != null ? Ok(result) : NotFound("Harcama bulunamadı veya bu veriye erişim yetkiniz yok.");
    }

    /// <summary>
    /// Harcamayı onaylar. (HR veya Admin yapabilir)
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Approver")] 
    public async Task<IActionResult> Approve(Guid id, [FromBody] string? note)
    {
        var command = new ApproveExpenseRequestCommand(id, note);
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Harcamayı reddeder. (HR veya Admin yapabilir)
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectExpenseRequestCommand command)
    {
        // Record tipinde 'with' kullanımı harika, sadece ID'yi eşliyoruz
        var updatedCommand = command with { ExpenseRequestId = id };
        return Ok(await _mediator.Send(updatedCommand));
    }
}