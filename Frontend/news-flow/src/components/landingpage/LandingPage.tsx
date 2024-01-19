import React from "react";
import Bar from "../bar/Bar";
import UserService from "../../services/UserService";

const LandingPage: React.FC = () => {
    const [userCount,setUserCount]=React.useState<Number>(0);
    const initializeUserCount=async()=>{
        let numUsers:Number;
        try{
            numUsers=await UserService.GetNumberOfActiveUsers();
        }
        catch(error:any){
            console.log('unexpected error in getting number of active users', error)
            numUsers=0;
        }
        setUserCount(numUsers);
    }

    React.useEffect(()=>{
        initializeUserCount()
    },[])
    return (
        <div className="h-screen flex relative">
            <Bar />
            <div
                className="bg-cover w-screen bg-center flex items-center justify-center relative"
                style={{ backgroundImage: "url('/Landing.jpg')" }}
            >


                    <div className="text-black text-center relative  left-0 w-full  py-8 ">
                        <h1 className="text-6xl text-gray-850 bg-white bg-opacity-50  font-bold pt-4 pb-4 mb-0  z-10 text-opacity-100">Welcome to NewsFlow</h1>
                        <p className="text-4xl text-gray-850 bg-white bg-opacity-50  font-bold pb-4  z-10 text-opacity-100">Join more than {`${userCount}`} currently active users!</p>
                    </div>
            </div>
        </div>
    );
};

export default LandingPage;
