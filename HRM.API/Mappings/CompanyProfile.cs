using AutoMapper;
using HRM.Core.DTOs.Company;
using HRM.Core.Entities;

namespace HRM.API.Mappings;

public class CompanyProfile : Profile
{
    public CompanyProfile()
    {
        CreateMap<Company, CompanyResponseDto>()
            .ForMember(dest => dest.LogoUrl, opt => opt.Ignore());

        CreateMap<CreateCompanyDto, Company>();
        CreateMap<UpdateCompanyDto, Company>();
    }
}
