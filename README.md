# Shared-Watch

Hosted, older version of the applicaton: https://sharedwatch.azurewebsites.net/  
Below there are screenshots presenting the current version.

## Description
**Shared Watch** is an online platform created for collaborative video watching sessions in virtual rooms.

The application allows users to set a username upon opening, granting access to functionalities including room browsing, filtering, joining existing rooms, and creating new ones.

User access to functionalities is username-dependent, with the username state persisting across sessions. Users can join public or private rooms, with conditions for joining such as unique usernames and room availability. Private room access requires password authorization.

Upon room creation, users are directed to the room view, where they can chat, manage a video playlist, view user lists, and access room settings. Roles include user and administrator, with administrators automatically assigned to room hosts Administrators can manage permissions, set passwords, assign roles, and remove users.

Chat messages include sender usernames and timestamps, while playlist videos feature titles and thumbnails. The video player synchronizes playback across users, and room state updates are real-time. New users inherit the room's current state, with inaccessible elements hidden or disabled.

Rooms are deleted upon all users leaving.

## Configuration
Set the Google YouTube API key through `YouTubeApi:ApiKey` to add YouTube videos. Uploaded MP4, WebM, and Ogg files do not use the YouTube API.

Video uploads are limited to 500 MB by default. `VideoStorage:Path`, `VideoStorage:MaxFileSizeBytes`, and `VideoStorage:MaxDurationSeconds` are bound through typed options; configure them with matching `VideoStorage__...` environment variables in deployment. Set the path to persistent storage, and mount the same shared directory for every backend replica so uploads can be served by any instance. The API streams files with byte-range support for seeking.

Room membership is renewed in Redis while clients are connected. If an API instance stops without a graceful disconnect, an active instance removes the stale room after the presence lease and cleanup grace period expire. Uploaded files no longer referenced by any playlist are reclaimed after a one-hour safety period; normal room closure deletes its uploads immediately.

Set `GlobalAdminToken` through the environment for the protected `api/room/getalldetails` endpoint. The endpoint stays unavailable when this value is not configured. Do not commit the token to the repository.

Room passwords are stored as hashes. Existing plaintext room passwords are upgraded to hashes the next time a user successfully joins those rooms.

Run PostgreSQL/TimescaleDB and Redis for local development with `docker compose up`. Every backend replica must use the same PostgreSQL database and Redis endpoint. Redis carries live playback state, active SignalR connection tracking, and the SignalR backplane; the backend does not keep these as process-local state. Locally Redis is published on port `6380` and PostgreSQL on port `5435` to avoid colliding with common local service ports.

For deployment, configure `ConnectionStrings__Redis`, `ConnectionStrings__DefaultConnection`, and the same `Jwt__Key`/`Jwt__Issuer` values on every replica through the environment. Redis should use private networking and authentication/TLS appropriate to the hosting platform.

## Technological Specification

### Frontend
+ **React** with **Typescript** (built with Vite)
#### NPM Packages:
+ **Toastify** - toast notifications
+ **React Player** - video player integration
+ **React Router Dom** - navigation of the React application
+ **Axios** - HTTP requests and responses
+ **Bootstrap** - styling and responsive design
+ **Preact Signals** - state management
+ **SignalR** - bi-directional, real-time communication

### Backend
+ **ASP.NET Core** (REST API)
+ **TimescaleDB**

#### NuGet Packages:
+ **Google.Apis.Youtube.v3** - access to Youtube API methods
+ **SignalR** - bi-directional, real-time communication

## Running the Application
React application:
- `cd ReactApp`
- `npm ci`
- `npm run dev`

ASP.NET Core application:
- `dotnet run --project WebApi/WebApi.csproj`

TimescaleDB:
- `docker-compose up` in the directory of the `docker-compose.yml` file


## User interface

User interface supports devices with various resolutions due to the responsive design.

### Main View

![image](https://github.com/user-attachments/assets/313d642a-46bf-4da2-a103-5cef7ea47e7c)

<img src="https://github.com/user-attachments/assets/346ecb3b-75a7-4f71-84eb-92bb3d95e9e1" width="49%" />
<img src="https://github.com/user-attachments/assets/2516b3f0-e132-4bac-9b47-c5f27f775139" width="49%" />

### Room View

![image](https://github.com/user-attachments/assets/a5f65f81-4da7-49e4-ae29-d46297e3b0a9)

### Room Control Panel

<img src="https://github.com/user-attachments/assets/75622dd7-f8d6-43a6-b44b-bbac3dbe5b47" width="49%" />
<img src="https://github.com/user-attachments/assets/ab5f76b4-4834-479f-a131-707025ab9322" width="49%" />

<img src="https://github.com/user-attachments/assets/d25737ac-b48d-45fd-90db-ee5bc8a870d2" width="49%" />
<img src="https://github.com/user-attachments/assets/87ac48a5-8dac-46a5-bc4c-5e88a2578828" width="49%" />

### Join Room View

![image](https://github.com/user-attachments/assets/f41c0dc5-c483-4848-8c0f-be60d03b3eaa)

### Other

Sample toast notification


![image](https://github.com/user-attachments/assets/9cde7827-7c2f-4e69-aa1c-aaf3fe39ca71)




