import React, { useEffect, useState } from 'react';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import { CardHeader, Avatar, IconButton, CardActions, Stack, Button } from '@mui/material';
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
import { useAuthContext } from '../../contexts/auth.context';
import UserService from '../../services/UserService'
import UserWriter from '../../models/UserWriter';
import SubscribeButton from './SubscribeButton';



interface props {}

const NewsPage: React.FC<props> = (
    props
    ) => {
    const { isAuthenticated, signout, user } = useAuthContext();
    const myid =  user? user.id : -1 //ovde se menja nakon auth
    const [isLikedByMe, setIsLikedByMe] = useState<boolean>(false)
    const [likedCount, setLikedCount] = useState<number>(0)
    const [newsInfo, setNewsInfo] = useState<News>(new News())
    const [writerName, setWriterName] = useState<string>('')
    const [subscribedToWriter, setSubscribedToWriter] = useState<boolean>(false)
    const params = useParams()
    const newsId = Number(params.id)

    const likeButtonClicked = async () => {
        if (isLikedByMe){
            const oldCount = likedCount
            setIsLikedByMe(false)
            setLikedCount(likedCount=>likedCount-1)
            const isOkay: boolean = await NewsService.DislikeNews(myid, newsId)
            if (!isOkay){
              setIsLikedByMe(true)
              setLikedCount(oldCount)
            }
          }
          else{
            const oldCount = likedCount
            setIsLikedByMe(true)
            setLikedCount(likedCount=>likedCount+1)
            const isOkay: boolean = await NewsService.LikeNews(myid, newsId)
            if (!isOkay){
              setIsLikedByMe(false)
              setLikedCount(oldCount)
            }
          }


        // const isOkay: boolean = (isLikedByMe)? 
        //     (await NewsService.DislikeNews(myid, newsId)) : (await NewsService.LikeNews(myid, newsId))
  
        // if (isOkay){
        //   await setIsLikedByMe(!isLikedByMe)
        //   setLikedCount((isLikedByMe)? (likedCount - 1):(likedCount + 1))
        // }
    }

    const initializeSubscribeInfo = async() => {
        const {data, isOkay} = await UserService.DoUserFollowWriter(myid, newsInfo.authorId)
        
        if (isOkay){
            setSubscribedToWriter(data)
        }
    }

    const initializeInfo = async () => {
        const info = await NewsService.ClickNews(newsId)
        if (info.length > 0){
            setNewsInfo(info[0])
        }

        setLikedCount(newsInfo.likeCount)

        const user: UserWriter[] = await UserService.GetUserWriterById(info[0].authorId)
        if (user.length > 0){
            setWriterName(user[0].name)
        }
    }

    const initializeLikeCount = async () => {
        const {data, status} = await NewsService.IsNewsLikedByUser(myid, newsId)
        if (status){
            setIsLikedByMe(data)
        }  
        
        

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
                    <Stack direction="row" spacing={8}>
                        <Typography variant="body1" component="div">
                            Author: {writerName}
                        </Typography>
                        {(myid !== newsInfo.authorId)? 
                             <SubscribeButton myid={myid} subscribedId={newsInfo.authorId} isSubscribed={subscribedToWriter}/>
                             :
                             <></>
                        }
                    </Stack>
                </CardContent>

                <CardActions sx={{backgroundColor: theme.palette.primary.main}}>
                    <IconButton aria-label="add to favorites"
                        style={{color: theme.palette.primary.contrastText}}
                        onClick={async e => {
                        likeButtonClicked()
                        }}>
                    {(!isLikedByMe)? <ThumbUpOffAltIcon /> : <ThumbUpIcon />}
                    <Typography style={{color: theme.palette.primary.contrastText, marginRight: '5%',  marginLeft: '15%'}} variant="body2" component="div">
                        {likedCount}
                    </Typography>
                    </IconButton>
                    <Stack direction="row">
                        <VisibilityIcon style={{color: theme.palette.primary.contrastText}}/>
                        <Typography style={{color: theme.palette.primary.contrastText, marginRight: '5%', marginLeft: '15%'}} variant="body2" component="div">
                            {newsInfo.viewsCount}
                        </Typography>
                    </Stack>

                </CardActions>
            </Card>
        </div>
    )
}

export default NewsPage