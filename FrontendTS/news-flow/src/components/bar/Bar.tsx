import React, { useState } from 'react';
import { AppBar, Toolbar, IconButton, Typography, Box, Button } from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import { SearchIconWrapper, StyledInputBase, Search } from "../search/Search";
import MenuIcon from '@mui/icons-material/Menu';

const Bar: React.FC = () => {
    const handleClickTrending = () => {
        // Rutiranje na trending deo
    }

    const handleClickPersonal = () => {
        // Rutiranje na preference za korisnika
    }

    const handleClickDifferent = () => {
        // Rutiranje na postove po odabiru
    }

    return (
        <div style={{height: '63px'}}>
        <AppBar component="nav">
            <Toolbar>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <Typography
                        variant="h6"
                        component="div"
                        sx={{ display: 'flex', alignItems: 'center', mr: 2 }}
                    >
                        News Flow
                    </Typography>
                    <Search style={{width: '60%' }}>
                        <SearchIconWrapper>
                            <SearchIcon />
                        </SearchIconWrapper>
                        <StyledInputBase
                            placeholder="Search…"
                            inputProps={{ 'aria-label': 'search' }}
                        />
                    </Search>
                </Box>
                <Box sx={{ flexGrow: 1 }} />
                <Box sx={{ display: { xs: 'none', sm: 'block' } }}>
                    <Button  color="inherit" onClick={handleClickTrending}>
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">Trending</Typography>
                    </Button>
                    <Button color="inherit" onClick={handleClickPersonal}>
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">Personal</Typography>
                    </Button>
                    <Button color="inherit" onClick={handleClickDifferent}>
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">Different</Typography>
                    </Button>
                </Box>
            </Toolbar>
        </AppBar>
        </div>
    );
}

export default Bar;
