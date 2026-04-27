using Expense.Application.Common.Interfaces;
using Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
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
    private readonly ICurrentUserService _currentUserService;

    public ExpensesController(IMediator mediator,  ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Yeni harcama talebi oluşturur.
    /// Tüm roller (Admin, Approver, Employee) harcama girebilir.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Approver,Employee")] 
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequestCommand command)
        => Ok(await _mediator.Send(command));

    /// <summary>
    /// Harcamaları listeler. 
    /// Handler içindeki mantık sayesinde Employee sadece kendininkileri, 
    /// Admin/Approver ise herkesinkini görür.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Approver,Employee")]
    public async Task<IActionResult> GetAll([FromQuery] GetExpenseRequestsQuery query)
        => Ok(await _mediator.Send(query));

    /// <summary>
    /// Tekil harcama detayı getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Approver,Employee")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetExpenseRequestByIdQuery { Id = id });
        return result != null ? Ok(result) : NotFound("Harcama bulunamadı veya bu veriye erişim yetkiniz yok.");
    }

    /// <summary>
    /// Harcamayı onaylar. Sadece yetkili roller yapabilir.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Approver")] 
    public async Task<IActionResult> Approve(Guid id, [FromBody] string? note)
    {
        var command = new ApproveExpenseRequestCommand(id, _currentUserService.UserId.Value, note);
    
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Harcamayı reddeder. Sadece yetkili roller yapabilir.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Approver")] 
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectExpenseRequestCommand command)
    {
        // Command içindeki ID'leri ve reddeden kişiyi setliyoruz
        var updatedCommand = command with 
        { 
            ExpenseRequestId = id, 
            ApproverId = _currentUserService.UserId.Value 
        };

        return Ok(await _mediator.Send(updatedCommand));
    }
}