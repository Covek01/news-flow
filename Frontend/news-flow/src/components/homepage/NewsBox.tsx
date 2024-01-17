import React from 'react';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import { CardHeader, Avatar, IconButton, CardActions, Stack } from '@mui/material'
import ThumbUpIcon from '@mui/icons-material/ThumbUp';
import ThumbUpOffAltIcon from '@mui/icons-material/ThumbUpOffAlt';
import CardMedia from '@mui/material/CardMedia';
import Typography from '@mui/material/Typography';
import FavoriteIcon from '@mui/icons-material/Favorite'
import  ShareIcon from '@mui/icons-material/Share'
import VisibilityIcon from '@mui/icons-material/Visibility'
import theme from '../Theme'
import { useNavigate, useParams } from 'react-router-dom'
import { useState, useEffect } from 'react';
import NewsService from '../../services/NewsService';
import CardActionArea from '@mui/material';
import { TheaterComedySharp } from '@mui/icons-material';


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
  title,
  authorName,
  imageUrl,
  likesCount,
  viewCount,
  datetimePosted
}) => {
  const myid = 8 //ovde se menja nakon auth
  const [isLikedByMe, setIsLikedByMe] = useState<boolean>(false)
  const [likedCount, setLikedCount] = useState<number>(likesCount)
  const [isHovered, setIsHovered] = useState<boolean>(false)
  let navigate = useNavigate()

  const linkToFullNews = async () => {
      navigate(`/newspage/${id}`)
  }

  const likeButtonClicked = async () => {
      setIsLikedByMe(!isLikedByMe)
      setLikedCount((isLikedByMe)? (likedCount - 1):(likedCount + 1))
      const isOkay: boolean = (isLikedByMe)? 
          (await NewsService.DisikeNews(myid, id)) : (await NewsService.LikeNews(myid, id))

      if (!isOkay){
        await setIsLikedByMe(!isLikedByMe)
        setLikedCount((isLikedByMe)? (likedCount - 1):(likedCount + 1))
      }
  }

  function handleMouseOver() {
      setIsHovered( true );
  }

  function handleMouseOut() {
      setIsHovered( false );
  }


  return (
    <Card
      style={{backgroundColor: theme.palette.secondary.light,  maxWidth: '700px', maxHeight: '550px', width: '100%', margin: '10px'}}
      >
        <div onMouseOver={e => {handleMouseOver()}} onMouseOut={e => {handleMouseOut()}} onClick={async e => {
          await linkToFullNews()
        }}>
                {(isHovered)? 
                <div style={{backgroundColor: '#bdbdbd'}}>
                      <CardHeader
                          title={title}
                          subheader={datetimePosted.toString()}
                      />
                      <CardMedia
                        component="img"
                        height="200"
                        image={imageUrl}
                        alt={"NO PICTURE"}
                        style={{ objectFit: 'cover', width: '60%', height: '50%' }}
                      />
                      <CardContent>
                        <Typography variant="body1" component="div">
                          {`Author: ${authorName}`}
                        </Typography>
                      </CardContent>
                </div>
                :
                <>
                      <CardHeader
                          title={title}
                          subheader={datetimePosted.toString()}
                      />
                      <CardMedia
                        component="img"
                        height="200"
                        image={imageUrl}
                        alt={"NO PICTURE"}
                        style={{ objectFit: 'cover', width: '60%', height: '50%' }}
                      />
                      <CardContent>
                        <Typography variant="body1" component="div">
                          {`Author: ${authorName}`}
                        </Typography>
                      </CardContent>
                </>
                }
        </div>
      <CardActions sx={{backgroundColor: theme.palette.primary.main}}>
        <IconButton aria-label="add to favorites"
            style={{color: theme.palette.primary.contrastText}}
            onClick={async e => {
              likeButtonClicked()
            }}>
          {(!isLikedByMe)? <ThumbUpOffAltIcon /> : <ThumbUpIcon />}
          <Typography style={{color: theme.palette.primary.contrastText, marginRight: '5%',  marginLeft: '15%'}} variant="body2" component="div">
            {`${likedCount}`}
          </Typography>
        </IconButton>
        <Stack direction="row">
          <VisibilityIcon style={{color: theme.palette.primary.contrastText}}/>
          <Typography style={{color: theme.palette.primary.contrastText, marginRight: '5%', marginLeft: '15%'}} variant="body2" component="div">
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
