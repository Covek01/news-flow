
import React from 'react';
import { Card, CardContent, CardMedia, Typography, Box } from '@mui/material';

interface NewsBoxProps {
    title: string;
    imageUrl: string;
    authorName: string;
  }

const NewsBox: React.FC<NewsBoxProps> = ({ title, imageUrl, authorName}) => (
  <Card sx={{ maxWidth: '700px', maxHeight: '550px', width: '100%', margin: '10px' }}>
    <CardMedia
      component="img"
      height="200"
      image={imageUrl}
      alt={"NO PICTURE"}
      style={{ objectFit: 'cover', width: '60%', height: '50%' }}
    />
    <CardContent>
      <Typography variant="h6" component="div">
        {title}
      </Typography>
      <Typography variant="body1" component="div">
        {`Author: ${authorName}`}
      </Typography>
    </CardContent>
  </Card>
);

export default NewsBox