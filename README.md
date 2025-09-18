# Lunatune API - Modern Music Streaming Backend

A robust, scalable .NET 8 Web API backend for the Lunatune music streaming platform. This repository contains the **complete server-side implementation** that powers a sophisticated music streaming application with advanced features including user authentication, playlist management, song likes, and cloud storage integration.

> **Frontend Application**: This backend API powers a React/Next.js frontend. View the frontend repository at [lunatune-app](https://github.com/TyeStanley/lunatune-app) for the complete client-side implementation.

## 🌐 Live Demo

**🔗 [View Live Application](https://lunatune-app.vercel.app/)**

> **Note**: This is a portfolio showcase project demonstrating full-stack development skills with modern backend technologies.

## 📋 Table of Contents

- [Features](#-features)
- [Technologies Used](#-technologies-used)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Key Highlights](#-key-highlights)
- [Performance & Optimization](#-performance--optimization)
- [Security Features](#-security-features)
- [Portfolio Context](#-portfolio-context)
- [About the Developer](#-about-the-developer)

## ✨ Features

### 🎵 Music Management

- **Song Management**: Complete CRUD operations for music tracks with metadata
- **Advanced Search**: Full-text search across song titles, artists, and albums
- **Popular Songs**: Intelligent sorting by like count and user engagement
- **File Streaming**: Efficient audio file streaming with Azure Blob Storage
- **Metadata Support**: Comprehensive song metadata including genre, album art, and duration

### 🎧 User Experience

- **User Authentication**: Secure Auth0 integration with JWT token validation
- **User Profiles**: Complete user management with profile information
- **Song Likes**: Like/unlike functionality with automatic playlist integration
- **Playlist Management**: Create, edit, and organize custom playlists
- **Library System**: Personal music library with playlist organization
- **Public/Private Playlists**: Flexible playlist visibility controls

### 🔐 Security & Performance

- **JWT Authentication**: Production-ready Auth0 integration with secure token handling
- **Rate Limiting**: Advanced rate limiting with per-user and global limits
- **CORS Configuration**: Secure cross-origin resource sharing
- **User Blocking**: Custom middleware for user access control
- **Environment-based Auth**: Development vs production authentication strategies

### 🏗️ Architecture

- **Entity Framework Core**: Advanced ORM with PostgreSQL integration
- **Database Migrations**: Version-controlled database schema management
- **Docker Support**: Containerized deployment with Docker Compose
- **Cloud Integration**: Azure Blob Storage for scalable file management

## 🛠 Technologies Used

### Backend Framework

- **.NET 8** - Latest .NET framework with modern C# features
- **ASP.NET Core** - High-performance web API framework
- **Entity Framework Core 8** - Advanced ORM with code-first migrations
- **PostgreSQL** - Robust, scalable relational database
- **Npgsql** - High-performance PostgreSQL data provider

### Authentication & Security

- **Auth0** - Enterprise-grade authentication and authorization
- **JWT Bearer Tokens** - Secure token-based authentication
- **Rate Limiting** - Built-in ASP.NET Core rate limiting
- **CORS** - Cross-origin resource sharing configuration
- **Custom Middleware** - User blocking and request processing

### Cloud & Storage

- **Azure Blob Storage** - Scalable cloud file storage
- **SAS Tokens** - Secure, time-limited file access
- **Docker** - Containerized deployment
- **Environment Configuration** - Flexible configuration management

### Development Tools

- **Swagger/OpenAPI** - Interactive API documentation
- **Entity Framework Migrations** - Database version control
- **Docker Compose** - Multi-container orchestration
- **Git** - Version control and collaboration

## 📁 Project Structure

```text
Lunatune/
├── Controllers/              # API Controllers
│   ├── SongsController.cs    # Song management endpoints
│   ├── PlaylistController.cs # Playlist operations
│   └── UsersController.cs    # User management
├── Data/                     # Data Access Layer
│   ├── ApplicationDbContext.cs # EF Core DbContext
│   └── Migrations/           # Database migrations
├── Models/                   # Data Models & DTOs
│   ├── Song.cs              # Song entity
│   ├── User.cs              # User entity
│   ├── Playlist.cs          # Playlist entity
│   └── SongLike.cs          # Like relationship
├── Services/                 # Business Logic Layer
│   ├── SongService.cs       # Song business logic
│   ├── UserService.cs       # User management
│   ├── PlaylistService.cs   # Playlist operations
│   ├── SongLikeService.cs   # Like functionality
│   └── AzureBlobStorageService.cs # File storage
├── Middleware/               # Custom Middleware
│   ├── UserBlockingMiddleware.cs # Access control
│   └── AllowAllAuthenticationHandler.cs # Dev auth
├── Program.cs               # Application entry point
├── appsettings.json         # Configuration
├── Dockerfile              # Container configuration
└── docker-compose.yml      # Multi-container setup
```

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL 14+
- Docker (optional)
- Auth0 account

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/TyeStanley/lunatune-api.git
   cd lunatune-api
   ```

2. **Install dependencies**

   ```bash
   dotnet restore
   ```

3. **Set up environment variables**

   Create a `.env` file in the root directory:

   ```env
   # Database
   ConnectionStrings__DefaultConnection="Host=localhost;Database=lunatune;Username=your_username;Password=your_password"

   # Auth0 (Production)
   Auth0__Domain="your_auth0_domain"
   Auth0__Audience="your_auth0_audience"

   # Azure Storage
   ConnectionStrings__AzureStorage="your_azure_storage_connection_string"
   AzureStorage__ContainerName="your_container_name"

   # Environment
   ASPNETCORE_ENVIRONMENT=Development
   ```

4. **Set up the database**

   ```bash
   # Create and run migrations
   dotnet ef database update
   ```

5. **Run the application**

   ```bash
   # Development
   dotnet run

   # Or with Docker
   docker-compose up
   ```

6. **Access the API**

   - API: [http://localhost:8080](http://localhost:8080)
   - Swagger UI: [http://localhost:8080/swagger](http://localhost:8080/swagger)

## 📚 API Documentation

### Authentication

The API uses different authentication strategies based on the environment:

**Production Mode** (`ASPNETCORE_ENVIRONMENT=Production`):

- Requires valid JWT tokens from Auth0
- All endpoints are protected with JWT Bearer authentication
- Token validation against Auth0 domain and audience

**Development Mode** (`ASPNETCORE_ENVIRONMENT=Development`):

- Uses a dummy authentication scheme that always succeeds
- No actual token validation required
- Allows testing without Auth0 setup

```http
Authorization: Bearer <your_jwt_token>
```

> **Note**: In development, any token (or no token) will be accepted. In production, valid Auth0 JWT tokens are required.

### Core Endpoints

#### Songs

- `GET /api/songs` - Get paginated songs with search
- `GET /api/songs/popular` - Get popular songs by likes
- `GET /api/songs/{id}` - Get specific song details
- `GET /api/songs/{id}/stream` - Stream audio file

#### Playlists

- `GET /api/playlist` - Get user's playlists
- `GET /api/playlist/all` - Get all public playlists
- `POST /api/playlist` - Create new playlist
- `GET /api/playlist/{id}` - Get playlist details
- `POST /api/playlist/{id}/songs/{songId}` - Add song to playlist
- `DELETE /api/playlist/{id}/songs/{songId}` - Remove song from playlist

#### Users

- `GET /api/users/me` - Get current user profile
- `POST /api/users` - Create new user account

#### Song Likes

- `POST /api/songs/{id}/like` - Like a song
- `DELETE /api/songs/{id}/like` - Unlike a song

## 🎯 Key Highlights

### Technical Excellence

- **Modern .NET 8**: Latest framework features with minimal APIs and performance improvements
- **Entity Framework Core**: Advanced ORM with code-first migrations and PostgreSQL
- **JWT Authentication**: Production-ready Auth0 integration with secure token handling
- **Rate Limiting**: Built-in rate limiting with per-user and global limits
- **Cloud Integration**: Azure Blob Storage for scalable file management

### Database Design

- **PostgreSQL**: Robust, scalable relational database with advanced features
- **UUID Primary Keys**: Globally unique identifiers for better scalability
- **Optimized Queries**: Efficient database queries with proper indexing
- **Migration System**: Version-controlled database schema management
- **Relationship Management**: Proper foreign key relationships and cascading

### Security Implementation

- **Auth0 Integration**: Enterprise-grade authentication and authorization
- **JWT Token Validation**: Secure token-based authentication with proper validation
- **Rate Limiting**: Protection against abuse with configurable limits
- **CORS Configuration**: Secure cross-origin resource sharing
- **Environment-based Security**: Different security strategies for dev/prod

### Performance Features

- **Async/Await**: Non-blocking I/O operations throughout the application
- **Entity Framework Optimization**: Efficient queries with proper loading strategies
- **Azure Blob Storage**: Scalable cloud storage with SAS token access
- **Docker Containerization**: Optimized container deployment
- **Connection Pooling**: Efficient database connection management

### Code Quality

- **Dependency Injection**: Proper IoC container usage throughout the application
- **Error Handling**: Comprehensive error handling with proper HTTP status codes
- **Type Safety**: Strong typing with C# nullable reference types

## ⚡ Performance & Optimization

- **.NET 8 Performance**: Latest framework optimizations and minimal APIs
- **Entity Framework Core**: Optimized queries with proper loading strategies
- **PostgreSQL**: High-performance database with advanced indexing
- **Azure Blob Storage**: Scalable cloud storage with efficient streaming
- **Docker Optimization**: Multi-stage builds and optimized container images
- **Connection Pooling**: Efficient database connection management
- **Async Operations**: Non-blocking I/O throughout the application

## 🔒 Security Features

### Authentication & Authorization

- **Auth0 Integration**: Enterprise-grade authentication service
- **JWT Bearer Tokens**: Secure token-based authentication
- **Role-based Access**: User-specific data access controls
- **Token Validation**: Comprehensive JWT token validation

### API Security

- **Rate Limiting**: Protection against abuse and DDoS attacks
- **CORS Configuration**: Secure cross-origin resource sharing
- **Input Validation**: Comprehensive request validation
- **SQL Injection Protection**: Entity Framework parameterized queries

### Data Protection

- **Environment Variables**: Secure configuration management
- **Azure Blob Storage**: Secure cloud file storage with SAS tokens
- **Database Security**: PostgreSQL with proper user permissions
- **HTTPS Enforcement**: SSL/TLS encryption in production

## 🎨 Portfolio Context

### Project Purpose

This project was created as a **comprehensive portfolio showcase** to demonstrate advanced backend development skills and modern .NET technologies. Lunatune API represents a production-ready music streaming backend designed to showcase:

- **Backend Development**: Complete .NET Web API with modern patterns
- **Database Design**: Advanced Entity Framework Core with PostgreSQL
- **Authentication**: Enterprise-grade Auth0 integration
- **Cloud Integration**: Azure Blob Storage for scalable file management
- **API Design**: RESTful API with comprehensive documentation
- **Security**: Production-ready security implementations

### Skills Demonstrated

- **.NET 8 & ASP.NET Core**: Latest framework features and best practices
- **Entity Framework Core**: Advanced ORM with code-first migrations
- **PostgreSQL**: Relational database design and optimization
- **Auth0 Integration**: Enterprise authentication and authorization
- **Azure Cloud Services**: Blob storage and cloud integration
- **Docker**: Containerization and deployment
- **API Design**: RESTful API development with Swagger documentation
- **Security**: JWT authentication, rate limiting, and CORS
- **Performance**: Async programming and database optimization

## 👨‍💻 About the Developer

**Tye Stanley** - Software Developer & UT University Graduate

- **Full-Stack Developer** with expertise in modern web technologies
- **Backend Specialist** in .NET, C#, and API development
- **Database Expert** with PostgreSQL and Entity Framework Core
- **Cloud Integration** experience with Azure services
- **Security Focus** on implementing production-ready authentication
- **Continuous Learner** always exploring new technologies and best practices

## 📞 Contact

- **Portfolio**: [tyestanley.com](https://tyestanley.com/)
- **LinkedIn**: [linkedin.com/in/tyestanley](https://www.linkedin.com/in/tyestanley/)
- **GitHub**: [github.com/TyeStanley](https://github.com/TyeStanley)
- **Email**: Available through contact form on portfolio website

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

---

⭐ **Portfolio Showcase Project** - Demonstrating advanced backend development skills through a comprehensive music streaming API with .NET 8, Entity Framework Core, PostgreSQL, and Azure integration.
