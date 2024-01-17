import React, { useEffect, useState } from 'react';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import { CardHeader, Avatar, IconButton, CardActions, Stack } from '@mui/material';
import ThumbUpIcon from '@mui/icons-material/ThumbUp';
import CardMedia from '@mui/material/CardMedia';
import Typography from '@mui/material/Typography';
import FavoriteIcon from '@mui/icons-material/Favorite'
import  ShareIcon from '@mui/icons-material/Share'
import VisibilityIcon from '@mui/icons-material/Visibility';
import theme from '../Theme'
import Bar from '../bar/Bar'
import { Route, useParams } from 'react-router-dom'
import NewsService from '../../services/NewsService';
import News from '../../models/News'
import ThumbUpOffAltIcon from '@mui/icons-material/ThumbUpOffAlt';



interface props {}

const NewsPage: React.FC<props> = (
    props
    ) => {
    const myid = 8 //ovde se menja nakon auth
    const [isLikedByMe, setIsLikedByMe] = useState<boolean>(false)
    const [likedCount, setLikedCount] = useState<number>(0)
    const [newsInfo, setNewsInfo] = useState<News>(new News())
    const params = useParams()
    const newsId = Number(params.id)

    const likeButtonClicked = async () => {
        const isOkay: boolean = (isLikedByMe)? 
            (await NewsService.DisikeNews(myid, newsId)) : (await NewsService.LikeNews(myid, newsId))
  
        if (isOkay){
          await setIsLikedByMe(!isLikedByMe)
          setLikedCount((isLikedByMe)? (likedCount - 1):(likedCount + 1))
        }
    }

    const initializeInfo = async () => {
        const info = await NewsService.GetNewsById(newsId)
        setNewsInfo(info)
        setLikedCount(newsInfo.likeCount)
    }

    const initializeLikeCount = async () => {
        setLikedCount(newsInfo.likeCount)
    }


    useEffect(() => {
        initializeInfo()
    }, [])

    useEffect(() => {
        initializeLikeCount()
    }, [newsInfo])
    
    return(
        <div>
            <Bar />
            <Card style={{backgroundColor: theme.palette.secondary.light,
                  maxWidth: '900px', width: '80%', margin: '1%', marginLeft: 'auto', marginRight: 'auto'}}>
                <Typography variant="h4" component="div">
                    {newsInfo.title}
                </Typography>
                <Typography variant="body2" component="div"
                    style={{marginBottom: '2%'}}>
                    {"Written: 2.2.2023"}
                </Typography>
                <CardMedia
                component="img"
                alt={'NO IMAGE'}
                height="400"
                image={newsInfo.imageUrl}
                title={newsInfo.title}
                style={{ objectFit: 'cover' }}
                />
                <CardContent>
                    <Typography variant="body1" component="div"
                        style={{marginTop: '4%', marginBottom: '5%'}}>
                        {newsInfo.text}
                    </Typography>
                    <Typography variant="body1" component="div">
                        Author: {newsInfo.authorName}
                    </Typography>
                </CardContent>

                <CardActions sx={{backgroundColor: theme.palette.primary.main}}>
                    <IconButton aria-label="add to favorites"
                        style={{color: theme.palette.primary.contrastText}}
                        onClick={async e => {
                        likeButtonClicked()
                        }}>
                    {(!isLikedByMe)? <ThumbUpOffAltIcon /> : <ThumbUpIcon />}
                    <Typography style={{color: theme.palette.primary.contrastText, marginRight: '5%',  marginLeft: '15%'}} variant="body2" component="div">
                        {`${newsInfo.likeCount}`}
                    </Typography>
                    </IconButton>
                    <Stack direction="row">
                    <VisibilityIcon style={{color: theme.palette.primary.contrastText}}/>
                    <Typography style={{color: theme.palette.primary.contrastText, marginRight: '5%', marginLeft: '15%'}} variant="body2" component="div">
                        {`${newsInfo.viewsCount}`}
                    </Typography>
                    </Stack>

                </CardActions>
            </Card>
        </div>
    )
}

export default NewsPage