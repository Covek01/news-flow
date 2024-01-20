import axios from "axios"
import {api, upService} from "./Service"
import News from "../models/News"
import { error } from "console";

class NewsService  {
    
    public async GetNewestNews(){
        try{
            const {data, status} = await api.get<News[]>("/news/getNewestNews2")

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
            const {data, status} = await api.get<News[]>("news/GetTrending2")

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
            const {data, status} = await api.get<News[]>("news/GetForYou")

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



    public async GetFilteredNews(tagIds: number[], authorId: number, locationId: number){
        try{
            const filter = {tagIds: tagIds, authorId: authorId, locationId: locationId}
            const {data, status} = await api.post<News[]>("news/getNewestNewsFiltered2", filter)

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

    public async ClickNews(id: number){
        try{
            const {data, status} = await api.get<News[]>(`news/ClickNews2/${id}`)

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

    public async LikeNews(userId: number, newsId: number){
        try{
            const {data, status} = await api.put<boolean>(`news/LikeNewsAndSetLikedRelation/${userId}/${newsId}`)

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

    public async DislikeNews(userId: number, newsId: number){
        try{
            const {data, status} = await api.put<boolean>(`news/DislikeNews/${userId}/${newsId}`)

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
            const {data, status} = await api.get<boolean>(`news/isNewsLikedByUser/${userId}/${newsId}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return {data, status: true}
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                return {data: false, status: false}
            } else {
                console.log('unexpected error: ', error);

                return {data: false, status: false}
            }
        }
    }

    public async CreateNews(news: object){
        try{
            const {data, status} = await upService.post<boolean>(`Uploader/news/CreateNews2/`, news)

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

export default new NewsService