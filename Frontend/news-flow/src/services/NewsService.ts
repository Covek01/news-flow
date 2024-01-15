import axios from "axios"
import {api} from "./Service"
import News from "../Types/News"

class NewsService  {
    
    public GetNewestNews(){
        return api.get("/news/getNewestNews")
    }

    public GetTrendingNews(){
        return api.get("/GetTrending2")
    }

    public GetForYou(){
        return api.get("/GetForYou")
    }
}

export default new NewsService