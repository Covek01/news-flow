import React from 'react';
import { Box } from '@mui/material';
import NewsBox from './NewsBox'; 
import News from "../../models/News"




interface NewsContainerProps{
    newsList: News[]
}

const NewsContainer: React.FC<NewsContainerProps> = ({newsList}) => {

    return (
        <div>
            <Box display="flex" flexDirection="column" alignItems="center">
            {newsList
            .sort((a, b) => b.id - a.id)
            .map((newsItem, index) => (
                <NewsBox
                    key={index}
                    id={newsItem.id}
                    authorName={newsItem.authorName}
                    title={newsItem.title}
                    imageUrl={newsItem.imageUrl}
                    likesCount={newsItem.likeCount}
                    viewCount={newsItem.viewsCount}
                    datetimePosted={newsItem.postTime}
                    />
            ))}
            </Box>
        </div>
    );
};

export default NewsContainer;
