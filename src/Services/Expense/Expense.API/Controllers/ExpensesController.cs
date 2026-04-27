using Expense.Application.Features.Expenses.Commands.ApproveExpenseRequest;
using Expense.Application.Features.Expenses.Commands.RejectExpenseRequest;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Expense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExpensesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseRequestCommand command)
            => Ok(await _mediator.Send(command));

        [HttpGet("{requestedById:guid}")]
        public async Task<IActionResult> GetByRequester(Guid requestedById)
            => Ok(await _mediator.Send(new GetExpenseRequestsQuery(requestedById)));

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveExpenseRequestCommand command)
            => Ok(await _mediator.Send(command with { ExpenseRequestId = id }));

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectExpenseRequestCommand command)
            => Ok(await _mediator.Send(command with { ExpenseRequestId = id }));
    }
}