import axios from "axios"
import {api} from "./Service"
import Tag from "../models/Tag"

class TagService  {

    public async GetAllTags(){
        try{
            const {data, status} = await api.get("/tag/getAllTags")

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                return error.message;
            } else {
                console.log('unexpected error: ', error);
                return 'An unexpected error occurred';
            }
        }
    }

    public async GetMyTags(){
        try{
            const {data, status} = await api.get("/tag/getMyTags")

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                const ret: Tag[] = []
                return ret; 
            } else {
                console.log('unexpected error: ', error);
                const ret: Tag[] = []
                return ret;
            }
        }
    }

    public async GetTagsByPrefix(prefix: string){
        try{
            const {data, status} = await api.get<Tag[]>(`tag/getTagsByPrefix/${prefix}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                const errObject: Tag[] = []
                return errObject;
            } else {
                console.log('unexpected error: ', error);

                const errObject: Tag[] = []
                return errObject;
            }
        }
    }


    public async GetTagsByName(name: string){
        try{
            const {data, status} = await api.get<Tag[]>(`tag/getByName/${name}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                const errObject: Tag[] = []
                return errObject;
            } else {
                console.log('unexpected error: ', error);

                const errObject: Tag[] = []
                return errObject;
            }
        }
    }

    public async LikeTagsFromNews(id:Number){
        try{
            const {data,status}=await api.put(`tag/LikeTagFromNews/${id}`);

            return data
        }catch(error){
            if(axios.isAxiosError(error)){
                console.log(`error message: `, error.message);

                return;
            }else {
                console.log(`unexpected error: `,error);
                return ;
            }
        }
    }
}

export default new TagService