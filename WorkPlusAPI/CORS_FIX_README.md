# CORS Fix for WorkPlus API

## Problem
The frontend application at `https://workplus.layerbiz.com` was unable to make requests to the API at `https://api.workplus.layerbiz.com` due to CORS (Cross-Origin Resource Sharing) policy violations.

## Error Message
```
Access to XMLHttpRequest at 'https://api.workplus.layerbiz.com/Auth/login' from origin 'https://workplus.layerbiz.com' has been blocked by CORS policy: Response to preflight request doesn't pass access control check: No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

## Solution
Updated the CORS configuration in the backend API to allow requests from the production frontend domain.

## Changes Made

### 1. Program.cs
- Modified the CORS configuration to read allowed origins from configuration files
- Added fallback hardcoded origins in case configuration is missing
- Included both HTTP and HTTPS variants of the production domain

### 2. appsettings.json (Development)
- Added `Cors.AllowedOrigins` configuration section
- Included localhost development URLs

### 3. appsettings.Production.json (Production)
- Added `Cors.AllowedOrigins` configuration section
- Included production domain URLs: `https://workplus.layerbiz.com` and `http://workplus.layerbiz.com`
- Also included localhost URLs for testing

## Allowed Origins
- **Development**: `http://localhost:5173`, `https://localhost:5173`, `http://localhost:3000`, `https://localhost:3000`
- **Production**: `https://workplus.layerbiz.com`, `http://workplus.layerbiz.com`, plus localhost URLs

## Deployment Instructions

### For Development
No additional steps needed. The changes will take effect when the application is restarted.

### For Production
1. Stop the running API application
2. Deploy the updated files:
   - `Program.cs`
   - `appsettings.json`
   - `appsettings.Production.json`
3. Restart the API application

### Verification
After deployment, test the login functionality from the frontend to ensure CORS errors are resolved.

## Technical Details
- The CORS policy is named "AllowReactApp"
- Allows any HTTP method and any headers
- Allows credentials to be sent with requests
- Applied globally to all API endpoints

## Future Modifications
To add new allowed origins, update the `Cors.AllowedOrigins` array in the appropriate `appsettings.json` file and restart the application. 