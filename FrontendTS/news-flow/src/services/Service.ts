import axios from "axios"




export default class Service{
    public axiosInstance: any

    constructor(){
        this.axiosInstance = axios.create({
            baseURL: `https://localhost:7005`,
            headers: {
              "Content-type": "application/json",
            },
          });
    }
}