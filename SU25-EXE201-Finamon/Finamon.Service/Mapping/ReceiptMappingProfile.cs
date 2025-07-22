using AutoMapper;
using Finamon_Data.Entities;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;

namespace Finamon.Service.Mapping
{
    public class ReceiptMappingProfile : Profile
    {
        public ReceiptMappingProfile()
        {
            CreateMap<Receipt, ReceiptRequestModel>().ReverseMap();
            CreateMap<Receipt, ReceiptResponseModel>().ReverseMap();
        }
    }
} 