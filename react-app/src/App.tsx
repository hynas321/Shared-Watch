import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import './App.css'
import MainView from './components/MainView'
import RoomView from './components/RoomView';
import Header from './components/Header';
import NotFoundView from './components/NotFoundView';

function App() {
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

  return (
    <>
      <div className="bg-image background-image">
        <RouterProvider router={router}/>
      </div>
    </>
  )
}

export default App
