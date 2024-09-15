import { createBrowserRouter, RouterProvider } from "react-router-dom";
import "./App.css";
import MainView from "./components/views/MainView";
import RoomView from "./components/views/RoomView";
import NotFoundView from "./components/views/NotFoundView";
import { useEffect } from "react";
import JoinRoomView from "./components/views/JoinRoomView";
import { appState, appHub, AppStateContext, AppHubContext } from "./context/AppContext";

function App() {
  useEffect(() => {
    document.body.className = "background-gradient";
  }, []);

  const router = createBrowserRouter([
    {
      path: "/",
      element: <MainView />,
    },
    {
      path: "/room/:id",
      element: <RoomView />,
    },
    {
      path: "/joinRoom/:id",
      element: <JoinRoomView />,
    },
    {
      path: "*",
      element: <NotFoundView />,
    },
  ]);

  return (
    <AppStateContext.Provider value={appState}>
      <AppHubContext.Provider value={appHub}>
        <RouterProvider router={router} />
      </AppHubContext.Provider>
    </AppStateContext.Provider>
  );
}

export default App;
