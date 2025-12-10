# Getting Started with IdAnimal Application

## 🎉 What's Been Built

Congratulations! You now have a fully functional backend API and a partially complete web application for cattle/livestock management.

### ✅ Completed (Ready to Use)

1. **Backend API** - 100% Complete
   - Full REST API with Swagger documentation
   - JWT-based authentication
   - SQL Server database with migrations
   - All CRUD endpoints for Establishments, Cattle, Custom Columns
   - Image upload support (Cloudinary integration)
   - User-scoped data security

2. **Web Application Infrastructure** - 100% Complete
   - HTTP client services
   - Authentication state management
   - Protected routes
   - Service layer complete

3. **Web UI Pages** - 70% Complete
   - ✅ Login page
   - ✅ Register page
   - ✅ Dashboard/Home page with statistics
   - ✅ Establishments page (full CRUD)
   - ⏳ Cattle listing page (needs implementation)
   - ⏳ Cattle detail page (needs implementation)
   - ⏳ Custom columns page (needs implementation)
   - ⏳ Establishment detail with Excel import (needs implementation)

## 🚀 How to Run the Application

### Step 1: Start the Backend API

Open a terminal and run:

```bash
cd C:\REPOS\IDANIMAL\IdAnimal.Backend\IdAnimal.API
dotnet run
```

You should see:
```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
```

The API is now running! You can access:
- **Swagger UI**: https://localhost:5001/swagger
- **API Base**: https://localhost:5001/api

### Step 2: Start the Web Application

Open a **new terminal** (keep the API running) and run:

```bash
cd C:\REPOS\IDANIMAL\IdAnimal.Backend\IdAnimal.Web
dotnet run
```

You should see something like:
```
Now listening on: https://localhost:5002
```

### Step 3: Open Your Browser

Navigate to: **https://localhost:5002** (or whatever port is shown)

## 📝 First Time Usage

### 1. Register a New Account

1. Click on "Regístrate aquí" on the login page
2. Fill in:
   - **Nombre Completo**: Your Name
   - **Email**: your@email.com
   - **Contraseña**: YourPassword123
3. Click "Registrarse"
4. You'll be automatically logged in and taken to the dashboard

### 2. Explore the Dashboard

You'll see:
- Number of establishments: 0
- Number of cattle: 0
- Average per establishment: 0

### 3. Create Your First Establishment

1. Click "Nuevo Establecimiento" or navigate to "Establecimientos" in the menu
2. Click "Nuevo Establecimiento" button
3. Fill in the form:
   - **Nombre**: El Ombú
   - **Provincia**: Buenos Aires
   - **Código Postal**: 1234
   - **RENSPA**: 12-345-67
4. Click "Guardar"

### 4. Test CRUD Operations

- **Edit**: Click the pencil icon next to an establishment
- **Delete**: Click the trash icon (will warn if it has cattle)
- **View**: Click on the establishment name

## 🔧 Configuration

### Backend API Configuration

Edit: `IdAnimal.Backend/IdAnimal.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=IdAnimalDb;Trusted_Connection=true"
  },
  "Jwt": {
    "Key": "YourSecretKeyHere",
    "Issuer": "IdAnimalAPI",
    "Audience": "IdAnimalClients"
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUD_NAME",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  }
}
```

**Important**: Update Cloudinary credentials for image uploads to work.

### Web App Configuration

Edit: `IdAnimal.Backend/IdAnimal.Web/appsettings.json`

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:5001"
  }
}
```

Make sure this matches your API URL.

## 🧪 Testing the API with Swagger

1. Go to https://localhost:5001/swagger
2. Click on "POST /api/auth/register"
3. Click "Try it out"
4. Enter test data:
```json
{
  "email": "test@example.com",
  "password": "Test123!",
  "fullName": "Test User"
}
```
5. Click "Execute"
6. You should get a 200 response with a JWT token

## 📚 API Endpoints Reference

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login

### Establishments
- `GET /api/establishments` - Get all user establishments
- `GET /api/establishments/{id}` - Get by ID
- `POST /api/establishments` - Create new
- `PUT /api/establishments/{id}` - Update
- `DELETE /api/establishments/{id}` - Delete

### Cattle
- `GET /api/cattle` - Get all cattle
- `GET /api/cattle?establishmentId={id}` - Filter by establishment
- `GET /api/cattle/{id}` - Get cattle detail
- `POST /api/cattle` - Create new cattle
- `PUT /api/cattle/{id}` - Update cattle
- `DELETE /api/cattle/{id}` - Delete cattle
- `POST /api/cattle/upload-image` - Upload image

### Custom Data Columns
- `GET /api/customdatacolumns` - Get all columns
- `POST /api/customdatacolumns` - Create column
- `DELETE /api/customdatacolumns/{id}` - Delete column

## 🛠️ Implementing Remaining Pages

See `WEB_APPLICATION_STATUS.md` for detailed instructions on implementing:
- Cattle listing page
- Cattle detail page
- Custom columns page
- Establishment detail with Excel import

**Pattern to follow**: Check `Establishments.razor` as a complete reference implementation.

## ❗ Troubleshooting

### API won't start
- **Error**: Port already in use
  - **Solution**: Change port in `Properties/launchSettings.json` or stop the other process

### Database errors
- **Error**: Cannot open database
  - **Solution**: Run migrations again: `dotnet ef database update`

### Web app can't connect to API
- **Error**: CORS or connection refused
  - **Solution**: Check `ApiSettings:BaseUrl` in web app `appsettings.json` matches API URL
  - **Solution**: Ensure API is running first

### Authentication not working
- **Error**: Redirects to login immediately
  - **Solution**: Check browser console for errors
  - **Solution**: Clear browser local storage (F12 → Application → Local Storage → Clear)

### Images not uploading
- **Error**: Upload failed
  - **Solution**: Configure Cloudinary credentials in `appsettings.json`

## 🎯 Next Steps

1. **Test what's built** - Register, login, create establishments
2. **Implement remaining pages** - Follow patterns in existing pages
3. **Add Excel/CSV import** - Use ClosedXML package
4. **Integrate BoldReports** - For PDF report generation
5. **Connect mobile app** - Update Akira.Match to use API
6. **Deploy to production** - Azure, AWS, or your preferred host

## 📖 Additional Resources

- **Backend README**: `README.md`
- **Web Status**: `WEB_APPLICATION_STATUS.md`
- **Swagger Documentation**: https://localhost:5001/swagger (when running)

## 💡 Tips

1. Keep both API and Web app running in separate terminals
2. Use Swagger to test API endpoints before implementing UI
3. Check browser console (F12) for errors
4. Copy patterns from existing pages (especially Establishments.razor)
5. Use the services layer - don't call API directly from pages

## 🆘 Need Help?

- Check the API is running: https://localhost:5001/swagger
- Check browser console for JavaScript errors (F12)
- Verify authentication by checking the "Application" tab in browser dev tools
- Look at existing working pages as reference

---

**Happy Coding!** 🐄✨
