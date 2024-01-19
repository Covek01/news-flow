import axios from "axios"
import {api} from "./Service"
import { error } from "console";
import Location from "../models/Location";

class LocationService {
    public async GetLocationsByPrefix(prefix: string){
        try{
            const {data, status} = await api.get<Location[]>(`location/getLocationsByPrefix/${prefix}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                const errObject: Location[] = []
                return errObject;
            } else {
                console.log('unexpected error: ', error);

                const errObject: Location[] = []
                return errObject;
            }
        }
    }


    public async GetLocationByName(name: string){
        try{
            const {data, status} = await api.get<Location>(`location/getByName/${name}`)

            console.log(JSON.stringify(data, null, 4));
            console.log('response status is: ', status);

            return data
        }
        catch (error) {
            if (axios.isAxiosError(error)) {
                console.log('error message: ', error.message);
                
                const errObject: Location = new Location(0, '')
                return errObject;
            } else {
                console.log('unexpected error: ', error);

                const errObject: Location = new Location(0, '')
                return errObject;
            }
        }
    }
}

export default new LocationService