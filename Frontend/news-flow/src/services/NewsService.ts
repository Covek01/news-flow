import axios from "axios"
import {api} from "./Service"
import News from "../models/News"
import { error } from "console";

class NewsService  {
    
    public async GetNewestNews(){
        try{
            const {data, status} = await api.get<News[]>("/news/getNewestNews")

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                const errObject: News[] = []
                return errObject;
            } else {
                console.log('unexpected error: ', error);
                const errObject: News[] = []
                return errObject;
            }
        }
        


    }

    public async GetTrendingNews(){
        try{
            const {data, status} = await api.get<News>("/GetTrending2")

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                const errObject: News[] = []
                return errObject;
            } else {
                console.log('unexpected error: ', error);
                const errObject: News[] = []
                return errObject;
            }
        }
    }

    public async GetForYou(){
        try{
            const {data, status} = await api.get<News>("/GetForYou")

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                const errObject: News[] = []
                return errObject;
            } else {
                console.log('unexpected error: ', error);

                const errObject: News[] = []
                return errObject;
            }
        }
    }

    public async GetNewsById(id: number){
        try{
            const {data, status} = await api.get<News>(`/news/getNewsById/${id}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                const errObject: News = new News()
                return errObject;
            } else {
                console.log('unexpected error: ', error);

                const errObject: News = new News()
                return errObject;
            }
        }
    }

    public async LikeNews(userId: number, newsId: number){
        try{
            const {data, status} = await api.put<boolean>(`LikeNewsAndSetLikedRelation/${userId}/${newsId}`)

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

    public async DisikeNews(userId: number, newsId: number){
        try{
            const {data, status} = await api.put<boolean>(`DislikeNews/${userId}/${newsId}`)

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

    public async IsNewsLikedByUser(userId: number, newsId: number){
        try{
            const {data, status} = await api.get<boolean>(`LikeNewsAndSetLikedRelation/${userId}/${newsId}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return {data, status: true}
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                return {data: null, status: false}
            } else {
                console.log('unexpected error: ', error);

                return {data: null, status: false}
            }
        }
    }
}

export default new NewsService