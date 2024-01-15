import Bar from "../bar/Bar"
import * as React from 'react';
import NewsContainer from "../homepage/NewsContainer";
import News from "../../Types/News"
import newsService from "../../services/NewsService"

const Trending: React.FC = () => {
    const [newsToShow, setNewsToShow] = React.useState<News[]>([])

    const initalizeNews = async () => {
        let news: object[]

        try{
            news = await newsService.GetForYou()
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

export default Trending