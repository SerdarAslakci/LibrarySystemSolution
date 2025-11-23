using AutoMapper;
using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.API.Dtos.FineTypeDtos;
using LibrarySystem.API.Dtos.LoanDtos;
using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateAuthorDto, Author>().ReverseMap();
            CreateMap<AppUser, UserViewDto>().ReverseMap();

            CreateMap<Loan, ReturnSummaryDto>()
                .ForMember(dest => dest.LoanId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.BookCopy.Book.Title))
                .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.BookCopy.BarcodeNumber))
                .ForMember(dest => dest.MemberFullName, opt => opt.MapFrom(src =>
                    src.AppUser != null
                        ? $"{src.AppUser.FirstName} {src.AppUser.LastName}"
                        : "Kullanıcı Bilgisi Yok"))
                .ForMember(dest => dest.MemberPhone, opt => opt.MapFrom(src =>
                    src.AppUser != null ? src.AppUser.PhoneNumber : "Telefon Yok"))
                .ForMember(dest => dest.ReturnedDate, opt => opt.MapFrom(src => src.ActualReturnDate))
                .ForMember(dest => dest.ReturnStatus, opt => opt.MapFrom(src =>
                    src.ActualReturnDate > src.ExpectedReturnDate ? "GECİKMELİ İADE" : "Zamanında İade"))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src =>
                    src.ActualReturnDate > src.ExpectedReturnDate
                        ? "Kitap gecikmeli iade edildi. Lütfen ceza durumunu kontrol ediniz."
                        : "Teşekkürler, kitap zamanında iade alındı."));

            CreateMap<Loan, LoanHistoryDto>()
                .ForMember(dest => dest.LoanId,opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.BookTitle,
                           opt => opt.MapFrom(src => src.BookCopy.Book.Title))
                .ForMember(dest => dest.Isbn,
                           opt => opt.MapFrom(src => src.BookCopy.Book.ISBN))
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src =>
                        src.BookCopy.Book.BookAuthors
                           .Select(ba => ba.Author.FirstName + " " + ba.Author.LastName)
                           .Aggregate((current, next) => current + ", " + next)
                    ))
                .ForMember(dest => dest.Room,
                           opt => opt.MapFrom(src => src.BookCopy.Shelf.Room.RoomCode))
                .ForMember(dest => dest.Shelf,
                           opt => opt.MapFrom(src => src.BookCopy.Shelf.ShelfCode));

            CreateMap<Fine, UserFineDto>()
                .ForMember(dest => dest.FineId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LoanDate, opt => opt.MapFrom(src => src.Loan.LoanDate))
                .ForMember(dest => dest.ExpectedReturnDate, opt => opt.MapFrom(src => src.Loan.ExpectedReturnDate))
                .ForMember(dest => dest.ActualReturnDate, opt => opt.MapFrom(src => src.Loan.ActualReturnDate))
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Loan.BookCopy.Book.Title))
                .ForMember(dest => dest.BarcodeNumber, opt => opt.MapFrom(src => src.Loan.BookCopy.BarcodeNumber))
                .ForMember(dest => dest.FineType, opt => opt.MapFrom(src => src.FineType.Name));

            CreateMap<FineType, ReturnFineTypeDto>().ReverseMap();
        }
    }
}
