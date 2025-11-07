using AutoMapper;
using GoalTrackerApp.Repositories;

namespace GoalTrackerApp.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ApplicationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public UserService UserService => new(_unitOfWork, _mapper);
        public GoalService GoalService => new(_unitOfWork, _mapper);
        public GoalCategoryService GoalCategoryService => new(_unitOfWork, _mapper);
    }
}