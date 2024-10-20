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
The working Google Youtube API key is needed in the _dotnet-server/appsettings.Development.json_ file for the playlist and video player to work.

The key needs to be set in _the dotnet-server/appsettings.json_ file if the application is to be deployed.

Docker is required to run the TimescaleDB.

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
- `npm install`
- `npm run start` 

ASP.NET Core application:
- `dotnet run`

TimescaleDB:
- `docker-compose up` in the directory of the `docker-compose.yml` file


## User interface

User interface supports devices with various resolutions due to the responsive design.

### Main View

![image](https://github.com/user-attachments/assets/313d642a-46bf-4da2-a103-5cef7ea47e7c)

<img src="https://github.com/user-attachments/assets/2516b3f0-e132-4bac-9b47-c5f27f775139" width="49%" />
<img src="https://github.com/user-attachments/assets/346ecb3b-75a7-4f71-84eb-92bb3d95e9e1" width="49%" />

### Room View

![image](https://github.com/user-attachments/assets/a5f65f81-4da7-49e4-ae29-d46297e3b0a9)

### Room Control Panel
<img src="https://github.com/user-attachments/assets/ab5f76b4-4834-479f-a131-707025ab9322" width="49%" />
<img src="https://github.com/user-attachments/assets/75622dd7-f8d6-43a6-b44b-bbac3dbe5b47" width="49%" />


<img src="https://github.com/user-attachments/assets/d25737ac-b48d-45fd-90db-ee5bc8a870d2" width="49%" />


<img src="https://github.com/user-attachments/assets/87ac48a5-8dac-46a5-bc4c-5e88a2578828" width="49%" />

### Join Room View
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
The working Google Youtube API key is needed in the _dotnet-server/appsettings.Development.json_ file for the playlist and video player to work.

The key needs to be set in _the dotnet-server/appsettings.json_ file if the application is to be deployed.

Docker is required to run the TimescaleDB.

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
- `npm install`
- `npm run start` 

ASP.NET Core application:
- `dotnet run`

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













