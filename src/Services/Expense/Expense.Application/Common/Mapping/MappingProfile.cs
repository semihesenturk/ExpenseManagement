using AutoMapper;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequests;
using Expense.Domain.Entities;

namespace Expense.Application.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateExpenseRequestCommand, ExpenseRequest>();
        CreateMap<ExpenseRequest, CreateExpenseRequestDto>();
        CreateMap<ExpenseRequest, GetExpenseRequestsDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}