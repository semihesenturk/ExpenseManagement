using Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequests;
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
        
    [HttpPost]
    [Authorize(Roles = "Personnel")] 
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequestCommand command)
        => Ok(await _mediator.Send(command));

        
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetExpenseRequestsQuery query)
        => Ok(await _mediator.Send(query));
        
    [HttpGet("{id:guid}")]
    [AllowAnonymous] 
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(new { Id = id, Description = "Sistem Test Harcaması", Amount = 1500, Status = "Pending" });
    }
        
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "HR,Admin")] 
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveExpenseRequestCommand command)
        => Ok(await _mediator.Send(command with { ExpenseRequestId = id }));
        
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "HR,Admin")] 
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectExpenseRequestCommand command)
        => Ok(await _mediator.Send(command with { ExpenseRequestId = id }));
}