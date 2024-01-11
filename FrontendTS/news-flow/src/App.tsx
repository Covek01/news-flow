
import { Routes, Route, Navigate } from "react-router-dom";
import './App.css';

import Homepage from "./components/homepage/Homepage"
//mport SignIn from "./components/signin/Signin"
import Registration from "./components/homepage/Homepage"
import Newest from "./components/newest/Newest";
import { Sign } from "crypto";



function App() {


    return (
        <>
            <Routes>
                <Route path="/newesr">  
                    <Route index element={<Navigate to="/registration" replace /> } />
                    <Route path="/homepage" element={<Homepage />} />
                    <Route path="/newest" element={<Newest />} />
                    <Route path="/registration" element={<Registration />} />
                </Route>
            </Routes>
        </>
    );

/*    async function populateWeatherData() {
        const response = await fetch('weatherforecast');
        const data = await response.json();
        setForecasts(data);
    }*/
}



export default App;