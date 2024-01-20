import Bar from "../bar/Bar"
import * as React from 'react';
import NewsContainer from "../homepage/NewsContainer";
import News from "../../models/News"
import newsService from "../../services/NewsService"
import { api } from "../../services/Service";
import NewsService from "../../services/NewsService";
import UserService from "../../services/UserService";

const Trending: React.FC = () => {
    const [newsToShow, setNewsToShow] = React.useState<News[]>([])

    const initalizeNews = async () => {
        let news: News[]

        try {
            news = await NewsService.GetTrendingNews();
        }
        catch (error: any) {
            console.log('unexpected error in getting trending news: ', error)
            news = []
        }

        const newsObject: Promise<News>[] = ( news.map(async (x) => {
            const authorName =(await UserService.GetUserWriterById((x as News).authorId))[0].name;
            return new News(
                (x as News).id,
                (x as News).title,
                (x as News).imageUrl,
                authorName,
                (x as News).summary,
                (x as News).text,
                (x as News).authorId,
                (x as News).viewsCount,
                (x as News).likeCount,
                (x as News).postTime
            )
        }))

        const newsRealObject=  await Promise.all(newsObject);
        setNewsToShow( newsRealObject)
    }

    React.useEffect(() => {
        initalizeNews()
    }, [])

    return (
        <div>
            <Bar />
            <NewsContainer newsList={newsToShow} />
            {!newsToShow.length && (
                <div>Unfortunately no news has been clicked on in the recent period :c</div>
            )}
        </div>
    )
}

export default Trending