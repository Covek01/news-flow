
import { Routes, Route, Navigate } from "react-router-dom";
import './App.css';

import Homepage from "./components/homepage/Homepage"
import Login from "./components/login/Login"
import Registration from "./components/homepage/Homepage"



function App() {


    return (
        <>
            <Routes>
                <Route path="/">  
                    <Route index element={<Navigate to="/homepage" replace /> } />
                    <Route path="/homepage" element={<Homepage />} />
                    <Route path="/login" element={<Login />} />
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