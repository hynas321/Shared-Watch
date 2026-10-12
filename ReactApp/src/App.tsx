import { lazy, Suspense, useEffect } from "react";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import "./App.css";
import NotFoundView from "./components/views/NotFoundView";
import { appState, appHub, AppStateContext, AppHubContext } from "./context/AppContext";
import { applyBackgroundTheme, getBackgroundTheme } from "./classes/helpers/BackgroundTheme";

const MainView = lazy(() => import("./components/views/MainView"));
const RoomView = lazy(() => import("./components/views/RoomView"));
const JoinRoomView = lazy(() => import("./components/views/JoinRoomView"));

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

function App() {
  useEffect(() => {
    applyBackgroundTheme(getBackgroundTheme());
  }, []);

  return (
    <AppStateContext.Provider value={appState}>
      <AppHubContext.Provider value={appHub}>
      <Suspense fallback={<div className="app-loading" role="status">Loading Shared Watch…</div>}>
        <RouterProvider router={router} />
      </Suspense>
      </AppHubContext.Provider>
    </AppStateContext.Provider>
  );
}

export default App;
