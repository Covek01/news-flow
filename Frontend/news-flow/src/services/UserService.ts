import axios from "axios"
import { api } from "./Service"
import { error } from "console";
import User from "../models/User";
import UserWriter from "../models/UserWriter";

class UserService {
    public async SubscribeTo(userIds: number[]) {
        try {
            const { data, status } = await api.put<number>(`user/SubscribeTo`, userIds)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);

                return -1
            } else {
                console.log('unexpected error: ', error);

                return -1
            }
        }
    }

    public async UnsubscribeFrom(userIds: number[]) {
        try {
            const { data, status } = await api.put<number>(`user/UnsubscribeFrom2`, userIds)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);

                return -1
            } else {
                console.log('unexpected error: ', error);

                return -1
            }
        }
    }

    public async GetUserWriterById(id: number) {
        try {
            const { data, status } = await api.get<UserWriter[]>(`user/GetUserById/${id}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);

                const data: UserWriter[] = []
                return data;
            } else {
                console.log('unexpected error: ', error);

                const data: UserWriter[] = []
                return data;
            }
        }
    }

    public async GetWritersByPrefix(prefix: string) {
        try {
            const { data, status } = await api.get<UserWriter[]>(`user/getWritersByPrefix/${prefix}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);

                const data: UserWriter[] = []
                return data;
            } else {
                console.log('unexpected error: ', error);

                const data: UserWriter[] = []
                return data;
            }
        }
    }

    public async GetWriterByName(name: string) {
        try {
            const { data, status } = await api.get<UserWriter[]>(`user/getWriterByName/${name}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);

                const data: UserWriter[] = []
                return data;
            } else {
                console.log('unexpected error: ', error);

                const data: UserWriter[] = []
                return data;
            }
        }
    }

    public async DoUserFollowWriter(userId: number, writerId: number){
        try{
            const {data, status} = await api.get<boolean>(`user/DoUserFollowWriter/${userId}/${writerId}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status for user following is: ', status);

            return {data, isOkay: true}
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                                
                const data = false
                return {data, isOkay: false}
            } else {
                console.log('unexpected error: ', error);

                const data = false
                return {data, isOkay: false}
            }
        }
    }

    public async DoIfollowHim(writerId: number){
        try{
            const {data, status} = await api.get<boolean>(`user/DoIFollowHim/${writerId}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status for user following is: ', status);

            return {data, isOkay: true}
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                                
                const data = false
                return {data, isOkay: false}
            } else {
                console.log('unexpected error: ', error);

                const data = false
                return {data, isOkay: false}
            }
        }
    }


    public async GetNumberOfActiveUsers() {
        try {
            const { data, status } = await api.get<Number>(`user/authcount`)
 
            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);
 
            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
 
                const data: Number = 0
                return data;
            } else {
                console.log('unexpected error: ', error);
 
                const data: Number = 0
                return data;
            }
        }
    }

}

export default new UserService