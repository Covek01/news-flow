import { AppBar, Toolbar, IconButton, Typography, Box } from "@mui/material"
import * as React from 'react';



const Bar = () => {

    return (
        <AppBar component="nav">
            <Toolbar>
                <IconButton
                    color="inherit"
                    aria-label="open drawer"
                    edge="start"
                   
                    sx={{ mr: 2, display: { sm: 'none' } }}
                >
                   
                </IconButton>
                <Typography
                    variant="h6"
                    component="div"
                    sx={{ flexGrow: 1, display: { xs: 'none', sm: 'block' } }}
                >
                    MUI
                </Typography>
                <Box sx={{ display: { xs: 'none', sm: 'block' } }}>
       
                </Box>
            </Toolbar>
        </AppBar>
    );
}

export default Bar

