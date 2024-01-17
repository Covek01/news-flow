
import { Routes, Route, Navigate } from "react-router-dom";
import './App.css';
import Trending from './components/trending/Trending'
import Homepage from "./components/homepage/Homepage"
import Personal from "./components/personal/Personal";
//mport SignIn from "./components/signin/Signin"
import Registration from "./components/homepage/Homepage"
import Newest from "./components/newest/Newest";
//import { Sign } from "crypto";
import 'crypto-browserify';
import { SignUp } from "./components/SignUp/Signup";
import AuthenticatedGuard from "./components/RouteGuards/AuthenticatedGuard";
import { AuthStateProvider } from "./contexts/auth.context";
import NewsPage from "./components/newspage/NewsPage";



function App() {


    return (
        <>
        <AuthStateProvider>
            <Routes>
                <Route>
                    <Route index element={<Navigate to="/personal" replace />} />
                    <Route path="/newspage/:id" element={<NewsPage />} />
                    <Route path="/homepage" element={<Homepage />} />
                    <Route path="/newest" element={<Newest />} />
                    <Route path="/trending" element={<Trending />} />
                    <Route path="/personal" element={<Personal />} />
                    <Route
                        path="/signup"
                        element={<AuthenticatedGuard>{<SignUp />}</AuthenticatedGuard>}
                    ></Route>
                </Route>
            </Routes>
            </AuthStateProvider>
        </>
    );


}



export default App;