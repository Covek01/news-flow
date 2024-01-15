import axios from "axios"
import Service from "./Service"
import News from "../Types/News"

class NewsService extends Service {
    constructor(){
        super()
    }
    
    public GetNewestNews(): object[]{
        return this.axiosInstance.get("/news/getNewestNews")
    }

    public GetTrendingNews(): object[]{
        return this.axiosInstance.get("/GetTrending2")
    }

    public GetForYou(): object[]{
        return this.axiosInstance.get("/GetForYou")
    }
}

export default new NewsService