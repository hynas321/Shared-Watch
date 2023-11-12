import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import './App.css'
import MainView from './components/MainView'
import RoomView from './components/RoomView';
import Header from './components/Header';
import NotFoundView from './components/NotFoundView';
import { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { updatedUsername } from './redux/slices/userState-slice';

function App() {
  const dispatch = useDispatch();

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
      path: "*",
      element: <NotFoundView />
    }
  ]);

  useEffect(() => {
    const username = localStorage.getItem("username");

    if (!username) {
      return;
    }
  
    dispatch(updatedUsername(username));
  }, []);

  return (
    <div className="bg-image background-image">
      <RouterProvider router={router}/>
    </div>
  )
}

export default App
