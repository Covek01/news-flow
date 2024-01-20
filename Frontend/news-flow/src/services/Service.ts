import axios from "axios"
import { lsGetSession } from "../utils/helpers";
import { error } from "console";

export const constants = {
  apiName: `https://localhost:7005`,
  newsUploadName: `https://localhost:7247`
};

const axiosInstace = axios.create({
  baseURL: constants.apiName,
});


const axiosInstance2 = axios.create({
  baseURL: constants.newsUploadName,
});



axiosInstace.interceptors.request.use(
  (config) => {
    let session = lsGetSession();
    if (session) {
      // if (new Date(session.expires) > new Date()) {
      //   lsRemoveSession();
      //   return config;
      // }

      config["headers"] = config.headers ?? {};
      // @ts-ignore
      config.headers["Authorization"] = `SessionId ${session.id}`;
      console.log("interceptor");
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

axiosInstance2.interceptors.request.use(
  (config) => {
    let session = lsGetSession();
    if (session) {
      config["headers"] = config.headers ?? {};
      config.headers["Authorization"] = `SessionId ${session.id}`;
      console.log("interceptor");
    }
    return config;
  }, (error) => {
    return Promise.reject(error);
  }
);

export const api = axiosInstace;
export const upService = axiosInstance2;
// export default class Service{
//     public axiosInstance: any

//     constructor(){
//         this.axiosInstance = axios.create({
//             baseURL: `https://localhost:7005`,
//             headers: {
//               "Content-type": "application/json",
//             },
//           });
//     }
// }