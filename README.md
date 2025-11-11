# Goal Tracker Full-Stack Application 🎯

This will be a full-stack web application for managing personal goals, built with a .NET API backend and a React frontend.

## Tech Stack

### Backend
* .NET 8 (C#)
* Entity Framework Core 8
* MS SQL Server
* JWT Bearer Authentication
* Repository & Unit of Work Patterns

### Frontend
* React
* React Router
* Axios
* JavaScript (ES6+)

## Planned Features

* **User Authentication:** User registration and login.
* **Goal Management:** Full CRUD (Create, Read, Update, Delete) operations for goals.
* **Category Management:** Create and manage categories to organize goals.
* **User Dashboard:** A simple view of user statistics (e.g., total goals, completed goals).
* **Admin Role:** Admin role to manage users and data.

## User Roles and Admin Management

### Role Structure
This backend implements a three-tier role system:
- **SuperAdmin**: Full system access with ability to manage all users and promote/demote admins
- **Admin**: Can manage users and content
- **User**: Standard user with access to their own content

### SuperAdmin Account
* A **SuperAdmin** account is automatically created when the application starts for the first time
* The SuperAdmin is seeded directly into the database via the startup process in `Program.cs`
* **Default Credentials:**
  - Username: `superadmin`
  - Email: `superadmin@goaltracker.com`
  - Password: Configured via `SuperAdminPassword` environment variable or User Secret (defaults to `SuperAdmin@123!` if not set)
  
**To set a custom SuperAdmin password:**
```bash
# Using .NET User Secrets (recommended for development)
dotnet user-secrets set "SuperAdminPassword" "YourSecurePassword@2025!" --project GoalTrackerApp

# Using environment variable (for production)
export SuperAdminPassword="YourSecurePassword@2025!"
```

### Security Model
* The `/register` endpoint creates **only standard users** for security reasons
* Regular users cannot self-promote to Admin or SuperAdmin roles
* Only the **SuperAdmin** can promote or demote users to/from the Admin role using dedicated endpoints:
  - `PATCH /api/users/{id}/promote` - Promote user to Admin
  - `PATCH /api/users/{id}/demote` - Demote user to regular User
* Role changes are performed through secure, authenticated API endpoints with JWT validation
* Authentication and authorization are handled via JWT tokens with role-based access control

### Admin Dashboard (Future Frontend)
When the frontend is implemented, the SuperAdmin will have access to an Admin Dashboard where they can:
* View a list of all registered users
* Search and filter users by username, email, or role
* Promote users to Admin role or demote them back to User
* Perform CRUD operations on user accounts
