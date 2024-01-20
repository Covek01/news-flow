import React from 'react';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import { CardHeader, Avatar, IconButton, CardActions, Stack } from '@mui/material'
import ThumbUpIcon from '@mui/icons-material/ThumbUp';
import ThumbUpOffAltIcon from '@mui/icons-material/ThumbUpOffAlt';
import CardMedia from '@mui/material/CardMedia';
import Typography from '@mui/material/Typography';
import FavoriteIcon from '@mui/icons-material/Favorite'
import ShareIcon from '@mui/icons-material/Share'
import VisibilityIcon from '@mui/icons-material/Visibility'
import theme from '../Theme'
import { useNavigate, useParams } from 'react-router-dom'
import { useState, useEffect } from 'react';
import NewsService from '../../services/NewsService';
import CardActionArea from '@mui/material';
import { TheaterComedySharp } from '@mui/icons-material';
import { useAuthContext } from '../../contexts/auth.context';
import { ClassNames } from '@emotion/react';
import TagService from '../../services/TagService';


interface NewsCardProps {
  id: number
  title: string
  authorName: string
  imageUrl: string
  likesCount: number
  viewCount: number
  datetimePosted: Date
}


const NewsCard: React.FC<NewsCardProps> = ({
  id,
  authorName,
  title,
  imageUrl,
  likesCount,
  viewCount,
  datetimePosted
}) => {
  const dateTime=new Date(datetimePosted);
  // console.log(datetimePosted);
  // console.log(dateTime);

  const { isAuthenticated, signout, user } = useAuthContext();
  const myid = user ? user.id : -1//ovde se menja nakon auth
  const [isLikedByMe, setIsLikedByMe] = useState<boolean>(false)
  const [likedCount, setLikedCount] = useState<number>(0)
  const [viewsCount, setViewsCount] = useState<number>(0)
  const [isHovered, setIsHovered] = useState<boolean>(false)
  let navigate = useNavigate()

  const linkToFullNews = async () => {
    navigate(`/newspage/${id}`)
  }

  const likeButtonClicked = async () => {
    if (isLikedByMe) {
      const oldCount = likedCount
      setIsLikedByMe(false)
      setLikedCount(likedCount - 1)
      const isOkay: boolean = await NewsService.DislikeNews(myid, id)
      if (!isOkay) {
        setIsLikedByMe(true)
        setLikedCount(oldCount)
      }
    }
    else {
      const oldCount = likedCount
      setIsLikedByMe(true)
      setLikedCount(likedCount + 1)
      const isOkay: boolean = await NewsService.LikeNews(myid, id)
      await TagService.LikeTagsFromNews(id);

      if (!isOkay) {
        setIsLikedByMe(false)
        setLikedCount(oldCount)
      }
    }
    // setIsLikedByMe(!isLikedByMe)
    // setLikedCount((isLikedByMe)? (likedCount - 1):(likedCount + 1))
    // const isOkay: boolean = (isLikedByMe)? 
    //     (await NewsService.DislikeNews(myid, id)) : (await NewsService.LikeNews(myid, id))

    // if (!isOkay){
    //   await setIsLikedByMe(!isLikedByMe)
    //   setLikedCount((isLikedByMe)? (likedCount - 1):(likedCount + 1))
    // }
  }

  const initialSetIsLiked = async () => {
    const { data, status } = await NewsService.IsNewsLikedByUser(myid, id)
    console.log(`For id ${id} result is ${data}`)
    if (status) {
      setIsLikedByMe(data)
    }
  }

  function handleMouseOver() {
    setIsHovered(true);
  }

  function handleMouseOut() {
    setIsHovered(false);
  }

  useEffect(() => {
    setLikedCount(likesCount)
    setViewsCount(viewCount)
    initialSetIsLiked()
    setTimeout(() => {
      console.log(`Image url is ${imageUrl}`)
    }, 3000)

  }, [])


  return (
    <Card
      style={{ backgroundColor: theme.palette.secondary.light, maxWidth: '700px', maxHeight: '550px', width: '100%', margin: '10px' }}
    >
      <div onMouseOver={e => { handleMouseOver() }} onMouseOut={e => { handleMouseOut() }} onClick={async e => {
        await linkToFullNews()
      }}>

        {(isHovered) ?
          <div style={{ backgroundColor: '#bdbdbd', cursor: 'pointer' }} className='flex flex-row'>
            <div className='flex items-center w-32'>
            <CardMedia
              component="img"
              height="200"
              image={imageUrl}
              alt={"NO PICTURE"}
              style={{ objectFit: 'cover', width: '120px', height: '120px' }}
            />
            </div>
            <div className='flex flex-col'>
            <CardHeader
              title={title}
              subheader={`${dateTime.getDate()}.${dateTime.getMonth()+1}.${dateTime.getFullYear()}`}
            />
            <CardContent>
              <Typography variant="body1" component="div">
                {`Author: ${authorName}`}
              </Typography>
            </CardContent>
            </div>
          </div>
          :
          <div className='flex flex-row'>
            <div className='flex items-center w-32'>
            <CardMedia
              component="img"
              height="200"
              image={imageUrl}
              alt={"NO PICTURE"}
              style={{ objectFit: 'cover', width: '120px', height: '120px' }}
            />
            </div>
            <div  className='flex flex-col'>
            <CardHeader
              title={title}
              subheader={ `${dateTime.getDate()}.${dateTime.getMonth()+1}.${dateTime.getFullYear()}`}
            />
            <CardContent>
              <Typography variant="body1" component="div">
                {`Author: ${authorName}`}
              </Typography>
            </CardContent>
            </div>
          </div>
        }



      </div>
      {/* <div
  onMouseOver={e => { handleMouseOver() }}
  onMouseOut={e => { handleMouseOut() }}
  onClick={async e => {
    await linkToFullNews();
  }}
  className='flex flex-row'
>
  {(isHovered) ? (
    <div style={{ backgroundColor: '#bdbdbd', cursor: 'pointer', display: 'flex' }}>
      <CardMedia
        component="img"
        height="200"
        image={imageUrl}
        alt={"NO PICTURE"}
        style={{ objectFit: 'cover', width: '120px', height: '120px' }}
      />
      <div>
        <CardHeader
          title={title}
          subheader={datetimePosted.toString()}
        />
        <CardContent>
          <Typography variant="body1" component="div">
            {`Author: ${authorName}`}
          </Typography>
        </CardContent>
      </div>
    </div>
  ) : (
    <div style={{ display: 'flex' }}>
      <CardMedia
        component="img"
        height="200"
        image={imageUrl}
        alt={"NO PICTURE"}
        style={{ objectFit: 'cover', width: '120px', height: '120px' }}
      />
      <div>
        <CardHeader
          title={title}
          subheader={datetimePosted.toString()}
        />
        <CardContent>
          <Typography variant="body1" component="div">
            {`Author: ${authorName}`}
          </Typography>
        </CardContent>
      </div>
    </div>
  )}
</div> */}
      <CardActions sx={{ backgroundColor: theme.palette.primary.main }}>
        <IconButton aria-label="add to favorites"
          style={{ color: theme.palette.primary.contrastText }}
          onClick={async e => {
            likeButtonClicked()
          }}>
          {(!isLikedByMe) ? <ThumbUpOffAltIcon /> : <ThumbUpIcon />}
          <Typography style={{ color: theme.palette.primary.contrastText, marginRight: '5%', marginLeft: '15%' }} variant="body2" component="div">
            {`${likedCount}`}
          </Typography>
        </IconButton>
        <Stack direction="row">
          <VisibilityIcon style={{ color: theme.palette.primary.contrastText }} />
          <Typography style={{ color: theme.palette.primary.contrastText, marginRight: '5%', marginLeft: '15%' }} variant="body2" component="div">
            {`${viewCount}`}
          </Typography>
        </Stack>

      </CardActions>
    </Card>
  );
};

export default NewsCard;



// const NewsCard: React.FC<NewsCardProps> = ({
//   title,
//   authorName,
//   imageUrl,
//   likesCount,
//   viewCount,
//   datetimePosted
// }) => {
//   return (
//     <Card sx={{ maxWidth: '700px', maxHeight: '550px', width: '100%', margin: '10px' }}>
//       <CardMedia
//         component="img"
//         height="200"
//         image={imageUrl}
//         alt={"NO PICTURE"}
//         style={{ objectFit: 'cover', width: '60%', height: '50%' }}
//       />
//       <CardContent>
//         <Typography variant="h6" component="div">
//           {title}
//         </Typography>
//         <Typography variant="body1" component="div">
//           {`Author: ${authorName}`}
//         </Typography>
//         <Typography variant="body1" component="div">
//           {`Likes: ${likesCount}`}
//         </Typography>
//         <Typography variant="body1" component="div">
//           {`Views: ${viewCount}`}
//         </Typography>
//         <Typography variant="body1" component="div">
//           {`Posted: ${datetimePosted}`}
//         </Typography>
//       </CardContent>
//     </Card>
//   );
// };

// export default NewsCard;
