import axios from "axios"
import {api} from "./Service"
import { error } from "console";
import User from "../models/User";

class UserService {
    public async SubscribeTo(userIds: number[]){
        try{
            const {data, status} = await api.put<boolean>(`user/SubscribeTo`, userIds)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return true
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                return false;
            } else {
                console.log('unexpected error: ', error);

                return false;
            }
        }
    }

    public async UnsubscribeFrom(userIds: number[]){
        try{
            const {data, status} = await api.put<boolean>(`user/UnsubscribeFrom`, userIds)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return true
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                return false;
            } else {
                console.log('unexpected error: ', error);

                return false;
            }
        }
    }
}