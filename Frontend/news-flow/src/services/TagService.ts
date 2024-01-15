import axios from "axios"
import {api} from "./Service"

class TagService  {

    public GetAllTags(){
        return api.get("/tag/getAllTags")
    }

    public GetMyTags(){
        return api.get("/tag/getAllTags")
    }
}

export default new TagService