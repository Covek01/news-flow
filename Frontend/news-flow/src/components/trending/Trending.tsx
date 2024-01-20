import Bar from "../bar/Bar"
import * as React from 'react';
import NewsContainer from "../homepage/NewsContainer";
import News from "../../models/News"
import newsService from "../../services/NewsService"
import { api } from "../../services/Service";
import NewsService from "../../services/NewsService";

const Trending: React.FC = () => {
    const [newsToShow, setNewsToShow] = React.useState<News[]>([])

    const initalizeNews = async () => {
        let news: News[]

        try{
            news = await NewsService.GetTrendingNews();
        }
        catch(error: any){
            console.log('unexpected error in getting trending news: ', error)
            news = []
        }

        const newsObject: News[] = news.map((x) => {
            return new News(
                    (x as News).id,
                    (x as News).title,
                    (x as News).imageUrl,
                    (x as News).authorName,
                    (x as News).summary,
                    (x as News).text,
                    (x as News).authorId,
                    (x as News).viewsCount,
                    (x as News).likeCount,
                    (x as News).date
                    )
        })
        setNewsToShow(newsObject)     
    }

    React.useEffect(() => {
        initalizeNews()
    }, [])

    return(
        <div>
            <Bar />
            <NewsContainer newsList={newsToShow}/>
            {!newsToShow.length&&(
                <div>Unfortunately no news has been clicked on in the recent period :c</div>
            )}
        </div>
    )
}

export default Trending