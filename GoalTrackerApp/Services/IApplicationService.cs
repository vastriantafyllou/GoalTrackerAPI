namespace GoalTrackerApp.Services;

public interface IApplicationService
{ 
    UserService UserService { get;  }
    GoalService GoalService { get;  }
    GoalCategoryService GoalCategoryService { get;  }
}