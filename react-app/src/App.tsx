import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import './App.css'
import MainView from './components/MainView'
import RoomView from './components/RoomView';
import NotFoundView from './components/NotFoundView';
import { useAppSelector } from './redux/hooks';
import { useEffect } from 'react';
import JoinRoomView from './components/JoinRoomView';

function App() {
  const userState = useAppSelector((state) => state.userState);

  useEffect(() => {
    const backgroundClass = userState.isInRoom ? 'background-gradient' : 'background-gradient';

    document.body.className = backgroundClass;

    return () => {
      document.body.className = '';
    };
  }, [userState.isInRoom]);
  
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
    <RouterProvider router={router}/>
  )
}

export default App
