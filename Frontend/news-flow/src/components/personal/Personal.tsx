import Bar from "../bar/Bar"
import * as React from 'react';
import NewsContainer from "../homepage/NewsContainer";
import News from "../../models/News"
import Tag from "../../models/Tag"
import newsService from "../../services/NewsService"
import tagService from "../../services/TagService";
import {Autocomplete, TextField} from "@mui/material"
import { api } from "../../services/Service";

const Personal: React.FC = () => {
    const [newsToShow, setNewsToShow] = React.useState<News[]>([])
    const [myTags, setMyTags] = React.useState<Tag[]>([])
    const initalizeNews = async () => {
        let news:News[];

        try{
            news = await api.get("/GetForYou");
        }
        catch(error: any){
            console.log('unexpected error in getting personal news: ', error)
            news = []
        }

        const newsObject: News[] = news.map((x) => {
            return new News(
                    (x as any).id,
                    (x as any).title,
                    (x as any).url,
                    (x as any).authorName,
                    (x as any).summary,
                    (x as any).text,
                    (x as any).authorId,
                    (x as any).viewsCount,
                    (x as any).likesCount,
                    (x as any).postTime
                    )
        })
        setNewsToShow(newsObject)     
    }


    const getMyTags = async (): Promise<Tag[]> => {
        let tags: Tag[]

        try{
            tags = await api.get("/tag/getAllTags");
        }
        catch(error: any){
            console.log('unexpected error in getting personal tags: ', error)
            tags = []
        }

        const tagsObject: Tag[] = tags.map((x) => {
            return new Tag(
                    (x as any).id,
                    (x as any).name
            )
        })

        return tagsObject
    }

    React.useEffect(() => {
        initalizeNews()
    }, [])

    return(
        <div>
            <Bar />
            <NewsContainer newsList={newsToShow}/>
        </div>
    )
}

export default Personal