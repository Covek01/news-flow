import React, { useState } from 'react';
import { AppBar, Toolbar, IconButton, Typography, Box, Button, Avatar } from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import { SearchIconWrapper, StyledInputBase, Search } from "../search/Search";
import MenuIcon from '@mui/icons-material/Menu';
import { useNavigate } from 'react-router-dom';
import { lsGetUser } from '../../utils/helpers';
import { useAuthContext } from '../../contexts/auth.context';
const Bar: React.FC = () => {
    const { isAuthenticated, signout, user } = useAuthContext();

    let navigate = useNavigate()
    const handleClickTrending = () => {
        // Rutiranje na trending deo
        navigate("/trending")
    }

    const handleClickPersonal = () => {
        // Rutiranje na preference za korisnika
        navigate("/personal")
    }

    const handleClickNewest = () => {
        // Rutiranje na postove po odabiru
        navigate("/newest")
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
                <Box sx={{ display:'flex', alignItems:'center' /*{ xs: 'none', sm: 'block' }*/ }}>
                    <Button color="inherit" onClick={handleClickNewest}>
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">Newest</Typography>
                    </Button>
                    <Button  color="inherit" onClick={handleClickTrending}>
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">Trending</Typography>
                    </Button>
                    <Button color="inherit" onClick={handleClickPersonal}>
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">Personal</Typography>
                    </Button>
                    <Avatar 
                      alt={user?.name ?? "User Name"}
                      src={
                        user?.imageUrl && user?.imageUrl?.length > 0
                          ? user?.imageUrl.replace(
                              "background=311b92",
                              "background=fdd835"
                            )
                          : "https://ui-avatars.com/api/?background=311b92&color=fff&name=B+W&rounded=true"
                      }
                    />
                </Box>
            </Toolbar>
        </AppBar>
        </div>
    );
}

export default Bar;
