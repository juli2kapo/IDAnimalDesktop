# IdAnimal Backend & Web Application

A comprehensive cattle/livestock management system with a backend API and web application to complement the mobile IdAnimal app.

## Architecture

This solution consists of three projects:

1. **IdAnimal.API** - ASP.NET Core Web API backend
2. **IdAnimal.Web** - Blazor Server web application
3. **IdAnimal.Shared** - Shared models and DTOs

## Features

### Backend API ✅ COMPLETED

- **Authentication**
  - JWT-based authentication
  - User registration and login
  - Secure password hashing with BCrypt

- **Establishments Management**
  - CRUD operations for cattle establishments
  - Province and RENSPA tracking
  - Cattle count per establishment

- **Cattle Management**
  - CRUD operations for cattle records
  - Caravan (ID), name, weight, origin, age, gender, GDM tracking
  - Custom data columns support (user-defined fields)
  - Filter by establishment or view all

- **Image & Video Management**
  - Upload snout images (morro) with SIFT descriptors/keypoints
  - Upload full cattle images
  - Caravan images support
  - Video uploads
  - Cloudinary integration for cloud storage

- **Custom Data Columns**
  - User-defined data fields for cattle
  - Dynamic column creation
  - Support for String, Number, Date, Boolean types

### Database ✅ COMPLETED

- SQL Server (LocalDB) with Entity Framework Core
- Fully migrated schema with relationships
- Indexed for performance (Caravan, EstablishmentId, etc.)

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

### Establishments
- `GET /api/establishments` - Get all user establishments
- `GET /api/establishments/{id}` - Get establishment by ID
- `POST /api/establishments` - Create new establishment
- `PUT /api/establishments/{id}` - Update establishment
- `DELETE /api/establishments/{id}` - Delete establishment

### Cattle
- `GET /api/cattle?establishmentId={id}` - Get all cattle (optionally filtered by establishment)
- `GET /api/cattle/{id}` - Get cattle details with images
- `POST /api/cattle` - Create new cattle record
- `PUT /api/cattle/{id}` - Update cattle record
- `DELETE /api/cattle/{id}` - Delete cattle record
- `POST /api/cattle/upload-image` - Upload cattle image (snout/full/caravan)

### Custom Data Columns
- `GET /api/customdatacolumns` - Get all custom columns
- `POST /api/customdatacolumns` - Create new custom column
- `DELETE /api/customdatacolumns/{id}` - Delete custom column

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server (LocalDB comes with Visual Studio)
- Cloudinary account (for image storage)

### Configuration

1. Update `IdAnimal.API/appsettings.json` with your Cloudinary credentials:

```json
{
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

2. Connection string is already configured for LocalDB:
   - `Server=(localdb)\\mssqllocaldb;Database=IdAnimalDb`

### Running the API

```bash
cd IdAnimal.Backend/IdAnimal.API
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

### Testing the API

1. **Register a user**:
```bash
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourPassword123",
  "fullName": "John Doe"
}
```

2. **Login and get JWT token**:
```bash
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourPassword123"
}
```

3. **Use the token** in subsequent requests:
```bash
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

## Web Application (TODO)

The Blazor web application will include:

### Planned Screens
- ✅ Login screen
- ⏳ Establishments listing/management
- ⏳ Cattle listings (by establishment and general view)
- ⏳ Custom data columns configuration
- ⏳ Cattle detail page (view images, edit data)
- ⏳ Establishment detail page
  - Excel/CSV import functionality
  - BoldReports integration for reporting
  - Statistics dashboard

## Project Structure

```
IdAnimal.Backend/
├── IdAnimal.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── EstablishmentsController.cs
│   │   ├── CattleController.cs
│   │   └── CustomDataColumnsController.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── CloudinaryService.cs
│   │   └── Interfaces
│   ├── Migrations/
│   └── Program.cs
├── IdAnimal.Shared/
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Establishment.cs
│   │   ├── Cattle.cs
│   │   ├── CattleImage.cs
│   │   ├── CattleFullImage.cs
│   │   ├── CattleVideo.cs
│   │   ├── CustomDataColumn.cs
│   │   └── CustomDataValue.cs
│   └── DTOs/
│       ├── LoginRequest.cs
│       ├── LoginResponse.cs
│       ├── RegisterRequest.cs
│       ├── EstablishmentDto.cs
│       ├── CattleDto.cs
│       ├── CattleDetailDto.cs
│       └── UploadImageRequest.cs
└── IdAnimal.Web/
    └── (Blazor Server app - to be implemented)
```

## Data Model

### User
- Email (unique)
- Password (hashed)
- Full name
- Created/last login dates

### Establishment
- Name, Province, Postal code, RENSPA
- Registration dates
- Belongs to User

### Cattle
- Caravan (ID), Name, Weight, Origin, Age, Gender, GDM
- Belongs to Establishment
- Has many Images, FullImages, Videos
- Has many CustomDataValues

### CustomDataColumn
- User-defined column name and data type
- Belongs to User

### CustomDataValue
- Stores custom field values for each cattle record
- Links CustomDataColumn to Cattle

## Security

- JWT tokens with 30-day expiration
- Password hashing with BCrypt
- User-scoped data (users can only access their own data)
- CORS enabled for web application integration

## Next Steps

1. ⏳ Implement Blazor Web Application UI
2. ⏳ Add Excel/CSV import functionality
3. ⏳ Integrate BoldReports for report generation
4. ⏳ Add data synchronization with mobile app
5. ⏳ Implement search and filtering
6. ⏳ Add statistics and analytics dashboard

## Mobile App Integration

The mobile app (Akira.Match) can be updated to use this backend by:
1. Replacing local SQLite-only storage with API calls
2. Implementing offline-first sync strategy
3. Uploading images to cloud storage via API
4. Syncing SIFT descriptors for cattle identification

## License

Proprietary - IdAnimal Project
