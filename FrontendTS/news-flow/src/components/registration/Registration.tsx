import {
    Box,
    Button,
    Checkbox,
    Grid,
    Stack,
    Switch,
    TextField,
    Typography,
  } from "@mui/material";
  import axios from "axios";
  import { useCallback } from "react";
  import { useForm } from "react-hook-form";
  import { Link, useNavigate } from "react-router-dom";
  import { api, constants } from "../../constants";
  import User from "../../Types/User";
  
  interface FormInputs {
    name: string;
    password: string;
    email: string;
    phone: string;
    repeatPassword: string;
    role: string;
    city: string;
    country: string;
    isJournalist: boolean
  }
  
 

const Registration: React.FC = () => {
    const {
      register,
      handleSubmit,
      getValues,
      formState: { errors },
    } = useForm<FormInputs>({
      reValidateMode: "onChange",
    });
    const navigate = useNavigate();
  
    const onSubmit = (data: FormInputs) => {
      console.log(data);
      let exampleUser: User & { password: string } = {
        name: data.name,
        email: data.email,
        phone: data.phone,
        password: data.password,
        country: data.country,
        city: data.city,
        role: (data.role === "User") ? "User" : "Journalist",
        isJournalist: data.role === "Journalist" 
      };
      console.log(exampleUser);
  
      api
        .post("/user/signup", exampleUser)
        .then((response) => {
          return navigate("/signin");
        })
        .catch((error) => {
          console.error(error);
        });
    };
  
    const repeatPasswordValidator = useCallback(
      (value: string) => getValues("password") === value,
      [getValues]
    );
  
    return (
      <Box
        sx={{
          height: "100vh",
          p: "1em",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        <Stack
          component="form"
          onSubmit={handleSubmit(onSubmit)}
          sx={{
            display: "flex",
            gap: "0.6em",
            width: { xs: "100%", sm: "80%" },
            maxWidth: "600px",
          }}
        >
          <TextField
            label="Name"
            className="form-field"
            error={Boolean(errors.name)}
            helperText={errors.name && "Name field is required"}
            {...register("name", { required: true })}
          ></TextField>
          <TextField
            label="Email"
            type="email"
            className="form-field"
            error={Boolean(errors.email)}
            helperText={errors.email && "Email is required"}
            {...register("email", { required: true })}
          ></TextField>
          <TextField
            label="Phone number"
            className="form-field"
            error={Boolean(errors.phone)}
            {...register("phone", { required: false })}
          ></TextField>
          <TextField
            label="Password"
            type="password"
            className="form-field"
            error={Boolean(errors.password)}
            helperText={errors.password && "Password is required"}
            {...register("password", { required: true })}
          ></TextField>
          <TextField
            label="Repeat password"
            type="password"
            className="form-field"
            {...register("repeatPassword", {
              required: true,
              validate: repeatPasswordValidator,
            })}
            error={Boolean(errors.repeatPassword)}
            helperText={errors.repeatPassword && "Passwords don't match"}
          ></TextField>
          <Box display="flex" alignItems="center">
            <Typography>Basic profile</Typography>
            <Switch {...register("isJournalist")}></Switch>
            <Typography>Company profile</Typography>
          </Box>
  
          <Button type="submit" variant="contained" color="primary">
            Sign Up
          </Button>
          <Grid container justifyContent="flex-end">
            <Box>
              Already have an account? <Link to="/signin">Sign In</Link>
            </Box>
          </Grid>
        </Stack>
      </Box>
    );
  };

  export default Registration