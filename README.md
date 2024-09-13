# Shared-Watch

https://sharedwatch.azurewebsites.net/

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

#### NuGet Packages:
+ **Google.Apis.Youtube.v3** - access to Youtube API methods
+ **SignalR** - bi-directional, real-time communication

## Running the Application
React application: `npm install` `npm run start` 

ASP.NET Core application: `dotnet run`

TimescaleDB: `docker-compose up` in the directory of the `docker-compose.yml` file
