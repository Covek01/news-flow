import Bar from "../bar/Bar"
import * as React from 'react';
import NewsContainer from "../homepage/NewsContainer";
import News from "../../Types/News"
import { RedisClientType, createClient } from "redis"
import { hasOnlyExpressionInitializer } from "typescript";
import newsService from "../../services/NewsService"

interface NewestProps{
    newsList: News[]
}



type ReturnTypeForNews = {
    title: string,
    text: string,
    summary: string,
    imageUrl: string,
    authorId: number,
    locationId: number
}

const Newest: React.FC = () => {
    const [newsToShow, setNewsToShow] = React.useState<News[]>([])
    const [redisClient, setRedisClient] = React.useState(null)

    const channelForNewestNews = 'newestnews'

    let client
    
    const initalizeNews = async () => {
        let news: object[]
        try{
            news = await newsService.GetNewestNews()
        }
        catch(error: any){
            console.log('unexpected error in getting newest news: ', error)
            news = []
        }

        const newsObject: News[] = news.map((x) => {
            return new News((x as any).title,
                    (x as any).url,
                    (x as any).authorName,
                    (x as any).summary,
                    (x as any).text,
                    (x as any).authorId,
                    (x as any).viewsCount,
                    (x as any).likesCount)
        })

        setNewsToShow(newsObject)

        client = await createClient({
            url: 'redis-13049.c304.europe-west1-2.gce.cloud.redislabs.com:13049,password=SqigRXvQ2D42QbxKJnxiTHXaKOeXXrMH'
        })
        .on('error', err => console.log('Redis client error', err))
        .connect()

        client.subscribe(channelForNewestNews, (newsJson) => {
            console.log("New newest new has arrived :D")
            let news = JSON.parse(newsJson) as News

            setNewsToShow([news, ...newsToShow])
        })
        
    }

    React.useEffect(() => {
        initalizeNews()
    }, [])

    return (
        <div>
            <Bar />
            <NewsContainer newsList={newsToShow}/>
        </div>
    );
}

export default Newest;