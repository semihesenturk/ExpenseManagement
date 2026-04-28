using AutoMapper;
using Expense.Application.Features.Expenses.Commands.CreateExpenseRequest;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequestById;
using Expense.Application.Features.Expenses.Queries.GetExpenseRequests;
using Expense.Domain.Entities;

namespace Expense.Application.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ExpenseRequest → CreateExpenseRequestDto
        CreateMap<ExpenseRequest, CreateExpenseRequestDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.RequestedById.ToString()))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));

        // ExpenseRequest → GetExpenseRequestByIdDto
        CreateMap<ExpenseRequest, GetExpenseRequestByIdDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // ExpenseRequest → GetExpenseRequestsDto
        CreateMap<ExpenseRequest, GetExpenseRequestsDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}