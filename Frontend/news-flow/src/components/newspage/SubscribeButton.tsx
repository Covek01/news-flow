import React, { useEffect, useState } from 'react';
import {Button, Typography} from '@mui/material'
import theme from '../Theme';
import UserService from '../../services/UserService';

interface props{
    myid: number
    subscribedId: number
    isSubscribed: boolean
}

const SubscribeButton: React.FC<props> = ({myid, subscribedId, isSubscribed}) => {
    const [isSubscribedToUser, setIsSubscribedToUser] = useState<boolean>(isSubscribed) 
    const handleClickSubscribe = async () => {
        setIsSubscribedToUser(true)
        const okay = await UserService.SubscribeTo([subscribedId])

        if (okay < 0){
            setIsSubscribedToUser(false)
        }
    }

    const handleClickUnsubscribe = async () => {
        setIsSubscribedToUser(false)
        const okay = await UserService.UnsubscribeFrom([subscribedId])

        if (okay < 0){
            setIsSubscribedToUser(true)
        }
    }

    return(
        <>
        {(isSubscribedToUser)? 
        <Button style={{backgroundColor: theme.palette.primary.main}} onClick={async e => {
            handleClickUnsubscribe()
        }}>
            <Typography sx={{ fontWeight: 'bold', color: theme.palette.primary.contrastText }} textAlign="center">Unubscribe</Typography>
        </Button>
        :
        <Button style={{backgroundColor: theme.palette.primary.main}} onClick={async e => {
            handleClickSubscribe()
        }}>
            <Typography sx={{ fontWeight: 'bold', color: theme.palette.primary.contrastText }} textAlign="center">Subscribe</Typography>
        </Button>
        }
        </>
    )
} 

export default SubscribeButton