import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import './App.css'
import MainView from './components/views/MainView'
import RoomView from './components/views/RoomView';
import NotFoundView from './components/views/NotFoundView';
import { useEffect } from 'react';
import JoinRoomView from './components/views/JoinRoomView';
import { AppStateContext, RoomHubContext, appState, roomHub } from './context/RoomHubContext';

function App() {
  useEffect(() => {
    const backgroundClass = appState.isInRoom.value ? 'background-gradient' : 'background-gradient';

    document.body.className = backgroundClass;

    return () => {
      document.body.className = '';
    };
  }, [appState.isInRoom]);
  
  const router = createBrowserRouter([
    {
      path: "/",
      element: <MainView />
    },
    {
      path: "/room/:id",
      element: <RoomView />
    },
    {
      path: "/joinRoom/:id",
      element: <JoinRoomView />
    },
    {
      path: "*",
      element: <NotFoundView />
    }
  ]);

  return (
    <AppStateContext.Provider value={appState}>
      <RoomHubContext.Provider value={roomHub}>
        <RouterProvider router={router}/>
      </RoomHubContext.Provider>
    </AppStateContext.Provider>
  )
}

export default App
