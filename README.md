# 🎯 GoalTracker API - Backend

> **A modern, secure REST API for personal goal management built with .NET 8, Entity Framework Core, and JWT authentication.**

This repository contains the **backend API** for the GoalTracker application. The frontend is being developed in **React + TypeScript** and will be available soon.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Tech Stack](#tech-stack)
- [Getting Started](#-getting-started)
- [Configuration](#configuration)
- [API Documentation](#-api-documentation)
- [Authentication & Roles](#-authentication--roles)
- [License](#-license)

---

## 🌟 Overview

**GoalTracker API** is a comprehensive backend solution for managing personal and professional goals. It provides a secure, role-based system that allows users to:

- Create, track, and manage personal goals
- Organize goals into categories
- Monitor progress and completion status
- Manage user accounts with role-based access control
- Secure authentication with JWT tokens

### Architecture Highlights

- **Clean Architecture** with Repository and Unit of Work patterns
- **Role-Based Authorization** (SuperAdmin, Admin, User)
- **Automatic SuperAdmin Seeding** on first startup
- **Secure Configuration** via User Secrets and Environment Variables
- **Comprehensive Logging** with Serilog
- **API Documentation** with Swagger/OpenAPI
- **BCrypt Password Hashing** for security

---

## ✨ Features

### 🔐 Authentication & Authorization
- JWT-based authentication with bearer tokens
- Three-tier role system (SuperAdmin, Admin, User)
- Secure password hashing with BCrypt
- User Secrets for sensitive configuration

### 🎯 Goal Management
- Full CRUD operations for goals
- Goal categorization
- Progress tracking
- User-specific goal isolation

### 👥 User Management
- User registration and login
- Profile management
- Admin and SuperAdmin user administration
- Role promotion/demotion (SuperAdmin only)

### 📊 Category Management
- Create custom goal categories
- Organize goals efficiently
- Category-based filtering

### 🛡️ Security
- No hardcoded credentials
- Environment-based configuration
- Role-based endpoint protection
- Soft delete for data preservation

---

## 🛠️ Tech Stack

### Core Technologies
- **.NET 8** - Modern, high-performance web framework
- **C# 12** - Latest language features
- **Entity Framework Core 9** - ORM for database operations
- **SQL Server** - Relational database

### Libraries & Tools
- **AutoMapper** - Object-to-object mapping
- **BCrypt.Net** - Password hashing
- **Serilog** - Structured logging
- **Swashbuckle** - Swagger/OpenAPI documentation
- **JWT Bearer** - Token-based authentication
- **Newtonsoft.Json** - JSON serialization

### Development Tools
- **JetBrains Rider** / **Visual Studio**
- **SQL Server Management Studio**
- **Postman** / **Swagger UI** for API testing

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or Developer Edition)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/vastriantafyllou/GoalTrackerAPI.git
   cd GoalTrackerAPI
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure User Secrets** (see [Configuration](#-configuration) section)

4. **Apply database migrations**
   ```bash
   dotnet ef database update --project GoalTrackerApp
   ```

5. **Run the application**
   ```bash
   dotnet run --project GoalTrackerApp
   ```

6. **Access Swagger UI**
   ```
   https://localhost:5001/swagger
   ```

---

## ⚙️ Configuration

### User Secrets (Development)

For development, use .NET User Secrets to store sensitive configuration:

```bash
cd GoalTrackerApp

# Database Configuration
dotnet user-secrets set "DB_SERVER" "localhost"
dotnet user-secrets set "DB_NAME" "GoalTrackerApiDB"
dotnet user-secrets set "DB_USER" "your_username"
dotnet user-secrets set "DB_PASS" "your_password"

# JWT Secret Key (must be at least 32 characters)
dotnet user-secrets set "Authentication:SecretKey" "YourVeryLongSecretKeyThatIsAtLeast32CharactersLongForHS256"

# SuperAdmin Password (IMPORTANT: Set a secure password!)
dotnet user-secrets set "SuperAdminPassword" "YourVerySecurePassword@2025!"
```

### Environment Variables (Production)

For production deployments, use environment variables:

```bash
export DB_SERVER="your-server.database.windows.net"
export DB_NAME="GoalTrackerApiDB"
export DB_USER="sqladmin"
export DB_PASS="YourSecurePassword!"
export Authentication__SecretKey="YourProductionSecretKey"
export SuperAdminPassword="YourProductionSuperAdminPassword@2025!"
```

### Connection String Format

The application constructs the connection string from environment variables:
```
Server={DB_SERVER};Database={DB_NAME};User={DB_USER};Password={DB_PASS};MultipleActiveResultSets=True;TrustServerCertificate=True
```

---

## 📚 API Documentation

### Base URL
```
https://localhost:5001/api
```

### Authentication Endpoints

#### Login
```http
POST /api/auth/LoginUserAsync
Content-Type: application/json

{
  "username": "superadmin",
  "password": "YourVerySecurePassword@2025!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "superadmin",
  "role": "SuperAdmin",
  "expiresAt": "2025-11-12T02:00:26Z"
}
```

### User Management Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/users/RegisterUserAsync` | Public | Register new user |
| `GET` | `/api/users/GetAllUsers` | Admin/SuperAdmin | Get all users (paginated) |
| `GET` | `/api/users/GetUserById/{id}` | Admin/SuperAdmin | Get user by ID |
| `GET` | `/api/users/GetUserByUsernameAsync/{username}` | Admin/SuperAdmin | Get user by username |
| `PUT` | `/api/users/UpdateUser/{id}` | Admin/SuperAdmin | Update user |
| `DELETE` | `/api/users/DeleteUser/{id}` | Admin/SuperAdmin | Soft delete user |
| `PATCH` | `/api/users/PromoteToAdmin/{id}` | **SuperAdmin** | Promote user to Admin |
| `PATCH` | `/api/users/DemoteToUser/{id}` | **SuperAdmin** | Demote user to regular User |

### Goal Management Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/goals/CreateGoal` | User | Create new goal |
| `GET` | `/api/goals/GetMyGoals` | User | Get all user's goals |
| `GET` | `/api/goals/GetGoal/{id}` | User | Get specific goal |
| `PUT` | `/api/goals/UpdateGoal/{id}` | User | Update goal |
| `DELETE` | `/api/goals/DeleteGoal/{id}` | User | Delete goal |

### Category Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/goalcategory/CreateCategory` | User | Create category |
| `GET` | `/api/goalcategory/GetMyCategories` | User | Get all categories |
| `GET` | `/api/goalcategory/GetCategory/{id}` | User | Get specific category |
| `PUT` | `/api/goalcategory/UpdateCategory/{id}` | User | Update category |
| `DELETE` | `/api/goalcategory/DeleteCategory/{id}` | User | Delete category |

### Request Examples

#### Register User
```bash
curl -X POST https://localhost:5001/api/users/RegisterUserAsync \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "password": "SecurePass@123!",
    "firstname": "John",
    "lastname": "Doe"
  }'
```

#### Create Goal
```bash
curl -X POST https://localhost:5001/api/goals/CreateGoal \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Learn .NET 8",
    "description": "Complete .NET 8 course",
    "categoryId": 1,
    "targetDate": "2025-12-31T00:00:00Z"
  }'
```

---

## 🔐 Authentication & Roles

### Role Hierarchy

```
SuperAdmin  →  Full system access
    ↓
   Admin    →  User management, content moderation
    ↓
   User     →  Personal goal management
```

### SuperAdmin Account

A **SuperAdmin** account is automatically created on first application startup:

- **Username:** `superadmin`
- **Email:** `superadmin@goaltracker.com`
- **Password:** Configured via `SuperAdminPassword` User Secret

**⚠️ IMPORTANT:** Always set a secure custom password before deploying to production!

### JWT Token Flow

1. **User Login** → Receives JWT token
2. **Include Token** → `Authorization: Bearer {token}` in requests
3. **Token Validation** → API validates token and role claims
4. **Access Granted** → Based on role permissions

### Token Expiration

- **Default:** 24 hours (configurable in `Program.cs`)
- **Remember Me:** Extended expiration available

### Security Best Practices

✅ **DO:**
- Use HTTPS in production
- Store JWT tokens securely (HttpOnly cookies recommended for web)
- Rotate JWT secret keys periodically
- Set strong SuperAdmin passwords
- Use User Secrets for local development

❌ **DON'T:**
- Hardcode credentials
- Commit secrets to version control
- Use default passwords in production
- Share JWT tokens between users

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Vasileios Triantafyllou**

- LinkedIn: [Vasileios Triantafyllou](https://www.linkedin.com/in/vasileios-triantafyllou-0b028710b/)
- GitHub: [@vastriantafyllou](https://github.com/vastriantafyllou)
- Email: triantafyllou.vasileios@gmail.com

---

## 🔗 Related Projects

- **GoalTracker Frontend** (Coming Soon) - React + TypeScript client application

---

## 📝 Changelog

### Version 1.0.0 (Current)
- ✅ JWT Authentication
- ✅ Role-Based Authorization (SuperAdmin, Admin, User)
- ✅ Goal Management CRUD
- ✅ Category Management
- ✅ User Management
- ✅ Automatic SuperAdmin Seeding
- ✅ Swagger Documentation
- ✅ Secure Configuration with User Secrets

---

## 🙏 Acknowledgments

- [.NET Team](https://github.com/dotnet) for the amazing framework
- [Entity Framework Core](https://github.com/dotnet/efcore) for ORM capabilities
- [AutoMapper](https://automapper.org/) for object mapping
- [Serilog](https://serilog.net/) for structured logging
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for API documentation

---

<p align="center">
  Made with ❤️ by Vasileios G. Triantafyllou
</p>

<p align="center">
  <strong>⭐ If you find this project useful, please consider giving it a star!</strong>
</p>
