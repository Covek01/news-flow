import Bar from "../bar/Bar"
import * as React from 'react';
import NewsContainer from "../homepage/NewsContainer";
import News from "../../models/News"
//import { RedisClientType, createClient } from "redis"
import { hasOnlyExpressionInitializer } from "typescript";
import newsService from "../../services/NewsService"
import { Buffer } from "buffer";
import Redis from 'ioredis'
import { channel } from "diagnostics_channel";
import {Stack, Autocomplete, IconButton, TextField, Typography, Chip} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import {useState, useEffect} from 'react'
import Tag from '../../models/Tag'
import theme from "../Theme";
import TagService from "../../services/TagService";
import LocationService from "../../services/LocationService";
import Location from "../../models/Location";
import { useAuthContext } from "../../contexts/auth.context";
import UserService from "../../services/UserService";
import User from "../../models/User";
import UserWriter from "../../models/UserWriter";
import FilterAltIcon from '@mui/icons-material/FilterAlt';
import NewsService from "../../services/NewsService";


interface NewestProps{
    newsList: News[]
}

const newsData: any[] = [
    {
        title: 'SILOVAO TASTU ZVONOM ZA SURENJE SVINJA',
        imageUrl: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAoHCBcWFRgWFhUWFhgZHRwfHRocGSEjHh8lGB8aIxkfHCEdLi4lHSErJBkfJjgmKy8xNTU1HiU7QDs0Py40NTEBDAwMEA8QHhISGjQhJCQ0ND80NDQ0NDE0NDQ0NDQ0MTQ0NDQ0NDQ0NDQ0NDQxNDQ0NDQ0NDQxNDQ0NDQ0NDQ0NP/AABEIALcBEwMBIgACEQEDEQH/xAAcAAEAAgMBAQEAAAAAAAAAAAAABQYDBAcBAgj/xAA/EAACAgAFAQUGAwUHBAMBAAABAgARAwQSITFBBQYiUWETMnGBkaEHQrEUUmLB0SNygpKi8PFEstLhM0NzFf/EABgBAQEBAQEAAAAAAAAAAAAAAAABAwIE/8QAHxEBAQEBAAIDAQEBAAAAAAAAAAERAiExA0FREjIi/9oADAMBAAIRAxEAPwDs0REBERAREQEREBERAREQERMWLiBQWYgKASSeABuSYGSeTlLZ7O9sYmIuXxDl8oprXZFgixq0kM7sCDoDKqqRqskCYe8Hcs5HLnMJnMQupFD2YXUzHYa8IriKL66j85LcV12JwxO/GexPZoMWlIprVdfhIvxAeL+8KPFi9zM9ze1Mc5lVDlg5GpWY9ebu7at7JM4vyTcX+fGutxETRyREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBKv+I2Iy9mZrTsSmk/3WZVf/AEky0TV7Qya42E+E4tMRWVh6MCD894ED+HWAq9n4AWtwxav3y7e0+jWPlNX8S8yiZIq+q3ZQtea7km+BQP2lX7I7cxex3bK5xHbALE4eKq7Gz7y9GLe8yXqDaiNQYAaf4n95cvmEwzgZgMFB1JpcWcQDTytA0G5O1HqKnHW5kdT2q+UwyELC7arJ8gfCKHN7k15+gkv2R282UZXVUxMQmnDDgHbw0fCfX06yGx8YlwzEBQLC30r+u3z+um/Icb6rsdepFee1j5DzmE/Wr9A92+8GHnMPWlgjZkPKnqPUev6cSan5p7Pzz4TI+G7Iynemo9RYutQND04BE733b7aTMYS+JTihVLqNtyPeA6qeQRNuet8Vl1znpORETRyREQEREBERAREQEREBERAREQEREBERAREQEREBERARMOPjKilmICgWSZRu3O8j4gIw7RBdnqfienw+sluCx9o948HCtQ3tGHIXgfFuB9zK9mO+btejQnOxBJ+p2+0peazXwH/uRmLmiZztqrF2l3jzGJrw8dx7M7afZowceZNGvoCJXM8qIE9jp8JsUmoqRQBvEXY0ANuKmF8Vuk+Dit+8ZfI18nl1fEBfE2J3GhVv0GkgL14WX7tvsFM1ho+V0K+GiqUBBV1TYLfKuLoHcHYGqBFHGCOoBE+cFnwW9pguykHgMb+n5hJ/M/F2tdsIaXPADAj42PPcckSyd1O2Hy+bwybVAqIy+hvV9Bv8dNbSFzeZGI+rSELm2AIotyzL8SAT5FvImbmbVdPtFNthquodd/dI9RsPPjrMrsrSeY/QcSs9we1jmMmjMbZbQnqdNUT8iJZpvLs1lZlx7ERKhERAREQEREBERAREQEREBERAREQEREBERAREQKB+IeedcTBw9LHBNF69Sw+1D6yq5/NYSlkR2AABrQelbEjbqPtLZ3qfVmGU9Ao862B49LJlT7TxCEZ/ZaiASNS2N6tnB947mlFDjrZmdm10r+NnVPW/hMAzAbgTezeZClB+zpoPLnCTW9c0Auld9htdVPtS4JJyaqlWuHoO/ABLtwd+lbCdyOWph4Lt7qXN89g46qruiIrEKGZ1As8cnb4zz2rqaXLjWNyoRtNb+patj16dJYcLOa0RPZOFVrcglyQQCNKsCqm+CBsPrOsRoY/dDNoupsNNPmHU/oZCZjDZSVOjUPy6vEd62A55luzeKzgKmG6EbEvbhRe2kvfi6HfbYgcCajZd6UB3RCQWp6YjbhuR6L9ZcFExsu1h1FlTYre/Tbzkp2Y1u5aqdaIIoHUtC/KiBt0+szZhcdsU6cZx4tArFJIVlsGr6rvv5TBmuznR30AsqgH3rIDDxEnrupJNfm8pl3zs1pzfpbu4Xapw80mEt+yxlvccklqPowI0mdbnB+72bCHBonWrF0PSwq0rejcehA8jO5ZbGDIrDhgD9Rc5+O+4dz7Z4iJq4IiICIiAiIgIiICIiAiIgIiICIiAiIgIiICfDGgTzPuaPbGOUwMRxsQpr4nYfcwOc4mZDs7OfESSTV8nah134HpZoSK7bzKHQqU+i2fX7gOwXce843pVPJo9QZHAUMGdlsL61Z3pb9ZB5nLongKGxZama9Wqvjx+szi1HP2noLUBTEUCvj63Sg0Ab63044n2vaJC6Aq6zYO7Wtn85J0A+gF7ecye1XUG9n4gAvOxN8n/AH5QMTCRr9kD4h/Z6vDweDV16G5pHL7TOaxoUoqgHViNejyumJF1sNIHXiS2XxsZ11I+GioGNsx1G/3VGogk1sdIN3vIzLZnLu6YWldL6aLkhB6KwAVmPSjufO5LZHs4NrUYQcId7Z7HwVASx2IoAzoZwM0+ohUGkHW4YGulHSfD8C1jqALEjznMUNqbR7MEDZd3IIsoOSLF80ZmONhsuG+GqPg2Rr12FYja66iuD/SZFzILshRPCNmNm+oI08bHg9fPkhAYePiI7LqBZyCNTGkG/icrQ8jpsAb+pm42dxR/aIqkjYADdlAOosOgc7BaHn5z7yecXWV9jqYNStVgjezxsLN2b9egkgXo3oUUAaoaTQN15dd5KsVrNZB8vmQjKVRfEhIq1saSvmRv8wQeZ1buD3iGawihAV8Lbbhl/KR5VsCP5ESifiHmS37E9+GsVb+PsyOPW5p9zO1/2fE9ob2JBHQqxG/oRt8phf8Anpr/AK5d3iY8LEDAEbggEfA8TJNmRERAREQEREBERAREQEREBERAREQEREBERA8lf7542nL6f32UfS2/VRLBKf31xgWw08hq/wAxAH/afrOb6WKfnsw2kUQpDX6k1X2H6yvZrMlaJUtrdU6UNZoFjd7/AAP3ltxMoMQ4KckvbLVWurxC/OhUqHb9OuLXhYDUoHFpTCh/hqTkrW7SzDa8OywUtpZduoIQ7b+961NPtMEMikEDExFDX1B5HwPHwubHaCI2AzhmLhQ6tQqxTLWnwg9OJsY+VXGRTZQ7MpII0sN18JrjcTSI28LDVldGqnRk3HnX8wD8RLX+GGOWyy4j+IlW1E3v7NmUEnqaUb9TKk2FiOm4VCdtWokC9iVGxPoDW/XaW7sPMpg4KYOHRGkoqk024NsaG5s6jwLJ44lRt/siJhjCCKgxArGgB4yi6m25Y3z/AADpKHi4bPm8I6iUIxdG9BlUqoe/ViSD5KhHMtXa6DECLhF8Nw/ibUKKViI+hhvZDkX0q+lmGz2oZpGCMqJgsitp8AOtTQPA8K15eUg1CPZuh0sdbFa1EHhm1bkLsFNj15Ek8DNa7CiiPe2ujQ2K8jzryIPXeIz7O+OFTc4KMeLBbGNAUP4FY30vrwdzs1XGGhcBWajR2awQBv12IHHQ+somu28IYnZbFltsJ0ZT5U4Qn/JiH069JQsFW0LqBUahTDYbbA7b1xZ6fSdS7p5dXwsTAe2RyyNzdOlH7zlr2PACfDd/z289vsZh8k8teHYvw97cOKhwHYF8NVrz0igfpt8NUus4T3VzrZPNasUaCjHDcE3QsLzwR+YH+u/dVN7y/HfGfjnqedfURE0ckREBERAREQEREBERAREQEREBERAREQE513ux9WZbfZAF/wBN/qZ0Scwz/wDaYrsCN3JH+ImvtU46WNT2jYbo+7KAQN63INc9L/WVLtDEJ1EiiSdvjcsuYANpZ1NbKD5Ac/e5WM1mAwI0m7rcbRyVo5YBVAWxXFkmq+P8pI4LggXtvzfH1kfpJ4ofGbGWwHvZk2B5Q/8AlNIidwlYopBsf06/aSHZ7o+IqXdNoICkiyuvTfnp34qjV3Uh8PEdULNoehsEUrwOCCzb/SbXdzMoMRMXWHXMKoxCt1lsSlXV4b0jT4N9yV5CsYvpE5jJ42KJtTVdiq4269fr8JH4is27nfTR8t76eUvOcyAxcPWjhNJIDMpbUCpsEkg9ebrmUbOM6sVvDautEX163EK0DhjWxw6R2AtqsNQ21rtqo9dj6zDmFP52sj3mAq/MKPlx/wAzcwFYEligJ3Gk2fXkAL9/l1+cYAcKL5J5Jrkkn/iUT/cvMhS1AhbUj5H7yvd9+yFyubxGT3MUe0UH8pbVrUeY1WfQNXSTfd004r90XXz85Kd/+7+JmsvhY+EbbBVtSVuytp1afVdFgdd+vOfc2O+blcuzLOyo7ratSM3IJw0Apx0bTv6gHmjXb+5PaS42VQA2cKsMm7vQAAfmP5ziL4jNaIfCVVgOhK6wL+TML/i9Ze/wizxBfCJvUBR8iuq/rufiZlzcsd9Tw6vERN2RERAREQEREBERAREQEREBERAREQEREDU7Rx9GFiP+6rEfEA195zLKZpUUgi2BJurBNy+d7sfTln/iKr97P2BnOMPDbSWFk3pA+/8AL9Zx17WHbeV8CYiuNVAEHyYcCuKr7nyqVTEYnVe3iJ9OTQEsXaRsjxUFscetHjc8Sudq4Xh26sAfQM6qfs0vMK10xEY1YJ6DzrmpIZOuLNkGuN6q6+o+s1W8LIpAtid793St7fLb5zxwVrFCkjDb3gRWmiMXa7vc9PyCdxEprGEwBJbVwCDvp5qgbrr6Sd7K7YS1dW2bqp1LXUKBdeXp6VILMoxxMDRpLf2tBj4SQgNEjgHzkl3eyqnL4mJqOr+0Y+GmD+I4yEb0AfCBfr6yo6DhvWWZVclmBNdeADXnVdOJQs9jYVsGxERl5BYArfGqzsKlh7WxsTDwtNvq8Lrt7ukDcE9DsP8AkynjM4jZjMFAo1JhXfTbEA9D1khWdAgJUupKcgGyt8E7kj026TKMwjDwW48xW/w85F9oqqOccHWEVAVG+pXbE1rXmNKsPVa6zb7Jxw6I6WFIatuaY8CUTPYWOuvfbbj7i/j/ADnSexcTVhj0J++/85ynItoxiCQSQDt09PjOld2sW1YfA/yMlHL++Pd9snmC4o4GKWKGvdJslDXGkcHqB1ozB3UzgwmXMILZG8a9SpJBr/UPid517vH2Quay74LGiaKtXusu6n4dCOoJHWcNyqvgYjIy6HW7VxwaujxdqR5WNJ+Pn75y7G3PWzH6HE9kb2Fnxj4GHidWXfbqNm+4klNpdmsq9iIlCIiAiIgIiICIiAiIgIiICIiAiJhzOOqKzsaCizAqnfrMgqmGNyDqYeV7Lf3/ANmU32hDDRwKPrdb/e5J9o+1xHZi2gvwSaU3QABPh2AoWRwJp42Gbv2Yc1dmh15BWww25vpM92ukPndQ8Z234/r/AL85H466wwKtutVXUkWNvQcyfGGoB8D+fnvvQHU+cLhqwJYOukXuPIdLP8p1EVzL9n0wZ3dyLC6iDpDc9OSALPPrNnL5ZFTQMMaTdqfU73vJPAyCuFYMx530bdOd/jxMr5CuXX4FT0naNPK4OGSo0KCnubbgH92+PlJfKLhqzEgDXs9Luw/jH5tgB8Jqvg4YKeNAdXW+n6cTY9phhj41sDi//UqJDP5gumgDVV6dItTey7dCPERYuQb5UCyy6HYBSwZhde6DRHF/f1kgvaKCwMY6drAs0B7vHHxmDFzeGfFqZ/gl8V5tBUY3ZaOrJT6iqqKJAAU2vh92wevNEjrNjI9lFMJcNDQW+epJs8erHbpc2R2guqlTEY6aI0DbVp53NWAelzZy+ZY//U6eril282oAQNbIdnaCC3rt133Pwl37tY2lt6AIqvLivvKviF+TpA8xd/Ib+fWbeQ7Q0e+wvooNn5yK6ROU/ix2UUxMPMpsHAw38tQvQfOypYX/AACdH7KzYdBv4gN/l19ZB/iRo/YMTWwUgqUJH5lNr8Ngd/K5x1Njrm5VQ/DzvAyYq4LG0ckH+EkEr+lfAjynWp+bclndDhwRZNbHYWP989QJdst33xkYuMcMpUf2eKt0TW+oUQLvaze3Ey56/nxXXXO+Y65E5v2P+ITnFRMdUK4jaQyWCp9V8Vj5+fwnSJrOpfTiyx7EROkIiICIiAiIgIiICIiAiIgeSm96O0DiE4eESwQ+MDqeh9QKPpfwm13v7wnLezwkoYmKSNTcIo95v7x4W9r3N1RrRyDOA+G2lloCyRuQPdI3rYefHTecdX6WJbs3GGnSjsj6dlYdB1IOzC/Kxt8ZEdsAq5D5fVydSeAmrO9enmPPymLO5rGW1xsNH0csyUw2H50IAnqdqIy+JMQgcaXLj4+Ir8OJwqIz+NpojDIPOkuR5caNP6GauFnyDfsnBojbENb/AN8GedrZp2cEYTsAfDxqob14fjPcHtRArB0x0Yivc8J87O5P0mnPpKyN2xo2OHmDXXwn6T1u3BRC4WIxogFlUj9TtPjDxkNEMw45uqsH03+ImXPOjuWQ7bdKHr/up0jFl86CAWRlb/8ABP1m6mIcQ0iE11bDTk+pPJM0VcVwx+n9ZmTE/hbb4SokEw3sEqFI/uAm+L3b9J8Zh8QXYYXZBXHA28vCD9D8+JqNjXZ3HyPT4TXOOBWzm+oQ1/qIP0lEh7fWdJYG6rXbcbdDv/OehAh8KqD+8FH2LbiaS6/yYbG+WI/QcCfaYGKeQoP98XA2D4vedyRxbUP9/Dyn0uEqEGwOvlxfN7/aeYfZzfnxKHUAE39aH3gJgA1pV2HF78eQ6/WQTnYnbwDAi9I4Y7Dg3Q6z7/FPLPjZXDxU1FEZtYHQMtBiP4SKvpqlXzBxDqNKmGALa7bbgdABsOKFH6WDuv2mcbHGXOplIdGB/MoXfVwRRIo7UWI6ic9TZjqXK51g4fXSu2//ADtt/wAec+jmNYoYRJG2oDm/I9SfvXxJmu2O4GYwMUhFbFw2YaGWz18IcE7NXO1Hoegju2OzsfLKiuAjvuFaiwXjUaJ0gmwBYO0ws8tZWNMdEoaACeSVN7dBVE8HqB6GdU/DjPNiJiqcT2iYbIEPlqW2W+oE5T2Ojswx3N2QFOoUp2rbzv06TpX4d9toEbAekbVYJ2u/DRPoVoHyr0l58dJ15joURE3ZEREBERAREQEREBERAREQIDvP3eXNqtnS63pb49PT7/Ayo4PZ+Pk7DAOlitVrVeTi0+Gp1Irjczps8nNmrrkz9sId8TDfURylaOtC735O9nmRWZzSqhKqxYjYth3Ro8EWo/8AU6xnu7mVxiWxMBCx/MBpb/MtN95AZn8Ocu1nDxs1gn+HF1f94Y/eScmuW42aZRftWvY07X8lB4+k+8t2jib6tDXxYUfH3al8zH4bYt2meJrgPgq31N/ykbmfw6zl2r5N68wyk/IKRc6xEGvaB6qo/wAR/wDITz9rYhdKBrF+K9h63e+/ElcTuPnV/wCmwHPmuJp/Uj9JrP3Wz3H7E4/u4+GR9Dcojcx2hpNBQWFWAEr4KAN/1m1l8Vy9lE0nrpAr/KeZ6vdTOj/o2O/7yfy5m3hd388DtlmHzA/WVHy+O490I3ze/sdp4mZxhwUXz2atvVjYkk/YOdI2yzg+athj/u3ng7o5tgAcup6kO61Z89DWfgRtA0P2gq1F8Ib9dPz53nj9oqFtsdQpH5RZHI4F7bDp1EmsHuNmui5LC62uon/Up/WbuX/DzEu3zmmzZ9lhKh/zKQZdFVSiNYTMYl/mc6F4ve6ofHb1nx//AFQE0r7HCuzs5dyfTQCoPG7N042l/wAr+HeUWjie1xiOrv8A+NH7ywZDsXL4P/xYGGh8wo1fXn7yaY5dgdgZvNDw4bKCdsTGBRQCPeXD5LeRpq6ES/d0+6mFkkOk68RgA7kbmuij8q9a895ZIkV5OR/ipkGXMLjVaNhjnjUjDUPTw0fnOuSI7xdi4ebwWwn26qwG6t0P9R1E46mzw65uV+eMNNGkEkjVdDpV7n5k/WWPLdoB9Jr3ldCE3vQgZStci1I/xCuZYsL8NcdnJZ8JV38QJJPkQtDmhyev1tXYXcTL4DK5LYjqbBIAHQ8b3vvz5eUxnHV9xpeuYlO6WPjPlcJscePSPFqDa1oFHsdSCL9b6UZNz5VQAABQHSfU9EmRlXsREqEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERA//Z',
        authorName: "Jovan Jovanovic"
    },
    {
        title: 'SURIO TASTU ZVONOM ZA SILOVANJE SVINJA',
        imageUrl: 'data:image/png;base64,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',
        authorName: "Jovan Jovic"
    },
    // Add more news items as needed
  ];



type ReturnTypeForNews = {
    title: string,
    text: string,
    summary: string,
    imageUrl: string,
    authorId: number,
    locationId: number
}

const Newest: React.FC = () => {
    const { isAuthenticated, signout, user } = useAuthContext();
    const myid: number = user?.id ?? -1
    const [newsToShow, setNewsToShow] = React.useState<News[]>([])
    //const [redisClient, setRedisClient] = React.useState(null)

    // const redisClient: Redis = new Redis({
    //     host: 'redis-13049.c304.europe-west1-2.gce.cloud.redislabs.com',
    //     port: 13049, 
    //     password: 'SqigRXvQ2D42QbxKJnxiTHXaKOeXXrMH',
    // })

    const channelForNewestNews = 'newestnews'


    const [tags, setTags] = useState<Tag[]>([]);
    const [locations, setLocations] = useState<Location[]>([]);
    const [selectedTag, setSelectedTag] = useState<string | null>(null);
    const [chosenTag, setChosenTag] = useState<Tag | null>(null);
    const [selectedLocation, setSelectedLocation] = useState<Location | null>(null);
    const [selectedWriter, setSelectedWriter] = useState<UserWriter | null>(null);
    const [writers, setWriters] = useState<UserWriter[]>([]);
    const [selectedTags, setSelectedTags] = useState<Tag[]>([])     //used to create news
    const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false)

    
    const getTags = async (input: string | null) => {
        if (input === null){
            input = ""
        }
        return await TagService.GetTagsByName(input)
    }
    
    const getLocations = async (input: string) => {
        return await LocationService.GetLocationsByPrefix(input)
    }

    const getWriters = async (input: string) => {
        return await UserService.GetWritersByPrefix(input)
    }

    const insertListedTag = async (tag: Tag | null) => {
            if (chosenTag === null  || tag?.name === ''){
                return
            }

            const t: Tag = new Tag(tag?.id ?? -1, tag?.name ?? '')
            const arr = selectedTags.filter(x => {
                return t.id === x.id
            })
            if (arr.length !== 0){
                return
            }
            setSelectedTags([...selectedTags, t])
            console.log(selectedTags)
            setChosenTag(null)
    }

    const handleDeleteTag = async (id: number) => {
        setSelectedTags(selectedTags.filter(tag => 
            tag.id !== id
        ))
        console.log(`Tag is deleted, now selected tags are ${JSON.stringify(selectedTags)}`)
    }

    
     const initalizeNews = async () => {
        let news: News[]
        try{
            news = await newsService.GetNewestNews()
        }
        catch(error: any){
            console.log('unexpected error in getting newest news: ', error)
            news = []
        }
    
        const newsObject: News[] = news.map((x) => {
            return new News(
                    (x as any).id,
                    (x as any).title,
                    (x as any).imageUrl,
                    (x as any).authorName,
                    (x as any).summary,
                    (x as any).text,
                    (x as any).authorId,
                    (x as any).viewsCount,
                    (x as any).likeCount,
                    (x as any).postTime
                    )
        })

        setNewsToShow(newsObject)

        // redisClient.subscribe(channelForNewestNews, (err, count) => {
        //     if (err) {
        //         console.error("Failed to subscribe: %s", err.message);
        //     }
        //     else {
        //         console.log(
        //             `Subscribed successfully! This client is currently subscribed to ${count} channels.`
        //           );
        //     }
        // })
        
        // redisClient.on("message", (channel, message) => {
        //     console.log(`Received ${message} from ${channel}`);
        //     const receivedMessage : News = JSON.parse(message)
        //     setNewsToShow([receivedMessage, ...newsToShow])
        //   });
        
    }

    const filterNews = async () => {
        const tagIds = selectedTags.map(tag => tag.id)
        const news = await NewsService.GetFilteredNews(tagIds, selectedWriter?.id ?? 0, selectedLocation?.id ?? 0)
        setNewsToShow(news)
    }

    React.useEffect(() => {
        initalizeNews()
    }, [])

    return (
        <div>
            <Bar />
            
            <div style={{backgroundColor: theme.palette.secondary.light, marginLeft: 'auto', marginRight: 'auto', marginTop: '3%', maxWidth: '750px', width: '90%', border: `2px solid ${theme.palette.primary.light}`}}>
            <div style={{marginLeft: 'auto', marginRight: 'auto', width: '80%', marginTop: '3%'}}>
               
                <Autocomplete   
                    style={{marginTop: '1%', marginBottom: '1%', width: '40%',  maxWidth: '300px'}}
                    options={locations.map((location) => location.name)}
                    getOptionLabel={(option) => option}
                    value={selectedLocation?.name ?? ""}
                    onChange={async (_, newValue) => {
                        const loc = await LocationService.GetLocationByName(newValue ?? "")
                        setSelectedLocation(loc)
                    }}
                    onInputChange={async (_, newInputValue) => {
                        const newLocations = await getLocations(newInputValue);
                        setLocations(newLocations);
                    }}
                    renderInput={(params) => (
                    <TextField {...params} label="Location" variant="outlined" />
                    )}
                />
                
                <Autocomplete   
                    style={{marginTop: '1%', marginBottom: '1%', width: '40%',  maxWidth: '300px'}}
                    options={writers.map((author) => author.name)}
                    getOptionLabel={(option) => option}
                    value={selectedWriter?.name ?? ""}
                    onChange={async (_, newValue) => {
                        const loc = await UserService.GetWriterByName(newValue ?? "")
                        const val = (loc.length > 0)? loc[0] : null
                        setSelectedWriter(val)
                    }}
                    onInputChange={async (_, newInputValue) => {
                        const newWriters = await getWriters(newInputValue);
                        setWriters(newWriters);
                    }}
                    renderInput={(params) => (
                    <TextField {...params} label="Writer" variant="outlined" />
                    )}
                />
                 <Stack direction="row">
                    <Autocomplete
                        style={{width: '40%', maxWidth: '300px'}}
                        options={tags.map((tag) => tag.name)}
                        getOptionLabel={(option) => option}
                        value={selectedTag}
                        onChange={async (_, newValue) => {
                            const newTags = await getTags(newValue);
                            await setChosenTag(newTags[0])
                            console.log(`Chosen tag is {id:${newTags[0].id} name:${newTags[0].name}}`)
                        }}
                        onSelect={ (_) => {

                            const a = 5
                            const b = 6
                        }}
                        onInputChange={async (_, newInputValue) => {
                        const newTags = await getTags(newInputValue);
                        setTags(newTags);
                        }}
                        renderInput={(params) => (
                        <TextField {...params} label="Tag" variant="outlined" />
                        )}
                        autoComplete
                    />
                    <IconButton color="inherit" onClick={e => {
                        insertListedTag(chosenTag)
                    }}
                        style={{ height: '50%', width: '20%',  maxWidth: '150px', borderRadius: '5%', backgroundColor: theme.palette.primary.main,
                        color: theme.palette.primary.contrastText, alignSelf: 'center',  marginLeft: '4%'}}>
                        <AddIcon />
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">ADD</Typography>
                    </IconButton>
                </Stack>

                <Stack direction="row" spacing={1} style={{marginBottom: '5%', marginTop:'1%'}}>
                    {selectedTags.map(tag => {
                        return <Chip key={tag.id} label={tag.name} onDelete={e => {
                            handleDeleteTag(tag.id)}
                        } />
                    })}
                </Stack>
                <IconButton color="inherit" onClick={async e => {
                        await filterNews()
                    }}
                        style={{ height: '50%', width: '20%', marginBottom: '2%',  maxWidth: '150px', borderRadius: '5%', backgroundColor: theme.palette.primary.main,
                        color: theme.palette.primary.contrastText, alignSelf: 'center', marginTop: '4.5%', marginLeft: '4%'}}>
                        <FilterAltIcon />
                        <Typography sx={{ fontWeight: 'bold' }} textAlign="center">FILTER</Typography>
                </IconButton>
            </div>
            </div>
            <NewsContainer newsList={newsToShow}/>
        </div>
    );
}

export default Newest;