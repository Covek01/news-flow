
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
import { SignIn } from "./components/SignIn/SignIn";
import NotAuthenticatedGuard from "./components/RouteGuards/NotAuthenticatedGuard";
import WriteNews from "./components/writenews/WriteNews";


function App() {


    return (
        <>
            <AuthStateProvider>
                <Routes>
                    <Route>
                        <Route index element={<Navigate to="/personal" replace />} />
                        <Route path="/newspage/:id" element={<NotAuthenticatedGuard>{<NewsPage />}</NotAuthenticatedGuard> }></Route>
                        <Route path="/homepage" element={<NotAuthenticatedGuard>{<Homepage />}</NotAuthenticatedGuard> }></Route>
                        <Route path="/newest" element={<NotAuthenticatedGuard>{<Newest />}</NotAuthenticatedGuard> }></Route>
                        <Route path="/trending" element={<NotAuthenticatedGuard>{<Trending />}</NotAuthenticatedGuard> }></Route>
                        <Route path="/personal" element={<NotAuthenticatedGuard>{<Personal />}</NotAuthenticatedGuard>}></Route>
                        <Route path="/writenews" element={<NotAuthenticatedGuard>{<WriteNews />}</NotAuthenticatedGuard>}></Route>
                        <Route
                            path="/signin"
                            element={<AuthenticatedGuard>{<SignIn />}</AuthenticatedGuard>}
                        ></Route>
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