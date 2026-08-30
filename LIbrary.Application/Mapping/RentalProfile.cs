using AutoMapper;
using Library.Application.DTOs.Rentals;
using Library.Domain.Entities;

namespace Library.Application.Mapping;

public class RentalProfile : Profile
{
    public RentalProfile()
    {
        CreateMap<Rental, RentalResponse>()
            .ForMember(dest => dest.BookTitle, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}