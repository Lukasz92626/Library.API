using AutoMapper;
using Library.Application.DTOs.Books;
using Library.Domain.Entities;

namespace Library.Application.Mapping;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookDto>();
    }
}