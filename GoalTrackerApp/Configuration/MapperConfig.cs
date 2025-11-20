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
                    opt => opt.MapFrom(src => src.GoalStatus.ToString()))
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(src => src.InsertedAt))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.GoalCategory != null ? src.GoalCategory.Name : null));

            CreateMap<GoalCreateDto, Goal>();
            
            CreateMap<GoalUpdateDto, Goal>()
                .ForMember(dest => dest.GoalStatus, 
                    opt => opt.MapFrom(src => src.Status));

            // GoalCategory mappings
            CreateMap<GoalCategoryCreateDto, GoalCategory>();
            
            CreateMap<GoalCategoryUpdateDto, GoalCategory>();
            
            CreateMap<GoalCategory, GoalCategoryReadOnlyDto>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(src => src.InsertedAt))
                .ForMember(dest => dest.GoalCount,
                    opt => opt.MapFrom(src => src.Goals.Count(g => !g.IsDeleted)));
        }
    }
}