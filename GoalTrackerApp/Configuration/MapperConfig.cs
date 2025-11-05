using AutoMapper;
using GoalTrackerApp.Data;
using GoalTrackerApp.Dto;

namespace GoalTrackerApp.Configuration
{
    /// <summary>
    /// Configures AutoMapper mappings for DTOs and entities.
    /// </summary>
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            // User mappings
            CreateMap<UserSignupDto, User>();
            CreateMap<User, UserReadOnlyDto>()
                .ForMember(dest => dest.UserRole, 
                    opt => opt.MapFrom(src => src.UserRole.ToString()));

            // Goal mappings
            CreateMap<Goal, GoalReadOnlyDto>()
                .ForMember(dest => dest.Status, 
                    opt => opt.MapFrom(src => src.GoalStatus.ToString()));

            CreateMap<GoalCreateDto, Goal>();
            
            CreateMap<GoalUpdateDto, Goal>()
                .ForMember(dest => dest.GoalStatus, 
                    opt => opt.MapFrom(src => src.Status));
        }
    }
}