using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlusNine.DataService.Repositories;
using PlusNine.DataService.Repositories.Interfaces;

namespace PlusNine.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        public BaseController(
            IUnitOfWork unitOfWork,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }
}
