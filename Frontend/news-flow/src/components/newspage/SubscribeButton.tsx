import React, { useEffect, useState } from 'react';
import {Button, Typography} from '@mui/material'
import theme from '../Theme';
import UserService from '../../services/UserService';


interface props{
    myid: number
    subscribedId: number
    isSubscribed: boolean
    updateSub: any
}

const SubscribeButton: React.FC<props> = ({myid, subscribedId, isSubscribed, updateSub}) => {

    

    const handleClickSubscribe = async () => {
        updateSub(true)
        const okay = await UserService.SubscribeTo([subscribedId])

        if (okay < 0){
            updateSub(false)
        }
    }

    const handleClickUnsubscribe = async () => {
        updateSub(false)
        const okay = await UserService.UnsubscribeFrom([subscribedId])

        if (okay < 0){
            updateSub(true)
        }
    }


    return(
        <>
        {(isSubscribed)? 
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