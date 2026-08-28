using AutoMapper;
using Library.Application.DTOs.Books;
using Library.Domain.Entities;

namespace Library.Application.Mapping;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<Book, BookDetailsDto>();
        
        CreateMap<CreateBookRequest, Book>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableCopies, opt => opt.MapFrom(src => src.TotalCopies));
    }
}