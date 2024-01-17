import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    primary: {
      light: '#4791db',
      main: '#1976d2',
      dark: '#115293',
      contrastText: '#fff',
    },
    secondary: {
      light: '#e0e0e0',
      main: '#9e9e9e',
      dark: '#424242',
      contrastText: '#000',
    },
  },
});

export default theme