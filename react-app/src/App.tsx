import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import './App.css'
import MainView from './components/MainView'
import RoomView from './components/RoomView';

function App() {
  const router = createBrowserRouter([
    {
      path: "/",
      element: <MainView />
    },
    {
      path: "/room",
      element: <RoomView />
    }
  ]);

  return (
    <div className="bg-image background-image">
      <RouterProvider router={router}/>
    </div>
  )
}

export default App
