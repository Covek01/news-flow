import axios from "axios"
import Service from "./Service"

class TagService extends Service {
    constructor(){
        super()
    }

    public GetAllTags(): object[]{
        return this.axiosInstance.get("/tag/getAllTags")
    }

    public GetMyTags(): object[]{
        return this.axiosInstance.get("/tag/getAllTags")
    }
}

export default new TagService