# HomeControllerHUB API Documentation for Frontend Developers

## Overview

HomeControllerHUB is a .NET Core backend application built with Domain Driven Design (DDD) architecture and CQRS (Command Query Responsibility Segregation) pattern. The system allows for management of IoT sensors, establishments, users, and sensor data collection. This document serves as a comprehensive guide for frontend developers to integrate with the HomeControllerHUB API.

## Authentication

The API uses JWT (JSON Web Token) authentication. To access protected endpoints, the client must:

1. Obtain an access token via the `/api/v1/users/token` endpoint
2. Include the token in the Authorization header of subsequent requests: `Authorization: Bearer {token}`
3. Refresh the token when it expires using the `/api/v1/users/refresh-token` endpoint

### Token Lifecycle

- Access tokens have a limited lifespan (typically 1 hour)
- Refresh tokens can be used to obtain new access tokens without re-authentication
- All tokens are invalidated when a user changes their password or is deactivated

## API Endpoints

All endpoints follow the pattern `/api/v1/{controller}/{action}`. The API uses standard HTTP status codes and returns consistent response formats.

### Common Response Formats

- Successful responses: Data in JSON format
- Error responses: Problem Details object following RFC 7807
- Paginated responses: Consistent format with `items`, `totalCount`, `pageNumber`, `totalPages`, `hasPreviousPage`, `hasNextPage`

### 1. User Management

#### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/v1/users/token` | Authenticate and generate access token | No |
| POST | `/api/v1/users/refresh-token` | Refresh an expired access token | No |

**Token Request Example:**
```json
{
  "login": "username",
  "password": "userpassword"
}
```

**Token Response Example:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2023-05-21T15:15:53Z",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

#### User Operations

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/v1/users` | Create a new user | Yes |
| PUT | `/api/v1/users/{id}` | Update user information | Yes |
| DELETE | `/api/v1/users/{id}` | Delete a user | Yes |
| GET | `/api/v1/users/current` | Get current user information | Yes |
| GET | `/api/v1/users/list` | Get list of users (optional search) | Yes |
| GET | `/api/v1/users/confirm-email` | Confirm user email | No |
| POST | `/api/v1/users/forgot-password` | Initiate password reset | No |
| POST | `/api/v1/users/reset-password-with-token` | Reset password with token | No |
| POST | `/api/v1/users/reset-password` | Reset password (for authenticated users) | Yes |

### 2. Establishment Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/v1/establishment` | Create a new establishment | Yes |
| PUT | `/api/v1/establishment` | Update an establishment | Yes |
| DELETE | `/api/v1/establishment` | Delete an establishment | Yes |
| GET | `/api/v1/establishment/{id}` | Get establishment by ID | Yes |
| GET | `/api/v1/establishment` | Get paginated list of establishments | Yes |
| GET | `/api/v1/establishment/list` | Get complete list of establishments | Yes |

### 3. Profile Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/v1/profiles` | Create a new profile | Yes |
| PUT | `/api/v1/profiles` | Update a profile | Yes |
| DELETE | `/api/v1/profiles` | Delete a profile | Yes |
| GET | `/api/v1/profiles/{id}` | Get profile by ID | Yes |
| GET | `/api/v1/profiles` | Get paginated list of profiles | Yes |
| GET | `/api/v1/profiles/list` | Get complete list of profiles | Yes |

### 4. Location Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/v1/locations` | Create a new location | Yes |
| PUT | `/api/v1/locations` | Update a location | Yes |
| DELETE | `/api/v1/locations` | Delete a location | Yes |
| GET | `/api/v1/locations/{id}` | Get location by ID | Yes |
| GET | `/api/v1/locations` | Get paginated list of locations | Yes |
| GET | `/api/v1/locations/list` | Get complete list of locations | Yes |

### 5. Sensor Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/v1/sensors` | Register a new sensor | Yes |
| PUT | `/api/v1/sensors` | Update sensor configuration | Yes |
| DELETE | `/api/v1/sensors` | Remove a sensor | Yes |
| GET | `/api/v1/sensors/{id}` | Get sensor details by ID | Yes |
| GET | `/api/v1/sensors` | Get paginated list of sensors | Yes |
| GET | `/api/v1/sensors/list` | Get complete list of sensors | Yes |
| GET | `/api/v1/sensors/{id}/readings` | Get historical readings from sensor | Yes |
| GET | `/api/v1/sensors/{id}/alerts` | Get alerts generated by sensor | Yes |

### 6. Sensor Data Operations

These endpoints are typically used by IoT devices to submit data, but the frontend might need to know about them:

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/v1/sensordata/readings` | Submit a single sensor reading | API Key* |
| POST | `/api/v1/sensordata/readings/batch` | Submit multiple sensor readings at once | API Key* |
| POST | `/api/v1/sensordata/status` | Update sensor status information | API Key* |

*Note: These endpoints use API keys for authorization instead of JWT tokens. Each sensor has its own API key.

## Data Models

### User Model

```json
{
  "id": "guid",
  "name": "string",
  "email": "string",
  "document": "string",
  "login": "string",
  "isActive": true,
  "profileId": "guid",
  "profileName": "string",
  "establishmentId": "guid",
  "establishmentName": "string"
}
```

### Establishment Model

```json
{
  "id": "guid",
  "code": "string",
  "name": "string",
  "document": "string",
  "responsible": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "isMaster": true,
  "subscriptionPlanId": "guid",
  "subscriptionEndDate": "2023-05-21T15:15:53Z"
}
```

### Sensor Model

```json
{
  "id": "guid",
  "establishmentId": "guid",
  "establishmentName": "string",
  "locationId": "guid",
  "locationName": "string",
  "name": "string",
  "deviceId": "string",
  "type": "integer", // enum: (0=Temperature, 1=Humidity, 2=Pressure, etc.)
  "model": "string",
  "firmwareVersion": "string",
  "apiKey": "string",
  "minThreshold": "number",
  "maxThreshold": "number",
  "isActive": true,
  "lastCommunication": "2023-05-21T15:15:53Z",
  "batteryLevel": "number"
}
```

### Location Model

```json
{
  "id": "guid",
  "establishmentId": "guid",
  "establishmentName": "string",
  "name": "string",
  "description": "string",
  "type": "integer", // enum: (0=Building, 1=Floor, 2=Room, etc.)
  "parentLocationId": "guid",
  "parentLocationName": "string"
}
```

### Sensor Reading Model

```json
{
  "id": "guid",
  "sensorId": "guid",
  "sensorName": "string",
  "timestamp": "2023-05-21T15:15:53Z",
  "value": "number",
  "unit": "string"
}
```

### Sensor Alert Model

```json
{
  "id": "guid",
  "sensorId": "guid",
  "sensorName": "string",
  "type": "integer", // enum: (0=ThresholdExceeded, 1=ThresholdBelowMinimum, etc.)
  "message": "string",
  "timestamp": "2023-05-21T15:15:53Z",
  "isAcknowledged": true,
  "acknowledgedAt": "2023-05-21T15:15:53Z",
  "acknowledgedById": "guid",
  "acknowledgedByName": "string"
}
```

## Pagination Parameters

For endpoints that return paginated results, you can use the following query parameters:

- `pageNumber`: Page number (1-based, defaults to 1)
- `pageSize`: Number of items per page (defaults to 10)
- `searchBy`: Search term to filter results
- `orderBy`: Field to order by (defaults to "Id")
- `orderDirection`: Direction to order (ASC or DESC, defaults to ASC)

Example: `/api/v1/sensors?pageNumber=2&pageSize=20&searchBy=temperature&orderBy=Name&orderDirection=DESC`

## Error Handling

The API returns standard HTTP status codes:

- 200 OK: Request successful
- 201 Created: Resource successfully created
- 204 No Content: Request successful, no content to return
- 400 Bad Request: Validation error or malformed request
- 401 Unauthorized: Missing or invalid authentication
- 403 Forbidden: Authenticated but not authorized for the operation
- 404 Not Found: Resource not found
- 500 Internal Server Error: Server-side error

Error responses follow the Problem Details format (RFC 7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-7c5d9d7f9fe546378d366886f9e262be-64ec1d764ec58d24-00",
  "errors": {
    "Name": ["The Name field is required."]
  }
}
```

## Implementation Examples

### Authentication Flow

```javascript
// Login
async function login(username, password) {
  const response = await fetch('/api/v1/users/token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      login: username,
      password: password
    })
  });
  
  if (!response.ok) {
    throw new Error('Authentication failed');
  }
  
  const data = await response.json();
  // Store token in localStorage or secure cookie
  localStorage.setItem('accessToken', data.token);
  localStorage.setItem('refreshToken', data.refreshToken);
  localStorage.setItem('tokenExpiration', data.expiration);
  
  return data;
}

// Add authentication header to requests
function getAuthHeaders() {
  const token = localStorage.getItem('accessToken');
  return {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  };
}

// Refresh token
async function refreshToken() {
  const refreshToken = localStorage.getItem('refreshToken');
  
  const response = await fetch('/api/v1/users/refresh-token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      refreshToken: refreshToken
    })
  });
  
  if (!response.ok) {
    // Redirect to login page
    return false;
  }
  
  const data = await response.json();
  localStorage.setItem('accessToken', data.token);
  localStorage.setItem('refreshToken', data.refreshToken);
  localStorage.setItem('tokenExpiration', data.expiration);
  
  return true;
}
```

### Fetching Sensor Data

```javascript
// Get paginated list of sensors
async function getSensors(pageNumber = 1, pageSize = 10, searchTerm = '') {
  const url = `/api/v1/sensors?pageNumber=${pageNumber}&pageSize=${pageSize}${searchTerm ? `&searchBy=${searchTerm}` : ''}`;
  
  const response = await fetch(url, {
    method: 'GET',
    headers: getAuthHeaders()
  });
  
  if (!response.ok) {
    throw new Error('Failed to fetch sensors');
  }
  
  return await response.json();
}

// Get sensor readings
async function getSensorReadings(sensorId, startDate, endDate, pageNumber = 1, pageSize = 50) {
  const startDateParam = startDate ? `&startDate=${startDate.toISOString()}` : '';
  const endDateParam = endDate ? `&endDate=${endDate.toISOString()}` : '';
  
  const url = `/api/v1/sensors/${sensorId}/readings?pageNumber=${pageNumber}&pageSize=${pageSize}${startDateParam}${endDateParam}`;
  
  const response = await fetch(url, {
    method: 'GET',
    headers: getAuthHeaders()
  });
  
  if (!response.ok) {
    throw new Error('Failed to fetch sensor readings');
  }
  
  return await response.json();
}
```

## Conclusion

This documentation provides a comprehensive overview of the HomeControllerHUB API. Frontend developers should use this as a reference for integrating with the backend services. For any further details or specific requirements, please check the API documentation in Swagger UI (available at `/swagger` when running the application in development mode).
