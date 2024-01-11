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
}

export default new NewsService